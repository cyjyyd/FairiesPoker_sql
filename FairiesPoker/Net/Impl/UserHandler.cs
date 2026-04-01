using System.Collections;
using System.Collections.Generic;
using Protocol.Code;
using Protocol.Dto;
using FairiesPoker;
using System.IO;
using System.Windows.Forms;

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
                onlineResponse(value as UserDto);
                break;
            case UserCode.GET_ONLINE_USERS_SRES:
                getOnlineUsersResponse(value as List<UserDto>);
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
            //保存服务器发来的角色数据
            Models.GameModel.UserDto = user;

            //更新一下本地的显示
        }
    }

    /// <summary>
    /// 上线的响应（登录成功后会收到）
    /// </summary>
    private void onlineResponse(UserDto user)
    {
        if (user != null)
        {
            // 保存用户数据
            Models.GameModel.UserDto = user;

            // 检查是否有待上传的临时头像
            UploadPendingAvatar();

            // 跳转到游戏大厅
            Lobby lobby = new Lobby(NetManager.Instance);
            lobby.Show();

            // 标记登录成功并关闭登录窗口
            if (Application.OpenForms["Login"] is Login loginForm)
            {
                loginForm.MarkLoginSuccess();
                loginForm.Close();
            }
        }
    }

    /// <summary>
    /// 上传待处理的头像（注册时选择的头像）
    /// </summary>
    private void UploadPendingAvatar()
    {
        try
        {
            string tempPath = System.IO.Path.Combine(Application.StartupPath, "temp_avatar.dat");
            if (System.IO.File.Exists(tempPath))
            {
                byte[] avatarData = System.IO.File.ReadAllBytes(tempPath);
                if (avatarData != null && avatarData.Length > 0)
                {
                    var dto = new AvatarDto(avatarData, "avatar.jpg");
                    var msg = new SocketMsg(OpCode.AVATAR, AvatarCode.UPLOAD_CREQ, dto);
                    NetManager.Instance.Execute(0, msg);
                }
                // 上传后删除临时文件
                System.IO.File.Delete(tempPath);
            }
        }
        catch
        {
            // 忽略错误
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

    /// <summary>
    /// 获取在线用户列表响应
    /// </summary>
    private void getOnlineUsersResponse(List<UserDto> users)
    {
        if (users != null)
        {
            Models.TriggerOnlineUsers(users);
        }
    }
}
