using Microsoft.Extensions.Logging;
using Protocol.Code;
using Protocol.Dto;

namespace FPServer.Game
{
    /// <summary>
    /// 游戏房间
    /// </summary>
    public class Room
    {
        public string RoomId { get; private set; }
        public string RoomName { get; set; }
        public int HostId { get; set; } = -1;
        public int LandlordId { get; set; } = -1;
        public int Multiple { get; set; } = 1;
        public int Status { get; set; } = RoomStatus.WAITING;
        public bool IsQuickMatch { get; set; } = false; // 是否是快速匹配房间

        private readonly Dictionary<int, UserDto> _players = new();
        private readonly List<int> _playerOrder = new();
        private readonly HashSet<int> _readyPlayers = new();
        private readonly ILogger<Room> _logger;
        private readonly ILoggerFactory _loggerFactory;

        // 游戏状态
        public GameState GameState { get; private set; }

        public Room(string roomId, ILogger<Room> logger, ILoggerFactory loggerFactory)
        {
            RoomId = roomId;
            RoomName = $"房间{roomId}";
            _logger = logger;
            _loggerFactory = loggerFactory;
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

            // 如果房主离开，转移房主
            if (HostId == userId && _playerOrder.Count > 0)
            {
                HostId = _playerOrder[0];
            }
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
        /// 取消准备
        /// </summary>
        public void CancelReady(int userId)
        {
            _readyPlayers.Remove(userId);
        }

        /// <summary>
        /// 检查玩家是否已准备
        /// </summary>
        public bool IsPlayerReady(int userId)
        {
            return _readyPlayers.Contains(userId);
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
        /// 是否等待中
        /// </summary>
        public bool IsWaiting() => Status == RoomStatus.WAITING;

        /// <summary>
        /// 是否所有人准备
        /// </summary>
        public bool IsAllReady() => _readyPlayers.Count == _players.Count && _players.Count > 0;

        /// <summary>
        /// 获取玩家ID列表
        /// </summary>
        public List<int> GetPlayerIds() => _playerOrder.ToList();

        /// <summary>
        /// 获取房间数据传输对象（用于房间列表）
        /// </summary>
        public RoomDto GetRoomDto()
        {
            var dto = new RoomDto
            {
                RoomId = RoomId,
                RoomName = RoomName,
                HostId = HostId,
                PlayerCount = _players.Count,
                MaxPlayers = 3,
                Status = Status,
                Players = _players.Values.ToList()
            };
            return dto;
        }

        /// <summary>
        /// 获取匹配房间数据传输对象（用于匹配）
        /// </summary>
        public MatchRoomDto GetMatchRoomDto()
        {
            var dto = new MatchRoomDto();
            foreach (var kvp in _players)
            {
                dto.UIdUserDict[kvp.Key] = kvp.Value;
            }
            dto.UIdList = _playerOrder.ToList();
            dto.ReadyUIdList = _readyPlayers.ToList();
            dto.HostId = HostId;
            dto.RoomId = RoomId;
            dto.RoomName = RoomName;
            dto.IsQuickMatch = IsQuickMatch;

            // 设置左右玩家ID
            dto.ResetPosition(HostId); // 使用第一个玩家作为参考

            return dto;
        }

        /// <summary>
        /// 获取指定玩家的匹配房间DTO（带有正确的左右玩家设置）
        /// </summary>
        public MatchRoomDto GetMatchRoomDtoForPlayer(int myUserId)
        {
            var dto = new MatchRoomDto();
            foreach (var kvp in _players)
            {
                dto.UIdUserDict[kvp.Key] = kvp.Value;
            }
            dto.UIdList = _playerOrder.ToList();
            dto.ReadyUIdList = _readyPlayers.ToList();
            dto.HostId = HostId;
            dto.RoomId = RoomId;
            dto.RoomName = RoomName;
            dto.IsQuickMatch = IsQuickMatch;

            // 根据当前玩家设置左右玩家
            dto.ResetPosition(myUserId);

            return dto;
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        public void StartGame()
        {
            Status = RoomStatus.PLAYING;
            GameState = new GameState(_loggerFactory.CreateLogger<GameState>());
            GameState.InitGame(_playerOrder.ToList());
            LandlordId = -1;
            _logger.LogInformation("房间 {RoomId} 开始游戏", RoomId);
        }

        /// <summary>
        /// 结束游戏
        /// </summary>
        public void EndGame()
        {
            Status = RoomStatus.WAITING;
            GameState = null;
            LandlordId = -1;
            _readyPlayers.Clear();
            _logger.LogInformation("房间 {RoomId} 游戏结束", RoomId);
        }
    }
}