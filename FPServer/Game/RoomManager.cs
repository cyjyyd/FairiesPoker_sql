using Microsoft.Extensions.Logging;

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
        /// 查找或创建房间
        /// </summary>
        public Room FindOrCreateRoom()
        {
            lock (_lock)
            {
                // 查找未满的房间
                var availableRoom = _rooms.Values.FirstOrDefault(r => !r.IsFull());
                if (availableRoom != null)
                {
                    return availableRoom;
                }

                // 创建新房间
                var roomId = Guid.NewGuid().ToString("N").Substring(0, 8);
                var newRoom = new Room(roomId, _loggerFactory.CreateLogger<Room>());
                _rooms[roomId] = newRoom;
                _logger.LogInformation("创建新房间: {RoomId}", roomId);
                return newRoom;
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