using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Protocol.Dto.Fight
{
    [ProtoContract]
    [Serializable]
    public class GrabDto
    {
        [ProtoMember(1)]
        public int UserId;
        [ProtoMember(2)]
        public List<CardDto> TableCardList;
        [ProtoMember(3)]
        public List<CardDto> PlayerCardList;

        public GrabDto()
        {

        }

        public GrabDto(int userId, List<CardDto> cards, List<CardDto> playerCards)
        {
            this.UserId = userId;
            this.TableCardList = cards;
            this.PlayerCardList = playerCards;
        }
    }
}