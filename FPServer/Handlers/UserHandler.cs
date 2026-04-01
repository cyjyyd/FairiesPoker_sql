using FPServer.Cache;
using FPServer.Database;
using FPServer.Network;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Protocol.Code;
using Protocol.Dto;

namespace FPServer.Handlers
{
    /// <summary>
    /// 用户处理器
    /// </summary>
    public class UserHandler
    {
        private readonly MessageHandler _messageHandler;
        private readonly ILogger<UserHandler> _logger;
        private readonly OnlineUserCache _userCache;

        public UserHandler(MessageHandler messageHandler, ILoggerFactory loggerFactory, OnlineUserCache userCache)
        {
            _messageHandler = messageHandler;
            _logger = loggerFactory.CreateLogger<UserHandler>();
            _userCache = userCache;
        }

        public void Handle(ClientConnection client, int subCode, object value)
        {
            switch (subCode)
            {
                case UserCode.GET_INFO_CREQ:
                    HandleGetInfo(client);
                    break;
                case UserCode.ONLINE_CREQ:
                    HandleOnline(client);
                    break;
                case UserCode.GET_ONLINE_USERS_CREQ:
                    HandleGetOnlineUsers(client);
                    break;
                default:
                    _logger.LogWarning("未知用户操作码: {SubCode}", subCode);
                    break;
            }
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        private void HandleGetInfo(ClientConnection client)
        {
            if (client.UserId <= 0)
            {
                _logger.LogWarning("未登录用户请求获取信息");
                return;
            }

            var userDto = _userCache.GetUserData(client.UserId);
            if (userDto != null)
            {
                var msg = new SocketMsg(OpCode.USER, UserCode.GET_INFO_SRES, userDto);
                _messageHandler.Send(client, msg);
            }
        }

        /// <summary>
        /// 用户上线
        /// </summary>
        private async void HandleOnline(ClientConnection client)
        {
            if (client.UserId <= 0) return;

            try
            {
                // 从数据库获取最新数据
                using var reader = await DbHelper.Instance.ExecuteReaderAsync(
                    "SELECT id, nickname, beans, win_count, lose_count, run_count, level, exp FROM users WHERE id = @id",
                    new MySqlParameter("@id", client.UserId));

                if (await reader.ReadAsync())
                {
                    var userDto = new UserDto(
                        reader.GetInt32("id"),
                        reader.GetString("nickname"),
                        reader.GetInt32("beans"),
                        reader.GetInt32("win_count"),
                        reader.GetInt32("lose_count"),
                        reader.GetInt32("run_count"),
                        reader.GetInt32("level"),
                        reader.GetInt32("exp"));

                    _userCache.UpdateUserData(client.UserId, userDto);

                    var msg = new SocketMsg(OpCode.USER, UserCode.ONLINE_SRES, userDto);
                    _messageHandler.Send(client, msg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户信息失败: {UserId}", client.UserId);
            }
        }

        /// <summary>
        /// 获取在线用户列表
        /// </summary>
        private void HandleGetOnlineUsers(ClientConnection client)
        {
            if (client.UserId <= 0)
            {
                _logger.LogWarning("未登录用户请求在线用户列表");
                return;
            }

            var onlineUsers = _userCache.GetAllOnlineUsers();
            var msg = new SocketMsg(OpCode.USER, UserCode.GET_ONLINE_USERS_SRES, onlineUsers);
            _messageHandler.Send(client, msg);
            _logger.LogDebug("用户 {UserId} 请求在线用户列表: {Count}人", client.UserId, onlineUsers.Count);
        }
    }
}