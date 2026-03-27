using FPServer.Network;

namespace FPServer.Cache
{
    /// <summary>
    /// 在线用户缓存
    /// </summary>
    public class OnlineUserCache
    {
        private readonly Dictionary<int, ClientConnection> _userConnections = new();
        private readonly Dictionary<int, Protocol.Dto.UserDto> _userDatas = new();
        private readonly object _lock = new();

        /// <summary>
        /// 添加在线用户
        /// </summary>
        public void AddUser(int userId, ClientConnection client, Protocol.Dto.UserDto userData)
        {
            lock (_lock)
            {
                client.UserId = userId;
                client.Username = userData.Name;
                _userConnections[userId] = client;
                _userDatas[userId] = userData;
            }
        }

        /// <summary>
        /// 移除在线用户
        /// </summary>
        public void RemoveUser(int userId)
        {
            lock (_lock)
            {
                _userConnections.Remove(userId);
                _userDatas.Remove(userId);
            }
        }

        /// <summary>
        /// 获取客户端连接
        /// </summary>
        public ClientConnection GetClient(int userId)
        {
            lock (_lock)
            {
                return _userConnections.TryGetValue(userId, out var client) ? client : null;
            }
        }

        /// <summary>
        /// 获取用户数据
        /// </summary>
        public Protocol.Dto.UserDto GetUserData(int userId)
        {
            lock (_lock)
            {
                return _userDatas.TryGetValue(userId, out var data) ? data : null;
            }
        }

        /// <summary>
        /// 更新用户数据
        /// </summary>
        public void UpdateUserData(int userId, Protocol.Dto.UserDto userData)
        {
            lock (_lock)
            {
                if (_userDatas.ContainsKey(userId))
                {
                    _userDatas[userId] = userData;
                }
            }
        }

        /// <summary>
        /// 检查用户是否在线
        /// </summary>
        public bool IsOnline(int userId)
        {
            lock (_lock)
            {
                return _userConnections.ContainsKey(userId);
            }
        }

        /// <summary>
        /// 获取所有在线用户ID
        /// </summary>
        public List<int> GetAllOnlineUserIds()
        {
            lock (_lock)
            {
                return _userConnections.Keys.ToList();
            }
        }

        /// <summary>
        /// 获取在线用户数
        /// </summary>
        public int GetOnlineCount()
        {
            lock (_lock)
            {
                return _userConnections.Count;
            }
        }
    }
}