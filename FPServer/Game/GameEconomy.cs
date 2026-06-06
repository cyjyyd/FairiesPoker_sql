using Microsoft.Extensions.Configuration;

namespace FPServer.Game
{
    public sealed class GameEconomy
    {
        public const int DefaultInitialBeans = 1000;
        public const int DefaultBaseStake = 20;
        public const int DefaultMaxMultiple = 16;
        public const int DefaultSingleGameLossLimitPercent = 30;
        public const int DefaultWeeklyReliefThreshold = 500;
        public const int DefaultWeeklyReliefBeans = 1000;
        public const int DefaultWeeklyReliefCooldownDays = 7;

        public int InitialBeans { get; }
        public int BaseStake { get; }
        public int MaxMultiple { get; }
        public int SingleGameLossLimitPercent { get; }
        public int WeeklyReliefThreshold { get; }
        public int WeeklyReliefBeans { get; }
        public int WeeklyReliefCooldownDays { get; }

        public static GameEconomy Default { get; } = new(
            DefaultInitialBeans,
            DefaultBaseStake,
            DefaultMaxMultiple,
            DefaultSingleGameLossLimitPercent,
            DefaultWeeklyReliefThreshold,
            DefaultWeeklyReliefBeans,
            DefaultWeeklyReliefCooldownDays);

        public GameEconomy(
            int initialBeans,
            int baseStake,
            int maxMultiple,
            int singleGameLossLimitPercent,
            int weeklyReliefThreshold,
            int weeklyReliefBeans,
            int weeklyReliefCooldownDays)
        {
            InitialBeans = Math.Max(0, initialBeans);
            BaseStake = Math.Max(1, baseStake);
            MaxMultiple = Math.Max(1, maxMultiple);
            SingleGameLossLimitPercent = Math.Clamp(singleGameLossLimitPercent, 1, 100);
            WeeklyReliefThreshold = Math.Max(0, weeklyReliefThreshold);
            WeeklyReliefBeans = Math.Max(0, weeklyReliefBeans);
            WeeklyReliefCooldownDays = Math.Max(1, weeklyReliefCooldownDays);
        }

        public static GameEconomy FromConfiguration(IConfiguration configuration)
        {
            var section = configuration.GetSection("Game");
            int legacyWinStake = GetInt(section, "BeansPerWin", DefaultBaseStake * 2);
            int baseStake = GetInt(section, "BaseStake", GetInt(section, "BeansPerLose", Math.Max(1, legacyWinStake / 2)));

            return new GameEconomy(
                GetInt(section, "InitialBeans", DefaultInitialBeans),
                baseStake,
                GetInt(section, "MaxMultiple", DefaultMaxMultiple),
                GetInt(section, "SingleGameLossLimitPercent", DefaultSingleGameLossLimitPercent),
                GetInt(section, "WeeklyReliefThreshold", DefaultWeeklyReliefThreshold),
                GetInt(section, "WeeklyReliefBeans", DefaultWeeklyReliefBeans),
                GetInt(section, "WeeklyReliefCooldownDays", DefaultWeeklyReliefCooldownDays));
        }

        public GameSettlementResult CalculateSettlement(
            IReadOnlyCollection<int> playerIds,
            IReadOnlyCollection<int> winnerIds,
            int landlordId,
            int multiple,
            IReadOnlyDictionary<int, int> currentBeans = null)
        {
            var winners = new HashSet<int>(winnerIds ?? Array.Empty<int>());
            bool landlordWins = landlordId > 0 && winners.Contains(landlordId);
            int effectiveMultiple = GetEffectiveMultiple(multiple);
            int farmerStake = BaseStake * effectiveMultiple;
            int landlordStake = farmerStake * 2;
            int clientBasePoints = landlordWins ? landlordStake : farmerStake;
            var playerList = (playerIds ?? Array.Empty<int>())
                .Where(id => id > 0)
                .ToList();

            if (landlordWins)
                return CalculateLandlordWinSettlement(playerList, winners, landlordId, farmerStake, currentBeans, clientBasePoints, effectiveMultiple);

            return CalculateFarmerWinSettlement(playerList, winners, landlordId, landlordStake, currentBeans, clientBasePoints, effectiveMultiple);
        }

        public int GetEffectiveMultiple(int multiple)
        {
            return Math.Min(MaxMultiple, Math.Max(1, multiple));
        }

        private GameSettlementResult CalculateLandlordWinSettlement(
            IReadOnlyList<int> playerIds,
            HashSet<int> winners,
            int landlordId,
            int farmerStake,
            IReadOnlyDictionary<int, int> currentBeans,
            int clientBasePoints,
            int effectiveMultiple)
        {
            var deltas = new List<GameSettlementDelta>();
            int landlordWin = 0;

            foreach (int userId in playerIds)
            {
                if (userId == landlordId)
                    continue;

                int loss = GetCappedLoss(userId, farmerStake, currentBeans);
                landlordWin += loss;
                deltas.Add(new GameSettlementDelta(userId, winners.Contains(userId), -loss));
            }

            if (landlordId > 0)
                deltas.Add(new GameSettlementDelta(landlordId, true, landlordWin));

            return new GameSettlementResult(true, clientBasePoints, effectiveMultiple, deltas);
        }

        private GameSettlementResult CalculateFarmerWinSettlement(
            IReadOnlyList<int> playerIds,
            HashSet<int> winners,
            int landlordId,
            int landlordStake,
            IReadOnlyDictionary<int, int> currentBeans,
            int clientBasePoints,
            int effectiveMultiple)
        {
            var deltas = new List<GameSettlementDelta>();
            int landlordLoss = landlordId > 0
                ? GetCappedLoss(landlordId, landlordStake, currentBeans)
                : 0;
            var farmerWinners = playerIds
                .Where(id => id != landlordId && winners.Contains(id))
                .ToList();
            int winnerCount = Math.Max(1, farmerWinners.Count);
            int baseWin = landlordLoss / winnerCount;
            int remainder = landlordLoss % winnerCount;

            foreach (int userId in playerIds)
            {
                if (userId == landlordId)
                {
                    deltas.Add(new GameSettlementDelta(userId, false, -landlordLoss));
                    continue;
                }

                bool isFarmerWinner = farmerWinners.Contains(userId);
                int win = isFarmerWinner ? baseWin : 0;
                if (isFarmerWinner && remainder > 0)
                {
                    win++;
                    remainder--;
                }

                deltas.Add(new GameSettlementDelta(userId, winners.Contains(userId), win));
            }

            return new GameSettlementResult(false, clientBasePoints, effectiveMultiple, deltas);
        }

        private int GetCappedLoss(int userId, int nominalLoss, IReadOnlyDictionary<int, int> currentBeans)
        {
            if (nominalLoss <= 0)
                return 0;

            if (currentBeans == null || !currentBeans.TryGetValue(userId, out int beans))
                return nominalLoss;

            beans = Math.Max(0, beans);
            if (beans == 0)
                return 0;

            int percentCap = Math.Max(1, beans * SingleGameLossLimitPercent / 100);
            int keepOneBeanCap = Math.Max(0, beans - 1);
            int cap = Math.Min(percentCap, keepOneBeanCap);

            return Math.Min(nominalLoss, cap);
        }

        public bool IsWeeklyReliefEligible(int beans, DateTime? lastReliefAt, DateTime now)
        {
            return WeeklyReliefBeans > 0 &&
                beans < WeeklyReliefThreshold &&
                (!lastReliefAt.HasValue ||
                    lastReliefAt.Value <= now.AddDays(-WeeklyReliefCooldownDays));
        }

        private static int GetInt(IConfigurationSection section, string key, int fallback)
        {
            return int.TryParse(section[key], out int value) ? value : fallback;
        }
    }

    public sealed class GameSettlementResult
    {
        public bool LandlordWins { get; }
        public int ClientBasePoints { get; }
        public int EffectiveMultiple { get; }
        public IReadOnlyList<GameSettlementDelta> Deltas { get; }

        public GameSettlementResult(bool landlordWins, int clientBasePoints, int effectiveMultiple, IReadOnlyList<GameSettlementDelta> deltas)
        {
            LandlordWins = landlordWins;
            ClientBasePoints = clientBasePoints;
            EffectiveMultiple = effectiveMultiple;
            Deltas = deltas;
        }
    }

    public sealed class GameSettlementDelta
    {
        public int UserId { get; }
        public bool IsWinner { get; }
        public int BeanDelta { get; }

        public GameSettlementDelta(int userId, bool isWinner, int beanDelta)
        {
            UserId = userId;
            IsWinner = isWinner;
            BeanDelta = beanDelta;
        }
    }
}
