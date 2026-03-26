using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Protocol.Dto.Fight
{
    /// <summary>
    /// 表示卡牌
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class CardDto
    {
        [ProtoMember(1)]
        public string Name;
        [ProtoMember(2)]
        public int Color;//红桃
        [ProtoMember(3)]
        public int Weight;//9

        public CardDto()
        {

        }

        public CardDto(string name, int color, int weight)
        {
            this.Name = name;
            this.Color = color;
            this.Weight = weight;
        }
    }
}