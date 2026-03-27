using FPServer.Cache;
using FPServer.Game;
using FPServer.Network;
using Microsoft.Extensions.Logging;
using Protocol.Code;
using Protocol.Dto;

namespace FPServer.Handlers
{
    /// <summary>
    /// 匹配处理器
    /// </summary>
    public class MatchHandler
    {
        private readonly MessageHandler _messageHandler;
        private readonly ILogger<MatchHandler> _logger;
        private readonly OnlineUserCache _userCache;
        private readonly RoomManager _roomManager;

        public MatchHandler(MessageHandler messageHandler, ILoggerFactory loggerFactory, OnlineUserCache userCache, RoomManager roomManager)
        {
            _messageHandler = messageHandler;
            _logger = loggerFactory.CreateLogger<MatchHandler>();
            _userCache = userCache;
            _roomManager = roomManager;
        }

        public void Handle(ClientConnection client, int subCode, object value)
        {
            if (client.UserId <= 0)
            {
                _logger.LogWarning("未登录用户尝试匹配操作");
                return;
            }

            switch (subCode)
            {
                case MatchCode.ENTER_CREQ:
                    HandleEnter(client);
                    break;
                case MatchCode.LEAVE_CREQ:
                    HandleLeave(client);
                    break;
                case MatchCode.READY_CREQ:
                    HandleReady(client);
                    break;
                default:
                    _logger.LogWarning("未知匹配操作码: {SubCode}", subCode);
                    break;
            }
        }

        /// <summary>
        /// 进入匹配队列
        /// </summary>
        private void HandleEnter(ClientConnection client)
        {
            var room = _roomManager.FindOrCreateRoom();
            var userDto = _userCache.GetUserData(client.UserId);

            if (room.AddPlayer(client.UserId, userDto))
            {
                _logger.LogInformation("用户进入匹配: {UserId}", client.UserId);

                // 发送进入成功
                var enterMsg = new SocketMsg(OpCode.MATCH, MatchCode.ENTER_SRES, room.GetRoomDto());
                _messageHandler.Send(client, enterMsg);

                // 广播给房间内所有玩家
                var broadcastMsg = new SocketMsg(OpCode.MATCH, MatchCode.ENTER_BRO, room.GetRoomDto());
                _messageHandler.BroadcastTo(room.GetPlayerIds(), broadcastMsg);

                // 检查是否满3人，可以开始游戏
                if (room.IsFull())
                {
                    var startMsg = new SocketMsg(OpCode.MATCH, MatchCode.START_BRO, room.GetRoomDto());
                    _messageHandler.BroadcastTo(room.GetPlayerIds(), startMsg);
                }
            }
        }

        /// <summary>
        /// 离开匹配队列
        /// </summary>
        private void HandleLeave(ClientConnection client)
        {
            var room = _roomManager.GetRoomByPlayerId(client.UserId);
            if (room != null)
            {
                room.RemovePlayer(client.UserId);
                _logger.LogInformation("用户离开匹配: {UserId}", client.UserId);

                // 广播给剩余玩家
                if (room.GetPlayerCount() > 0)
                {
                    var leaveMsg = new SocketMsg(OpCode.MATCH, MatchCode.LEAVE_BRO, room.GetRoomDto());
                    _messageHandler.BroadcastTo(room.GetPlayerIds(), leaveMsg);
                }

                if (room.IsEmpty())
                {
                    _roomManager.RemoveRoom(room.RoomId);
                }
            }
        }

        /// <summary>
        /// 准备
        /// </summary>
        private void HandleReady(ClientConnection client)
        {
            var room = _roomManager.GetRoomByPlayerId(client.UserId);
            if (room != null)
            {
                room.SetReady(client.UserId);
                _logger.LogInformation("用户准备: {UserId}", client.UserId);

                // 广播准备状态
                var readyMsg = new SocketMsg(OpCode.MATCH, MatchCode.READY_BRO, room.GetRoomDto());
                _messageHandler.BroadcastTo(room.GetPlayerIds(), readyMsg);

                // 检查是否所有人都准备了
                if (room.IsAllReady() && room.IsFull())
                {
                    var startMsg = new SocketMsg(OpCode.MATCH, MatchCode.START_BRO, room.GetRoomDto());
                    _messageHandler.BroadcastTo(room.GetPlayerIds(), startMsg);
                }
            }
        }
    }
}