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
    }
}
