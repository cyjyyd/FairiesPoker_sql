using Microsoft.Extensions.Logging;
using Protocol.Dto;

namespace FPServer.Game
{
    /// <summary>
    /// 房间管理器
    /// </summary>
    public class RoomManager
    {
        private readonly Dictionary<string, Room> _rooms = new();
        private readonly Dictionary<int, string> _playerRoomMap = new();
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<RoomManager> _logger;
        private readonly object _lock = new();

        public RoomManager(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<RoomManager>();
        }

        /// <summary>
        /// 查找或创建房间（用于快速匹配）
        /// </summary>
        public Room FindOrCreateRoom()
        {
            lock (_lock)
            {
                // 查找未满的快速匹配房间
                var availableRoom = _rooms.Values.FirstOrDefault(r => !r.IsFull() && r.IsWaiting() && r.IsQuickMatch);
                if (availableRoom != null)
                {
                    return availableRoom;
                }

                // 创建新的快速匹配房间
                var roomId = Guid.NewGuid().ToString("N").Substring(0, 8);
                var newRoom = new Room(roomId, _loggerFactory.CreateLogger<Room>(), _loggerFactory)
                {
                    IsQuickMatch = true,
                    RoomName = "快速匹配"
                };
                _rooms[roomId] = newRoom;
                _logger.LogInformation("创建新快速匹配房间: {RoomId}", roomId);
                return newRoom;
            }
        }

        /// <summary>
        /// 创建指定名称的房间
        /// </summary>
        public Room CreateRoom(int hostId, string roomName)
        {
            lock (_lock)
            {
                var roomId = Guid.NewGuid().ToString("N").Substring(0, 8);
                var newRoom = new Room(roomId, _loggerFactory.CreateLogger<Room>(), _loggerFactory)
                {
                    RoomName = roomName,
                    HostId = hostId
                };
                _rooms[roomId] = newRoom;
                _logger.LogInformation("创建新房间: {RoomId} - {RoomName}", roomId, roomName);
                return newRoom;
            }
        }

        /// <summary>
        /// 添加玩家到房间
        /// </summary>
        public bool AddPlayerToRoom(Room room, int userId, UserDto userDto)
        {
            lock (_lock)
            {
                if (room.AddPlayer(userId, userDto))
                {
                    _playerRoomMap[userId] = room.RoomId;
                    _logger.LogDebug("玩家 {UserId} 加入房间 {RoomId}", userId, room.RoomId);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 从房间移除玩家
        /// </summary>
        public void RemovePlayerFromRoom(Room room, int userId)
        {
            lock (_lock)
            {
                room.RemovePlayer(userId);
                _playerRoomMap.Remove(userId);
                _logger.LogDebug("玩家 {UserId} 离开房间 {RoomId}", userId, room.RoomId);
            }
        }

        /// <summary>
        /// 获取所有房间（不包括快速匹配房间）
        /// </summary>
        public List<Room> GetAllRooms()
        {
            lock (_lock)
            {
                return _rooms.Values.Where(r => r.IsWaiting() && !r.IsQuickMatch).ToList();
            }
        }

        /// <summary>
        /// 根据玩家ID获取房间
        /// </summary>
        public Room GetRoomByPlayerId(int playerId)
        {
            lock (_lock)
            {
                if (_playerRoomMap.TryGetValue(playerId, out var roomId))
                {
                    return _rooms.TryGetValue(roomId, out var room) ? room : null;
                }
                return null;
            }
        }

        /// <summary>
        /// 获取房间
        /// </summary>
        public Room GetRoom(string roomId)
        {
            lock (_lock)
            {
                return _rooms.TryGetValue(roomId, out var room) ? room : null;
            }
        }

        /// <summary>
        /// 移除房间
        /// </summary>
        public void RemoveRoom(string roomId)
        {
            lock (_lock)
            {
                if (_rooms.TryGetValue(roomId, out var room))
                {
                    foreach (var playerId in room.GetPlayerIds())
                    {
                        _playerRoomMap.Remove(playerId);
                    }
                    _rooms.Remove(roomId);
                    _logger.LogInformation("移除房间: {RoomId}", roomId);
                }
            }
        }

        /// <summary>
        /// 玩家离开房间
        /// </summary>
        public void LeaveRoom(int playerId)
        {
            lock (_lock)
            {
                if (_playerRoomMap.TryGetValue(playerId, out var roomId))
                {
                    if (_rooms.TryGetValue(roomId, out var room))
                    {
                        room.RemovePlayer(playerId);
                        if (room.IsEmpty())
                        {
                            _rooms.Remove(roomId);
                            _logger.LogInformation("移除空房间: {RoomId}", roomId);
                        }
                    }
                    _playerRoomMap.Remove(playerId);
                }
            }
        }
    }
}