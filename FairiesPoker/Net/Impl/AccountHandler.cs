using FairiesPoker;
using Protocol.Code;
using Protocol.Dto;
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
                // 登录成功，跳转到Lobby由UserHandler.onlineResponse处理
                break;
            case -1:
                System.Windows.Forms.MessageBox.Show("登录失败：该账号不存在！", "警告", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                break;
            case -2:
                System.Windows.Forms.MessageBox.Show("登录失败：该账号已登录！", "警告", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                break;
            case -3:
                System.Windows.Forms.MessageBox.Show("登录失败：请检查您的用户名或密码！", "警告", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 注册响应
    /// </summary>
    private void registResponse(int result)
    {
        switch (result)
        {
            case 0:
                // 注册成功，由Register.cs处理提示和关闭窗口
                Models.TriggerRegisterResult(true);
                break;
            case -1:
                Models.TriggerRegisterResult(false);
                System.Windows.Forms.MessageBox.Show("该账号已经被注册，请换一个用户名吧", "警告", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                break;
            case -2:
                Models.TriggerRegisterResult(false);
                System.Windows.Forms.MessageBox.Show("用户名长度需要在4-16个字符之间", "错误", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                break;
            case -3:
                Models.TriggerRegisterResult(false);
                System.Windows.Forms.MessageBox.Show("服务器错误，请稍后重试", "错误", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                break;
            default:
                Models.TriggerRegisterResult(false);
                break;
        }
    }
}
