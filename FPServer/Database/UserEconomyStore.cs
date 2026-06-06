using FPServer.Game;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace FPServer.Database
{
    public sealed class UserEconomyStore
    {
        private readonly GameEconomy _economy;
        private readonly ILogger<UserEconomyStore> _logger;

        public UserEconomyStore(GameEconomy economy, ILogger<UserEconomyStore> logger)
        {
            _economy = economy ?? GameEconomy.Default;
            _logger = logger;
        }

        public async Task<int> TryGrantWeeklyReliefAsync(int userId, int currentBeans)
        {
            if (currentBeans >= _economy.WeeklyReliefThreshold || _economy.WeeklyReliefBeans <= 0)
                return currentBeans;

            int affected = await DbHelper.Instance.ExecuteNonQueryAsync(
                @"UPDATE users
                  SET beans = beans + @amount, weekly_relief_at = NOW()
                  WHERE id = @id
                    AND beans < @threshold
                    AND (weekly_relief_at IS NULL OR weekly_relief_at <= DATE_SUB(NOW(), INTERVAL @cooldownDays DAY))",
                new MySqlParameter("@amount", _economy.WeeklyReliefBeans),
                new MySqlParameter("@id", userId),
                new MySqlParameter("@threshold", _economy.WeeklyReliefThreshold),
                new MySqlParameter("@cooldownDays", _economy.WeeklyReliefCooldownDays));

            if (affected > 0)
            {
                int newBeans = currentBeans + _economy.WeeklyReliefBeans;
                _logger.LogInformation(
                    "Weekly relief granted to user {UserId}: +{Amount}, beans {Before}->{After}",
                    userId,
                    _economy.WeeklyReliefBeans,
                    currentBeans,
                    newBeans);
                return newBeans;
            }

            return currentBeans;
        }

        public async Task ApplySettlementAsync(IReadOnlyCollection<GameSettlementDelta> deltas)
        {
            if (deltas == null || deltas.Count == 0)
                return;

            using var connection = await DbHelper.Instance.GetConnectionAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                foreach (var delta in deltas)
                {
                    using var command = new MySqlCommand(
                        @"UPDATE users
                          SET beans = GREATEST(0, beans + @beanDelta),
                              win_count = win_count + @winIncrement,
                              lose_count = lose_count + @loseIncrement
                          WHERE id = @id",
                        connection,
                        transaction);

                    command.Parameters.AddRange(new[]
                    {
                        new MySqlParameter("@beanDelta", delta.BeanDelta),
                        new MySqlParameter("@winIncrement", delta.IsWinner ? 1 : 0),
                        new MySqlParameter("@loseIncrement", delta.IsWinner ? 0 : 1),
                        new MySqlParameter("@id", delta.UserId)
                    });

                    await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
