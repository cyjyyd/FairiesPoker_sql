using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Code
{
    public class AccountCode
    {
        //注册的操作码
        public const int REGIST_CREQ = 0;//client request //参数 AccountDto 账号密码
        public const int REGIST_SRES = 1;//server response

        //登录的操作码
        public const int LOGIN = 2;//参数 AccountDto 账号密码

        //登出的操作码
        public const int LOGOUT_CREQ = 3;//client request 登出请求
        public const int LOGOUT_SRES = 4;//server response 登出响应

        //修改密码的操作码
        public const int CHANGE_PASSWORD_CREQ = 5;//client request 修改密码请求
        public const int CHANGE_PASSWORD_SRES = 6;//server response 修改密码响应
    }
}
