using FPServer.Cache;
using FPServer.Database;
using FPServer.Game;
using FPServer.Handlers;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Protocol.Code;

namespace FPServer.Network
{
    /// <summary>
    /// 消息处理器
    /// </summary>
    public class MessageHandler
    {
        private readonly ServerPeer _server;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<MessageHandler> _logger;
        private readonly AccountHandler _accountHandler;
        private readonly UserHandler _userHandler;
        private readonly MatchHandler _matchHandler;
        private readonly FightHandler _fightHandler;
        private readonly OnlineUserCache _userCache;
        private readonly RoomManager _roomManager;

        public MessageHandler(ServerPeer server, ILoggerFactory loggerFactory)
        {
            _server = server;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<MessageHandler>();
            _userCache = new OnlineUserCache();
            _roomManager = new RoomManager(loggerFactory);
            _accountHandler = new AccountHandler(this, loggerFactory, _userCache);
            _userHandler = new UserHandler(this, loggerFactory, _userCache);
            _matchHandler = new MatchHandler(this, loggerFactory, _userCache, _roomManager);
            _fightHandler = new FightHandler(this, loggerFactory, _userCache, _roomManager);
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        public void HandleMessage(ClientConnection client, SocketMsg msg)
        {
            _logger.LogDebug("收到消息 OpCode:{OpCode} SubCode:{SubCode}", msg.OpCode, msg.SubCode);

            switch (msg.OpCode)
            {
                case OpCode.ACCOUNT:
                    _accountHandler.Handle(client, msg.SubCode, msg.Value);
                    break;
                case OpCode.USER:
                    _userHandler.Handle(client, msg.SubCode, msg.Value);
                    break;
                case OpCode.MATCH:
                    _matchHandler.Handle(client, msg.SubCode, msg.Value);
                    break;
                case OpCode.FIGHT:
                    _fightHandler.Handle(client, msg.SubCode, msg.Value);
                    break;
                default:
                    _logger.LogWarning("未知操作码: {OpCode}", msg.OpCode);
                    break;
            }
        }

        /// <summary>
        /// 处理用户下线
        /// </summary>
        public async void HandleUserOffline(ClientConnection client)
        {
            if (client.UserId > 0)
            {
                _logger.LogInformation("用户下线: {UserId} {Username}", client.UserId, client.Username);

                // 从在线缓存移除
                _userCache.RemoveUser(client.UserId);

                // 更新数据库状态
                try
                {
                    await DbHelper.Instance.ExecuteNonQueryAsync(
                        "UPDATE users SET is_online = 0 WHERE id = @id",
                        new MySqlParameter("@id", client.UserId));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "更新用户离线状态失败");
                }

                // 处理房间离开
                _roomManager.LeaveRoom(client.UserId);
            }
        }

        /// <summary>
        /// 发送消息给指定客户端
        /// </summary>
        public void Send(ClientConnection client, SocketMsg msg)
        {
            client.Send(msg);
        }

        /// <summary>
        /// 广播消息
        /// </summary>
        public void Broadcast(SocketMsg msg)
        {
            _server.Broadcast(msg);
        }

        /// <summary>
        /// 广播给指定用户
        /// </summary>
        public void BroadcastTo(List<int> userIds, SocketMsg msg)
        {
            _server.BroadcastTo(userIds, msg);
        }

        /// <summary>
        /// 获取在线客户端
        /// </summary>
        public ClientConnection GetClient(int userId)
        {
            return _userCache.GetClient(userId);
        }
    }
}