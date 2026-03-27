using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Protocol.Dto
{
    [ProtoContract]
    [Serializable]
    public class ChatDto
    {
        [ProtoMember(1)]
        public int UserId;
        [ProtoMember(2)]
        public int ChatType;
        [ProtoMember(3)]
        public string Text;
        /// <summary>
        /// 发送者昵称
        /// </summary>
        [ProtoMember(4)]
        public string UserName;
        /// <summary>
        /// 目标用户ID（私聊时使用）
        /// </summary>
        [ProtoMember(5)]
        public int TargetUserId;
        /// <summary>
        /// 发送时间戳
        /// </summary>
        [ProtoMember(6)]
        public long Timestamp;

        public ChatDto()
        {

        }

        public ChatDto(int userId, int chatType)
        {
            this.UserId = userId;
            this.ChatType = chatType;
        }

        public ChatDto(int userId, string userName, string text, int chatType, int targetUserId = 0)
        {
            this.UserId = userId;
            this.UserName = userName;
            this.Text = text;
            this.ChatType = chatType;
            this.TargetUserId = targetUserId;
            this.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }

    /// <summary>
    /// 聊天类型常量
    /// </summary>
    public static class ChatTypes
    {
        public const int WORLD = 0;   // 全服频道
        public const int ROOM = 1;    // 房间频道
        public const int PRIVATE = 2; // 私聊
    }
}