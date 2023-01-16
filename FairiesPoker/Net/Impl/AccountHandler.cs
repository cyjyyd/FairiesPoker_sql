using FairiesPoker;
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
                loginResponse(Convert.ToInt32(value));
                break;
            case AccountCode.REGIST_SRES:
                registResponse(Convert.ToInt32(value));
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
                //TO DO:跳转场景
                break;
            case -1:
                //账号不存在
                System.Windows.Forms.MessageBox.Show("登录失败：该账号不存在！","警告",System.Windows.Forms.MessageBoxButtons.OK,System.Windows.Forms.MessageBoxIcon.Warning);
                break;
            case -2:
                System.Windows.Forms.MessageBox.Show("登录失败：该账号已登录！", "警告", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                //账号已登录
                break;
            case -3:
                System.Windows.Forms.MessageBox.Show("登录失败：请检查您的用户名或密码！", "警告", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
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
                System.Windows.Forms.MessageBox.Show("恭喜,注册成功！请牢记您的账号和密码！","Success",System.Windows.Forms.MessageBoxButtons.OK,System.Windows.Forms.MessageBoxIcon.Information);
                //注册成功
                break;
            case -1:
                System.Windows.Forms.MessageBox.Show("该账号已经被注册，请换一个用户名吧", "Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                //账号已存在
                break;
            case -2:
                System.Windows.Forms.MessageBox.Show("用户名非法输入", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                //账号输入不合法
                break;
            case -3:
                System.Windows.Forms.MessageBox.Show("接收到的密码格式不合法，如出现此提示，请更新", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
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
