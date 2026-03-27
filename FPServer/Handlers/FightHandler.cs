using FPServer.Cache;
using FPServer.Game;
using FPServer.Network;
using Microsoft.Extensions.Logging;
using Protocol.Code;
using Protocol.Dto.Fight;

namespace FPServer.Handlers
{
    /// <summary>
    /// 战斗处理器
    /// </summary>
    public class FightHandler
    {
        private readonly MessageHandler _messageHandler;
        private readonly ILogger<FightHandler> _logger;
        private readonly OnlineUserCache _userCache;
        private readonly RoomManager _roomManager;

        public FightHandler(MessageHandler messageHandler, ILoggerFactory loggerFactory, OnlineUserCache userCache, RoomManager roomManager)
        {
            _messageHandler = messageHandler;
            _logger = loggerFactory.CreateLogger<FightHandler>();
            _userCache = userCache;
            _roomManager = roomManager;
        }

        public void Handle(ClientConnection client, int subCode, object value)
        {
            if (client.UserId <= 0)
            {
                _logger.LogWarning("未登录用户尝试战斗操作");
                return;
            }

            var room = _roomManager.GetRoomByPlayerId(client.UserId);
            if (room == null)
            {
                _logger.LogWarning("用户不在房间中: {UserId}", client.UserId);
                return;
            }

            switch (subCode)
            {
                case FightCode.GRAB_LANDLORD_CREQ:
                    HandleGrabLandlord(room, client, value);
                    break;
                case FightCode.DEAL_CREQ:
                    HandleDeal(room, client, value as DealDto);
                    break;
                case FightCode.PASS_CREQ:
                    HandlePass(room, client);
                    break;
                default:
                    _logger.LogWarning("未知战斗操作码: {SubCode}", subCode);
                    break;
            }
        }

        /// <summary>
        /// 处理抢地主
        /// </summary>
        private void HandleGrabLandlord(Room room, ClientConnection client, object value)
        {
            bool grab = Convert.ToBoolean(value);
            _logger.LogInformation("用户抢地主: {UserId} {Grab}", client.UserId, grab);

            // 广播抢地主结果 - 使用GrabDto，将是否抢地主信息放在UserId的正负表示
            // 正数表示抢，负数表示不抢
            var grabDto = new GrabDto(grab ? client.UserId : -client.UserId, null, null);
            var msg = new SocketMsg(OpCode.FIGHT, FightCode.GRAB_LANDLORD_BRO, grabDto);
            _messageHandler.BroadcastTo(room.GetPlayerIds(), msg);
        }

        /// <summary>
        /// 处理出牌
        /// </summary>
        private void HandleDeal(Room room, ClientConnection client, DealDto dealDto)
        {
            if (dealDto == null) return;

            _logger.LogInformation("用户出牌: {UserId}", client.UserId);

            // 广播出牌结果
            var msg = new SocketMsg(OpCode.FIGHT, FightCode.DEAL_BRO, dealDto);
            _messageHandler.BroadcastTo(room.GetPlayerIds(), msg);
        }

        /// <summary>
        /// 处理不出
        /// </summary>
        private void HandlePass(Room room, ClientConnection client)
        {
            _logger.LogInformation("用户不出: {UserId}", client.UserId);

            var msg = new SocketMsg(OpCode.FIGHT, FightCode.PASS_SRES, client.UserId);
            _messageHandler.Send(client, msg);

            // 广播给其他玩家
            var broadcastMsg = new SocketMsg(OpCode.FIGHT, FightCode.PASS_SRES, client.UserId);
            var others = room.GetPlayerIds().Where(id => id != client.UserId).ToList();
            _messageHandler.BroadcastTo(others, broadcastMsg);
        }
    }
}