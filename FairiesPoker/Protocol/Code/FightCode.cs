using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Code
{
    public class FightCode
    {
        public const byte GRAB_LANDLORD_CREQ = 0;//客户端发起抢地主的请求
        public const byte GRAB_LANDLORD_BRO = 1;//服务器广播抢地主的结果
        public const byte TURN_GRAB_BRO = 2;//服务器广播下一个玩家抢地主的结果

        public const byte DEAL_CREQ = 3;//客户端发起出牌的请求
        public const byte DEAL_SRES = 4;//服务器给客户端出牌的响应
        public const byte DEAL_BRO = 5;//服务器广播出牌的结果

        public const byte PASS_CREQ = 6;//客户端发起不出的请求
        public const byte PASS_SRES = 7;//服务器发给客户端不出的响应

        public const byte TURN_DEAL_BRO = 8;//服务器广播转换出牌的结果

        public const byte LEAVE_BRO = 9;//服务器广播有玩家退出游戏

        public const byte OVER_BRO = 10;//服务器广播游戏结束

        public const byte GET_CARD_SRES = 11;//服务器给客户端卡牌的响应

        public const byte REFRESH_MULTIPLE = 12;//刷新倍数
    }
}
