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
        /// 开始游戏（发牌）
        /// </summary>
        public void StartGame(Room room)
        {
            _logger.LogInformation("房间 {RoomId} 开始游戏", room.RoomId);

            room.StartGame();
            var gameState = room.GameState;

            // 给每个玩家发送手牌
            foreach (var userId in room.GetPlayerIds())
            {
                var cards = gameState.GetPlayerCards(userId);
                var msg = new SocketMsg(OpCode.FIGHT, FightCode.GET_CARD_SRES, cards);
                _messageHandler.SendToUser(userId, msg);
            }

            // 广播第一个抢地主的玩家
            int firstGrabUserId = gameState.GetNextGrabUserId();
            BroadcastTurnGrab(room, firstGrabUserId);
        }

        /// <summary>
        /// 处理抢地主
        /// </summary>
        private void HandleGrabLandlord(Room room, ClientConnection client, object value)
        {
            var gameState = room.GameState;
            if (gameState == null)
            {
                _logger.LogWarning("游戏未开始");
                return;
            }

            bool grab = Convert.ToBoolean(value);
            _logger.LogInformation("用户抢地主: {UserId} {Grab}", client.UserId, grab);

            // 处理抢地主
            int result = gameState.ProcessGrab(client.UserId, grab);

            if (result > 0)
            {
                // 抢地主成功
                room.LandlordId = result;

                // 广播抢地主结果
                var grabDto = new GrabDto(
                    result,
                    gameState.GetTableCards(),
                    gameState.GetPlayerCards(result)
                );
                var msg = new SocketMsg(OpCode.FIGHT, FightCode.GRAB_LANDLORD_BRO, grabDto);
                _messageHandler.BroadcastTo(room.GetPlayerIds(), msg);

                // 广播地主开始出牌
                BroadcastTurnDeal(room, result);
            }
            else
            {
                // 继续下一个抢地主
                int nextUserId = gameState.GetNextGrabUserId();
                if (nextUserId > 0)
                {
                    BroadcastTurnGrab(room, nextUserId);
                }
            }
        }

        /// <summary>
        /// 广播抢地主轮换
        /// </summary>
        private void BroadcastTurnGrab(Room room, int userId)
        {
            _logger.LogDebug("广播抢地主轮换: {UserId}", userId);
            var msg = new SocketMsg(OpCode.FIGHT, FightCode.TURN_GRAB_BRO, userId);
            _messageHandler.BroadcastTo(room.GetPlayerIds(), msg);
        }

        /// <summary>
        /// 处理出牌
        /// </summary>
        private void HandleDeal(Room room, ClientConnection client, DealDto dealDto)
        {
            if (dealDto == null) return;

            var gameState = room.GameState;
            if (gameState == null)
            {
                _logger.LogWarning("游戏未开始");
                return;
            }

            _logger.LogInformation("用户出牌: {UserId}", client.UserId);

            // 处理出牌
            if (gameState.ProcessDeal(client.UserId, dealDto))
            {
                // 出牌成功，广播出牌结果
                var msg = new SocketMsg(OpCode.FIGHT, FightCode.DEAL_BRO, dealDto);
                _messageHandler.BroadcastTo(room.GetPlayerIds(), msg);

                // 发送成功响应给出牌者
                var sresMsg = new SocketMsg(OpCode.FIGHT, FightCode.DEAL_SRES, 0);
                _messageHandler.Send(client, sresMsg);

                // 检查游戏是否结束
                if (gameState.IsGameFinished())
                {
                    HandleGameOver(room);
                }
                else
                {
                    // 广播下一个出牌的玩家
                    BroadcastTurnDeal(room, gameState.CurrentTurnUserId);
                }
            }
            else
            {
                // 出牌失败，发送失败响应
                var msg = new SocketMsg(OpCode.FIGHT, FightCode.DEAL_SRES, -1);
                _messageHandler.Send(client, msg);
            }
        }

        /// <summary>
        /// 处理不出
        /// </summary>
        private void HandlePass(Room room, ClientConnection client)
        {
            var gameState = room.GameState;
            if (gameState == null)
            {
                _logger.LogWarning("游戏未开始");
                return;
            }

            _logger.LogInformation("用户不出: {UserId}", client.UserId);

            // 处理不出
            if (gameState.ProcessPass(client.UserId))
            {
                // 不出成功，发送响应
                var sresMsg = new SocketMsg(OpCode.FIGHT, FightCode.PASS_SRES, 0);
                _messageHandler.Send(client, sresMsg);

                // 广播下一个出牌的玩家
                BroadcastTurnDeal(room, gameState.CurrentTurnUserId);
            }
            else
            {
                // 不能不出
                var msg = new SocketMsg(OpCode.FIGHT, FightCode.PASS_SRES, -1);
                _messageHandler.Send(client, msg);
            }
        }

        /// <summary>
        /// 广播出牌轮换
        /// </summary>
        private void BroadcastTurnDeal(Room room, int userId)
        {
            _logger.LogDebug("广播出牌轮换: {UserId}", userId);
            var msg = new SocketMsg(OpCode.FIGHT, FightCode.TURN_DEAL_BRO, userId);
            _messageHandler.BroadcastTo(room.GetPlayerIds(), msg);
        }

        /// <summary>
        /// 处理游戏结束
        /// </summary>
        private void HandleGameOver(Room room)
        {
            var gameState = room.GameState;
            var winners = gameState.GetWinners();

            _logger.LogInformation("游戏结束，胜利者: {Winners}", string.Join(",", winners));

            // 创建结束DTO
            var overDto = new OverDto
            {
                WinUIdList = winners,
                WinIdentity = winners.Contains(room.LandlordId) ? 0 : 1, // 0=地主赢，1=农民赢
                BeenCount = room.Multiple * (winners.Contains(room.LandlordId) ? 2 : 1)
            };

            // 广播游戏结束
            var msg = new SocketMsg(OpCode.FIGHT, FightCode.OVER_BRO, overDto);
            _messageHandler.BroadcastTo(room.GetPlayerIds(), msg);

            // 结束游戏
            room.EndGame();
        }
    }
}