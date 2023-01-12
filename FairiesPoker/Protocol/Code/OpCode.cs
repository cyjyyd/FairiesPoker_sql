using System;
using System.Collections.Generic;

namespace Protocol.Code
{
    public class OpCode
    {
        public const byte ACCOUNT = 0;//账号模块
        public const byte USER = 1;//用户模块
        public const byte MATCH = 2;//匹配模块
        public const byte CHAT = 3;//聊天模块
        public const byte FIGHT = 4;//战斗模块
    }
}
