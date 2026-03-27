using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Code
{
    /// <summary>
    /// 房间模块操作码（使用MATCH模块）
    /// 这里扩展MatchCode以支持新的房间功能
    /// </summary>
    public static class RoomCode
    {
        // 房间列表
        public const int GET_ROOMS_CREQ = 20;
        public const int GET_ROOMS_SRES = 21;

        // 创建房间
        public const int CREATE_CREQ = 22;
        public const int CREATE_SRES = 23;

        // 加入房间
        public const int JOIN_CREQ = 24;
        public const int JOIN_SRES = 25;
        public const int JOIN_BRO = 26;

        // 离开房间
        public const int LEAVE_CREQ = 27;
        public const int LEAVE_BRO = 28;

        // 房间玩家更新
        public const int UPDATE_BRO = 29;
    }
}