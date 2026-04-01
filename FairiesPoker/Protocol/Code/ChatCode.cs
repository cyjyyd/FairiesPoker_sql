using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Code
{
    /// <summary>
    /// 聊天模块操作码
    /// </summary>
    public class ChatCode
    {
        /// <summary>
        /// 发送聊天消息请求
        /// </summary>
        public const int SEND_CREQ = 0;

        /// <summary>
        /// 发送聊天消息响应
        /// </summary>
        public const int SEND_SRES = 1;

        /// <summary>
        /// 收到聊天消息广播
        /// </summary>
        public const int RECEIVE_BRO = 2;

        /// <summary>
        /// 获取历史消息请求
        /// </summary>
        public const int GET_HISTORY_CREQ = 3;

        /// <summary>
        /// 获取历史消息响应
        /// </summary>
        public const int GET_HISTORY_SRES = 4;

        /// <summary>
        /// 登录时推送今日消息
        /// </summary>
        public const int PUSH_TODAY_SRES = 5;
    }
}