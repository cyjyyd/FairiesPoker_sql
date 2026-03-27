using Microsoft.Extensions.Logging;
using Protocol.Dto;

namespace FPServer.Game
{
    /// <summary>
    /// 游戏房间
    /// </summary>
    public class Room
    {
        public string RoomId { get; private set; }
        public int LandlordId { get; set; } = -1;
        public int Multiple { get; set; } = 1;

        private readonly Dictionary<int, UserDto> _players = new();
        private readonly List<int> _playerOrder = new();
        private readonly HashSet<int> _readyPlayers = new();
        private readonly ILogger<Room> _logger;

        public Room(string roomId, ILogger<Room> logger)
        {
            RoomId = roomId;
            _logger = logger;
        }

        /// <summary>
        /// 添加玩家
        /// </summary>
        public bool AddPlayer(int userId, UserDto userDto)
        {
            if (_players.Count >= 3 || _players.ContainsKey(userId))
            {
                return false;
            }

            _players[userId] = userDto;
            _playerOrder.Add(userId);
            _logger.LogDebug("房间 {RoomId} 添加玩家: {UserId}", RoomId, userId);
            return true;
        }

        /// <summary>
        /// 移除玩家
        /// </summary>
        public void RemovePlayer(int userId)
        {
            _players.Remove(userId);
            _playerOrder.Remove(userId);
            _readyPlayers.Remove(userId);
        }

        /// <summary>
        /// 设置准备
        /// </summary>
        public void SetReady(int userId)
        {
            if (_players.ContainsKey(userId))
            {
                _readyPlayers.Add(userId);
            }
        }

        /// <summary>
        /// 获取玩家数量
        /// </summary>
        public int GetPlayerCount() => _players.Count;

        /// <summary>
        /// 是否已满
        /// </summary>
        public bool IsFull() => _players.Count >= 3;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty() => _players.Count == 0;

        /// <summary>
        /// 是否所有人准备
        /// </summary>
        public bool IsAllReady() => _readyPlayers.Count == _players.Count;

        /// <summary>
        /// 获取玩家ID列表
        /// </summary>
        public List<int> GetPlayerIds() => _playerOrder.ToList();

        /// <summary>
        /// 获取房间数据传输对象
        /// </summary>
        public MatchRoomDto GetRoomDto()
        {
            var dto = new MatchRoomDto();
            foreach (var kvp in _players)
            {
                dto.UIdUserDict[kvp.Key] = kvp.Value;
            }
            dto.UIdList = _playerOrder.ToList();
            dto.ReadyUIdList = _readyPlayers.ToList();
            return dto;
        }
    }
}