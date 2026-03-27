using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// 客户端socket的封装
/// </summary>
public class ClientPeer
{
    private Socket socket;
    private string ip;
    private int port;

    /// <summary>
    /// 是否已连接
    /// </summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// 连接失败事件
    /// </summary>
    public event Action<string> OnConnectFailed;

    /// <summary>
    /// 连接成功事件
    /// </summary>
    public event Action OnConnectSuccess;

    /// <summary>
    /// 断开连接事件
    /// </summary>
    public event Action OnDisconnected;

    /// <summary>
    /// 构造连接对象
    /// </summary>
    /// <param name="ip">IP地址</param>
    /// <param name="port">端口号</param>
    public ClientPeer(string ip, int port)
    {
        try
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.ip = ip;
            this.port = port;
        }
        catch (Exception e)
        {
            Debug.WriteLine("在线功能初始化失败！" + e.Message);
        }
    }

    public void Connect()
    {
        try
        {
            // 使用 IPAddress.Parse 直接解析，避免 Dns.GetHostEntry 返回 IPv6
            IPAddress ipAddress;
            if (!IPAddress.TryParse(ip, out ipAddress))
            {
                // 如果不是IP地址，尝试DNS解析
                var hostEntry = Dns.GetHostEntry(ip);
                // 优先获取 IPv4 地址
                ipAddress = Array.Find(hostEntry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
                if (ipAddress == null && hostEntry.AddressList.Length > 0)
                {
                    ipAddress = hostEntry.AddressList[0];
                }
            }

            if (ipAddress == null)
            {
                IsConnected = false;
                OnConnectFailed?.Invoke("无法解析服务器地址");
                return;
            }

            var endPoint = new IPEndPoint(ipAddress, port);
            socket.Connect(endPoint);

            IsConnected = true;
            Debug.WriteLine("连接服务器成功！");
            OnConnectSuccess?.Invoke();

            //开始异步接收数据
            startReceive();
        }
        catch (Exception e)
        {
            IsConnected = false;
            Debug.WriteLine("连接失败：" + e.Message);
            OnConnectFailed?.Invoke("连接服务器失败: " + e.Message);
        }
    }

    #region 接收数据

    //接收的数据缓冲区
    private byte[] receiveBuffer = new byte[1024];

    /// <summary>
    /// 一旦接收到数据，就存到缓存区里面
    /// </summary>
    private List<byte> dataCache = new List<byte>();

    private bool isProcessReceive = false;

    public Queue<SocketMsg> socketMsgQueue = new Queue<SocketMsg>();

    /// <summary>
    /// 开始异步接收数据
    /// </summary>
    private void startReceive()
    {
        if (socket == null || !socket.Connected)
        {
            Debug.WriteLine("没有连接成功，无法接收数据");
            return;
        }

        try
        {
            socket.BeginReceive(receiveBuffer, 0, 1024, SocketFlags.None, receiveCallBack, socket);
        }
        catch (Exception e)
        {
            Debug.WriteLine("开始接收数据失败: " + e.Message);
            HandleDisconnect();
        }
    }

    /// <summary>
    /// 收到消息的回调
    /// </summary>
    /// <param name="ar"></param>
    private void receiveCallBack(IAsyncResult ar)
    {
        try
        {
            int length = socket.EndReceive(ar);
            if (length == 0)
            {
                // 服务器断开连接
                HandleDisconnect();
                return;
            }

            byte[] tmpByteArray = new byte[length];
            Buffer.BlockCopy(receiveBuffer, 0, tmpByteArray, 0, length);

            //处理收到的数据
            dataCache.AddRange(tmpByteArray);
            if (isProcessReceive == false)
                processReceive();

            startReceive();
        }
        catch (SocketException ex)
        {
            // Socket错误
            Debug.WriteLine("Socket错误: " + ex.SocketErrorCode);
            HandleDisconnect();
        }
        catch (Exception e)
        {
            Debug.WriteLine("接收数据异常: " + e.Message);
            HandleDisconnect();
        }
    }

    /// <summary>
    /// 处理断开连接
    /// </summary>
    private void HandleDisconnect()
    {
        if (!IsConnected) return;
        IsConnected = false;
        OnDisconnected?.Invoke();
    }

    /// <summary>
    /// 处理收到的数据
    /// </summary>
    private void processReceive()
    {
        isProcessReceive = true;
        //解析数据包
        byte[] data = EncodeTool.DecodePacket(ref dataCache);

        if (data == null)
        {
            isProcessReceive = false;
            return;
        }

        SocketMsg msg = EncodeTool.DecodeMsg(data);

        //存储消息 等待处理
        socketMsgQueue.Enqueue(msg);

        //尾递归
        processReceive();
    }

    #endregion

    #region 发送数据

    public void Send(int opCode, int subCode, object value)
    {
        SocketMsg msg = new SocketMsg(opCode, subCode, value);

        Send(msg);
    }

    public void Send(SocketMsg msg)
    {
        if (socket == null || !socket.Connected)
        {
            Debug.WriteLine("未连接，无法发送数据");
            return;
        }

        byte[] data = EncodeTool.EncodeMsg(msg);
        byte[] packet = EncodeTool.EncodePacket(data);

        try
        {
            socket.Send(packet);//如果卡顿改成异步方法beginSend
        }
        catch (Exception e)
        {
            Debug.WriteLine("发送数据失败: " + e.Message);
            HandleDisconnect();
        }
    }

    #endregion

    /// <summary>
    /// 安全关闭连接（先尝试发送登出请求）
    /// </summary>
    /// <param name="logoutMessage">登出消息（可选）</param>
    public void SafeClose(SocketMsg logoutMessage = null)
    {
        try
        {
            // 先发送登出消息
            if (logoutMessage != null && socket != null && socket.Connected)
            {
                try
                {
                    byte[] data = EncodeTool.EncodeMsg(logoutMessage);
                    byte[] packet = EncodeTool.EncodePacket(data);
                    socket.Send(packet);
                    Debug.WriteLine("已发送登出消息");
                }
                catch (Exception e)
                {
                    Debug.WriteLine("发送登出消息失败: " + e.Message);
                }
            }

            // 短暂等待消息发送
            System.Threading.Thread.Sleep(100);

            // 关闭连接
            IsConnected = false;
            socket?.Shutdown(SocketShutdown.Both);
            socket?.Close();
            Debug.WriteLine("连接已安全关闭");
        }
        catch (Exception e)
        {
            Debug.WriteLine("安全关闭连接失败: " + e.Message);
        }
    }

    /// <summary>
    /// 关闭连接
    /// </summary>
    public void Close()
    {
        try
        {
            IsConnected = false;
            socket?.Shutdown(SocketShutdown.Both);
            socket?.Close();
        }
        catch { }
    }
}
