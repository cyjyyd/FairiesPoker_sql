using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public ChatDto()
        {

        }

        public ChatDto(int userId, int chatType)
        {
            this.UserId = userId;
            this.ChatType = chatType;
        }
    }
}