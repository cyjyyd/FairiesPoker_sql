using Protocol.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using FairiesPoker.MG.Network.Impl;

namespace FairiesPoker.MG.Network
{

/// <summary>
/// 网络模块
/// </summary>
public class NetManager
{
    bool debug = true;
    public static NetManager Instance = null;
    private ClientPeer client;

    /// <summary>
    /// 是否已连接
    /// </summary>
    public bool IsConnected => client?.IsConnected ?? false;

    /// <summary>
    /// 连接状态变化事件
    /// </summary>
    public event Action<bool, string> OnConnectionStateChanged;

    public NetManager()
    {
        // 设置静态实例
        Instance = this;
    }

    public void Start()
    {
        debug = true;
        if (debug)
        {
            // 直接使用 127.0.0.1，避免 DNS 解析问题
            client = new ClientPeer("127.0.0.1", 40960);
        }
        else
        {
            client = new ClientPeer("www.fairybcd.top", 40960);
        }

        // 订阅连接事件
        client.OnConnectSuccess += () =>
        {
            OnConnectionStateChanged?.Invoke(true, "连接服务器成功");
        };

        client.OnConnectFailed += (msg) =>
        {
            OnConnectionStateChanged?.Invoke(false, msg);
        };

        client.OnDisconnected += () =>
        {
            OnConnectionStateChanged?.Invoke(false, "与服务器断开连接");
        };

        client.Connect();
    }

    public void Update()
    {
        if (client == null)
            return;

        while (client.socketMsgQueue.Count > 0)
        {
            SocketMsg msg = client.socketMsgQueue.Dequeue();
            //处理消息
            processSocketMsg(msg);
        }
    }

    #region 处理接收到的服务器发来的消息

    HandlerBase accountHandler = new AccountHandler();
    HandlerBase userHandler = new UserHandler();
    HandlerBase matchHandler = new MatchHandler();
    HandlerBase chatHandler = new ChatHandler();
    HandlerBase fightHandler = new FightHandler();
    HandlerBase avatarHandler = new AvatarHandler();

    /// <summary>
    /// 接受网络的消息
    /// </summary>
    public void processSocketMsg(SocketMsg msg)
    {
        switch (msg.OpCode)
        {
            case OpCode.ACCOUNT:
                accountHandler.OnReceive(msg.SubCode, msg.Value);
                break;
            case OpCode.USER:
                userHandler.OnReceive(msg.SubCode, msg.Value);
                break;
            case OpCode.MATCH:
                matchHandler.OnReceive(msg.SubCode, msg.Value);
                break;
            case OpCode.CHAT:
                chatHandler.OnReceive(msg.SubCode, msg.Value);
                break;
            case OpCode.FIGHT:
                fightHandler.OnReceive(msg.SubCode, msg.Value);
                break;
            case OpCode.AVATAR:
                avatarHandler.OnReceive(msg.SubCode, msg.Value);
                break;
            default:
                break;
        }
    }

    #endregion


    #region 处理客户端内部 给服务器发消息的 事件
    public void Execute(int eventCode, object message)
    {
        switch (eventCode)
        {
            case 0:
                client.Send(message as SocketMsg);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 安全断开连接（先发送登出消息再断开）
    /// </summary>
    /// <param name="logoutMessage">登出消息（可选）</param>
    public void SafeDisconnect(SocketMsg logoutMessage = null)
    {
        if (client != null)
        {
            client.SafeClose(logoutMessage);
        }
    }

    /// <summary>
    /// 直接断开连接
    /// </summary>
    public void Disconnect()
    {
        if (client != null)
        {
            client.Close();
        }
    }
    #endregion
}

}
