using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace FPServer.Network
{
    /// <summary>
    /// 客户端连接封装
    /// </summary>
    public class ClientConnection
    {
        private readonly Socket _socket;
        private readonly ServerPeer _server;
        private readonly ILogger<ClientConnection> _logger;
        private readonly byte[] _receiveBuffer = new byte[1024];
        private List<byte> _dataCache = new List<byte>();
        private bool _isProcessing = false;
        public int UserId { get; set; } = -1;
        public string Username { get; set; }
        public DateTime LastActiveTime { get; set; }
        public bool IsConnected => _socket != null && _socket.Connected;

        public ClientConnection(Socket socket, ServerPeer server, ILogger<ClientConnection> logger)
        {
            _socket = socket;
            _server = server;
            _logger = logger;
            LastActiveTime = DateTime.Now;
        }

        /// <summary>
        /// 开始接收数据
        /// </summary>
        public void StartReceive()
        {
            BeginReceive();
        }

        private void BeginReceive()
        {
            try
            {
                if (!IsConnected) return;
                _socket.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "接收数据失败");
                Disconnect();
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int length = _socket.EndReceive(ar);
                if (length == 0)
                {
                    // 客户端断开连接
                    _logger.LogInformation("客户端断开连接: {UserId}", UserId);
                    Disconnect();
                    return;
                }

                byte[] tmpArray = new byte[length];
                Buffer.BlockCopy(_receiveBuffer, 0, tmpArray, 0, length);
                _dataCache.AddRange(tmpArray);

                if (!_isProcessing)
                {
                    ProcessReceive();
                }

                BeginReceive();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "接收回调失败");
                Disconnect();
            }
        }

        private void ProcessReceive()
        {
            _isProcessing = true;
            byte[] data = EncodeTool.DecodePacket(ref _dataCache);
            if (data == null)
            {
                _isProcessing = false;
                return;
            }

            try
            {
                var msg = EncodeTool.DecodeMsg(data);
                LastActiveTime = DateTime.Now;
                _server.HandleMessage(this, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理消息失败");
            }

            ProcessReceive();
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        public void Send(SocketMsg msg)
        {
            if (!IsConnected) return;

            try
            {
                byte[] data = EncodeTool.EncodeMsg(msg);
                byte[] packet = EncodeTool.EncodePacket(data);
                _socket.BeginSend(packet, 0, packet.Length, SocketFlags.None, SendCallback, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送消息失败");
                Disconnect();
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                _socket.EndSend(ar);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送回调失败");
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _socket?.Shutdown(SocketShutdown.Both);
                _socket?.Close();
            }
            catch { }

            _server.RemoveClient(this);
        }
    }
}