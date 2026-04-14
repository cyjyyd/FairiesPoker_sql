using FairiesPoker;
using Protocol.Code;
using Protocol.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FairiesPoker.MG.Network.Impl
{

public class AccountHandler : HandlerBase
{
    public override void OnReceive(int subCode, object value)
    {
        switch (subCode)
        {
            case AccountCode.LOGIN:
                loginResponse(Convert.ToInt32(value));
                break;
            case AccountCode.REGIST_SRES:
                registResponse(Convert.ToInt32(value));
                break;
            case AccountCode.CHANGE_PASSWORD_SRES:
                changePasswordResponse(Convert.ToInt32(value));
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 登录响应
    /// </summary>
    private void loginResponse(int result)
    {
        // 登录结果由UserHandler的onlineResponse处理
        // 0=成功, -1=不存在, -2=已登录, -3=密码错误
        Models.TriggerLoginResult(result);
    }

    /// <summary>
    /// 注册响应
    /// </summary>
    private void registResponse(int result)
    {
        switch (result)
        {
            case 0:
                Models.TriggerRegisterResult(true);
                break;
            case -1:
            case -2:
            case -3:
                Models.TriggerRegisterResult(false);
                break;
            default:
                Models.TriggerRegisterResult(false);
                break;
        }
    }

    /// <summary>
    /// 修改密码响应
    /// </summary>
    private void changePasswordResponse(int result)
    {
        switch (result)
        {
            case 0:
                Models.TriggerChangePasswordResult(true, "密码修改成功！");
                break;
            case -1:
                Models.TriggerChangePasswordResult(false, "用户名不存在");
                break;
            case -2:
                Models.TriggerChangePasswordResult(false, "旧密码错误");
                break;
            case -3:
                Models.TriggerChangePasswordResult(false, "服务器错误，请稍后重试");
                break;
            case -4:
                Models.TriggerChangePasswordResult(false, "新密码不能与旧密码相同");
                break;
            default:
                Models.TriggerChangePasswordResult(false, "修改失败，请重试");
                break;
        }
    }
}
}
