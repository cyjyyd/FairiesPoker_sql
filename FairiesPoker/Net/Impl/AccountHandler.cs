using Protocol.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AccountHandler : HandlerBase
{
    public override void OnReceive(int subCode, object value)
    {
        switch (subCode)
        {
            case AccountCode.LOGIN:
                loginResponse((int)value);
                break;
            case AccountCode.REGIST_SRES:
                registResponse((int)value);
                //registResponse(value.ToString());
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
        switch (result)
        {
            case 0:
                //跳转场景
                break;
            case -1:
                //账号不存在
                break;
            case -2:
                //账号已登录
                break;
            case -3:
                //账号密码不匹配
                break;
            default:
                break;
        }

        //if(result == "登录成功")
        //{
        //    promptMsg.Change(result.ToString(), Color.green);
        //    Dispatch(AreaCode.UI, UIEvent.PROMPT_MSG, promptMsg);
        //    //跳转场景
        //    //TODO
        //    return;
        //}

        ////登录错误
        //promptMsg.Change(result.ToString(), Color.red);
        //Dispatch(AreaCode.UI, UIEvent.PROMPT_MSG, promptMsg);
    }

    /// <summary>
    /// 注册响应
    /// </summary>
    private void registResponse(int result)
    {
        switch (result)
        {
            case 0:
                //注册成功
                break;
            case -1:
                //账号已存在
                break;
            case -2:
                //账号输入不合法
                break;
            case -3:
                //密码输入不合法
                break;
            default:
                break;
        }

        //if (result == "注册成功")
        //{
        //    promptMsg.Change(result.ToString(), Color.green);
        //    Dispatch(AreaCode.UI, UIEvent.PROMPT_MSG, promptMsg);
        //    //跳转场景
        //    //TODO
        //    return;
        //}

        ////注册错误
        //promptMsg.Change(result.ToString(), Color.red);
        //Dispatch(AreaCode.UI, UIEvent.PROMPT_MSG, promptMsg);
    }
}
