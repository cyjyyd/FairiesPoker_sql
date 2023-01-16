using System.Collections;
using System.Collections.Generic;
using Protocol.Code;
using Protocol.Dto;

/// <summary>
/// 角色的网络消息处理类
/// </summary>
public class UserHandler : HandlerBase
{
    public override void OnReceive(int subCode, object value)
    {
        switch (subCode)
        {
            case UserCode.CREATE_SRES:
                createResponse((int)value);
                break;
            case UserCode.GET_INFO_SRES:
                getInfoResponse(value as UserDto);
                break;
            case UserCode.ONLINE_SRES:
                onlineResponse((int)value);
                break;
            default:
                break;
        }
    }

    private SocketMsg socketMsg = new SocketMsg();

    /// <summary>
    /// 获取信息的回应
    /// </summary>
    private void getInfoResponse(UserDto user)
    {
        if(user == null)
        {
            //没有角色
            //显示创建面板
        }
        else
        {
            //有角色
            //隐藏创建面板
            //角色上线
            //socketMsg.Change(OpCode.USER, UserCode.ONLINE_CREQ, null);
            //Dispatch(AreaCode.NET, 0, socketMsg);

            //保存服务器发来的角色数据
            //GameModel model = new GameModel();
            Models.GameModel.UserDto = user;

            //更新一下本地的显示
        }
    }

    /// <summary>
    /// 上线的响应
    /// </summary>
    /// <param name="result"></param>
    private void onlineResponse(int result)
    {
        if (result == 0)
        {
            //上线成功
        }
        else if (result == -1)
        {
            //客户端非法登录
        }
        else if(result == -2)
        {
            //没有角色 不能创建
        }
    }

    /// <summary>
    /// 创建角色的响应
    /// </summary>
    private void createResponse(int result)
    {
        if(result == -1)
        {
            //非法登录
        }
        else if(result == -2)
        {
            //已有角色
        }
        else if(result == 0)
        {
            //创建成功
            //隐藏创建面板
            //获取角色信息
            socketMsg = new SocketMsg(OpCode.USER, UserCode.GET_INFO_CREQ, null);
        }
    }
}
