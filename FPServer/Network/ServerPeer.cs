using FPServer.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace FPServer.Network
{
    /// <summary>
    /// 服务端Socket封装
    /// </summary>
    public class ServerPeer
    {
        private Socket _listenSocket;
        private readonly ILogger<ServerPeer> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _configuration;
        private readonly int _port;
        private readonly string _host;
        private readonly List<ClientConnection> _clients = new List<ClientConnection>();
        private readonly object _clientsLock = new object();
        private readonly MessageHandler _messageHandler;

        public ServerPeer(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<ServerPeer>();
            _host = configuration["Server:Host"] ?? "0.0.0.0";
            _port = int.Parse(configuration["Server:Port"] ?? "40960");
            _messageHandler = new MessageHandler(this, loggerFactory, configuration);
        }

        /// <summary>
        /// 获取头像处理器（用于控制台命令）
        /// </summary>
        public AvatarHandler GetAvatarHandler()
        {
            return _messageHandler.GetAvatarHandler();
        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        public void Start()
        {
            try
            {
                _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var endPoint = new IPEndPoint(IPAddress.Parse(_host), _port);
                _listenSocket.Bind(endPoint);
                _listenSocket.Listen(100);

                _logger.LogInformation("服务器启动成功，监听 {Host}:{Port}", _host, _port);

                // 开始接受连接
                BeginAccept();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "服务器启动失败");
                throw;
            }
        }

        private void BeginAccept()
        {
            try
            {
                _listenSocket.BeginAccept(AcceptCallback, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "接受连接失败");
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                var clientSocket = _listenSocket.EndAccept(ar);
                var clientEndPoint = clientSocket.RemoteEndPoint as IPEndPoint;
                _logger.LogInformation("新客户端连接: {EndPoint}", clientEndPoint);

                var client = new ClientConnection(clientSocket, this, _loggerFactory.CreateLogger<ClientConnection>());
                lock (_clientsLock)
                {
                    _clients.Add(client);
                }
                client.StartReceive();

                // 继续接受下一个连接
                BeginAccept();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "接受连接回调失败");
                BeginAccept();
            }
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        public void HandleMessage(ClientConnection client, SocketMsg msg)
        {
            _messageHandler.HandleMessage(client, msg);
        }

        /// <summary>
        /// 移除客户端
        /// </summary>
        public void RemoveClient(ClientConnection client)
        {
            lock (_clientsLock)
            {
                _clients.Remove(client);
            }

            // 处理用户下线
            if (client.UserId > 0)
            {
                _messageHandler.HandleUserOffline(client);
            }
        }

        /// <summary>
        /// 广播消息给所有客户端
        /// </summary>
        public void Broadcast(SocketMsg msg)
        {
            lock (_clientsLock)
            {
                foreach (var client in _clients)
                {
                    client.Send(msg);
                }
            }
        }

        /// <summary>
        /// 广播消息给指定客户端
        /// </summary>
        public void BroadcastTo(List<int> userIds, SocketMsg msg)
        {
            lock (_clientsLock)
            {
                foreach (var client in _clients.Where(c => userIds.Contains(c.UserId)))
                {
                    client.Send(msg);
                }
            }
        }

        /// <summary>
        /// 获取在线用户数
        /// </summary>
        public int GetOnlineCount()
        {
            lock (_clientsLock)
            {
                return _clients.Count;
            }
        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        public void Stop()
        {
            try
            {
                lock (_clientsLock)
                {
                    foreach (var client in _clients.ToList())
                    {
                        client.Disconnect();
                    }
                    _clients.Clear();
                }

                _listenSocket?.Close();
                _logger.LogInformation("服务器已停止");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止服务器失败");
            }
        }
    }
}