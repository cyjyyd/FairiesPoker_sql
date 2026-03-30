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
                // 匹配相关
                case MatchCode.ENTER_CREQ:
                    HandleEnter(client);
                    break;
                case MatchCode.LEAVE_CREQ:
                    HandleLeave(client);
                    break;
                case MatchCode.READY_CREQ:
                    HandleReady(client);
                    break;
                case MatchCode.START_CREQ:
                    HandleStartGame(client);
                    break;

                // 房间相关
                case RoomCode.GET_ROOMS_CREQ:
                    HandleGetRooms(client);
                    break;
                case RoomCode.CREATE_CREQ:
                    HandleCreateRoom(client, value as RoomDto);
                    break;
                case RoomCode.JOIN_CREQ:
                    HandleJoinRoom(client, value);
                    break;
                case RoomCode.LEAVE_CREQ:
                    HandleLeaveRoom(client);
                    break;

                default:
                    _logger.LogWarning("未知匹配操作码: {SubCode}", subCode);
                    break;
            }
        }

        #region 匹配功能

        /// <summary>
        /// 进入匹配队列
        /// </summary>
        private void HandleEnter(ClientConnection client)
        {
            var room = _roomManager.FindOrCreateRoom();
            var userDto = _userCache.GetUserData(client.UserId);

            if (_roomManager.AddPlayerToRoom(room, client.UserId, userDto))
            {
                _logger.LogInformation("用户进入匹配: {UserId}", client.UserId);

                // 发送进入成功（设置正确的左右玩家）
                var myRoomDto = room.GetMatchRoomDtoForPlayer(client.UserId);
                var enterMsg = new SocketMsg(OpCode.MATCH, MatchCode.ENTER_SRES, myRoomDto);
                _messageHandler.Send(client, enterMsg);

                // 广播给房间内所有玩家（每个人需要收到自己视角的房间数据）
                foreach (var userId in room.GetPlayerIds())
                {
                    var roomDto = room.GetMatchRoomDtoForPlayer(userId);
                    var broadcastMsg = new SocketMsg(OpCode.MATCH, MatchCode.ENTER_BRO, roomDto);
                    var userClient = _userCache.GetClient(userId);
                    if (userClient != null)
                    {
                        _messageHandler.Send(userClient, broadcastMsg);
                    }
                }

                // 检查是否满3人
                if (room.IsFull())
                {
                    // 如果是快速匹配房间，直接开始游戏
                    if (room.IsQuickMatch)
                    {
                        StartGame(room);
                    }
                    else
                    {
                        // 自定义房间广播更新后的房间状态
                        BroadcastRoomUpdate(room);
                    }
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
                _roomManager.RemovePlayerFromRoom(room, client.UserId);
                _logger.LogInformation("用户离开匹配: {UserId}", client.UserId);

                // 广播给剩余玩家
                if (room.GetPlayerCount() > 0)
                {
                    BroadcastRoomUpdate(room);
                }

                if (room.IsEmpty())
                {
                    _roomManager.RemoveRoom(room.RoomId);
                }
            }
        }

        /// <summary>
        /// 准备/取消准备
        /// </summary>
        private void HandleReady(ClientConnection client)
        {
            var room = _roomManager.GetRoomByPlayerId(client.UserId);
            if (room != null)
            {
                // 检查是否已准备，如果已准备则取消
                if (room.IsPlayerReady(client.UserId))
                {
                    room.CancelReady(client.UserId);
                    _logger.LogInformation("用户取消准备: {UserId}", client.UserId);
                }
                else
                {
                    room.SetReady(client.UserId);
                    _logger.LogInformation("用户准备: {UserId}", client.UserId);
                }

                // 广播准备状态
                BroadcastRoomUpdate(room);
            }
        }

        /// <summary>
        /// 房主开始游戏
        /// </summary>
        private void HandleStartGame(ClientConnection client)
        {
            var room = _roomManager.GetRoomByPlayerId(client.UserId);
            if (room == null) return;

            // 只有房主可以开始游戏
            if (room.HostId != client.UserId)
            {
                _logger.LogWarning("非房主尝试开始游戏: {UserId}", client.UserId);
                return;
            }

            // 检查是否满员且全部准备
            if (!room.IsFull() || !room.IsAllReady())
            {
                _logger.LogWarning("房间未满足开始条件");
                return;
            }

            // 开始游戏
            StartGame(room);
        }

        /// <summary>
        /// 广播房间更新
        /// </summary>
        private void BroadcastRoomUpdate(Room room)
        {
            foreach (var userId in room.GetPlayerIds())
            {
                var roomDto = room.GetMatchRoomDtoForPlayer(userId);
                var msg = new SocketMsg(OpCode.MATCH, MatchCode.READY_BRO, roomDto);
                var userClient = _userCache.GetClient(userId);
                if (userClient != null)
                {
                    _messageHandler.Send(userClient, msg);
                }
            }
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        private void StartGame(Room room)
        {
            _logger.LogInformation("房间 {RoomId} 所有人已准备，开始游戏", room.RoomId);

            // 广播游戏开始
            var startMsg = new SocketMsg(OpCode.MATCH, MatchCode.START_BRO, room.GetMatchRoomDto());
            _messageHandler.BroadcastTo(room.GetPlayerIds(), startMsg);

            // 通知FightHandler开始游戏（发牌）
            _messageHandler.GetFightHandler().StartGame(room);
        }

        #endregion

        #region 自定义房间功能

        /// <summary>
        /// 获取房间列表
        /// </summary>
        private void HandleGetRooms(ClientConnection client)
        {
            var rooms = _roomManager.GetAllRooms();
            var roomList = rooms.Select(r => r.GetRoomDto()).ToList();

            var msg = new SocketMsg(OpCode.MATCH, RoomCode.GET_ROOMS_SRES, roomList);
            _messageHandler.Send(client, msg);
        }

        /// <summary>
        /// 创建房间
        /// </summary>
        private void HandleCreateRoom(ClientConnection client, RoomDto roomDto)
        {
            var userDto = _userCache.GetUserData(client.UserId);
            var room = _roomManager.CreateRoom(client.UserId, roomDto?.RoomName ?? $"{userDto.Name}的房间");

            if (_roomManager.AddPlayerToRoom(room, client.UserId, userDto))
            {
                _logger.LogInformation("用户 {UserId} 创建房间: {RoomId}", client.UserId, room.RoomId);

                // 发送房间创建成功（使用 MatchRoomDto 以便正确显示玩家）
                var msg = new SocketMsg(OpCode.MATCH, RoomCode.CREATE_SRES, room.GetMatchRoomDtoForPlayer(client.UserId));
                _messageHandler.Send(client, msg);

                // 广播房间列表更新给所有在线用户
                BroadcastRoomListUpdate();
            }
        }

        /// <summary>
        /// 加入房间
        /// </summary>
        private void HandleJoinRoom(ClientConnection client, object value)
        {
            string roomId = value?.ToString();
            if (string.IsNullOrEmpty(roomId))
            {
                return;
            }

            var room = _roomManager.GetRoom(roomId);
            if (room == null)
            {
                return;
            }

            var userDto = _userCache.GetUserData(client.UserId);
            if (_roomManager.AddPlayerToRoom(room, client.UserId, userDto))
            {
                _logger.LogInformation("用户 {UserId} 加入房间: {RoomId}", client.UserId, roomId);

                // 广播给房间内所有玩家（每个人收到自己视角的数据）
                foreach (var userId in room.GetPlayerIds())
                {
                    var roomDto = room.GetMatchRoomDtoForPlayer(userId);
                    var broadcastMsg = new SocketMsg(OpCode.MATCH, RoomCode.UPDATE_BRO, roomDto);
                    var userClient = _userCache.GetClient(userId);
                    if (userClient != null)
                    {
                        _messageHandler.Send(userClient, broadcastMsg);
                    }
                }

                // 广播房间列表更新给所有在线用户
                BroadcastRoomListUpdate();
            }
        }

        /// <summary>
        /// 离开房间
        /// </summary>
        private void HandleLeaveRoom(ClientConnection client)
        {
            var room = _roomManager.GetRoomByPlayerId(client.UserId);
            if (room != null)
            {
                _roomManager.RemovePlayerFromRoom(room, client.UserId);
                _logger.LogInformation("用户 {UserId} 离开房间: {RoomId}", client.UserId, room.RoomId);

                // 广播给剩余玩家
                if (room.GetPlayerCount() > 0)
                {
                    foreach (var userId in room.GetPlayerIds())
                    {
                        var roomDto = room.GetMatchRoomDtoForPlayer(userId);
                        var leaveMsg = new SocketMsg(OpCode.MATCH, RoomCode.UPDATE_BRO, roomDto);
                        var userClient = _userCache.GetClient(userId);
                        if (userClient != null)
                        {
                            _messageHandler.Send(userClient, leaveMsg);
                        }
                    }
                }

                if (room.IsEmpty())
                {
                    _roomManager.RemoveRoom(room.RoomId);
                }

                // 广播房间列表更新
                BroadcastRoomListUpdate();
            }
        }

        /// <summary>
        /// 广播房间列表更新给所有在线用户
        /// </summary>
        private void BroadcastRoomListUpdate()
        {
            var rooms = _roomManager.GetAllRooms();
            var roomList = rooms.Select(r => r.GetRoomDto()).ToList();
            var msg = new SocketMsg(OpCode.MATCH, RoomCode.GET_ROOMS_SRES, roomList);
            _messageHandler.Broadcast(msg);
        }

        #endregion
    }
}