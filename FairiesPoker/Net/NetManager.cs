using Protocol.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

/// <summary>
/// 网络模块
/// </summary>
public class NetManager
{
    bool debug = true;
    public static NetManager Instance = null;
    public static IPHostEntry IPinfo;
    private ClientPeer client;

    public void Start()
    {
        debug = true;
        if (debug)
        {
            IPinfo = Dns.GetHostEntry("127.0.0.1");
            client = new ClientPeer(IPinfo.AddressList[0].ToString(), 40960);

        }
        else
        {
            IPinfo = Dns.GetHostEntry("www.fairybcd.top");
            client = new ClientPeer(IPinfo.AddressList[0].ToString(), 40960);
        }
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
    #endregion
}

