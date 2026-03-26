using Protocol.Constant;
using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Protocol.Dto.Fight
{
    [ProtoContract]
    [Serializable]
    public class DealDto
    {
        /// <summary>
        /// 选中要出的牌
        /// </summary>
        [ProtoMember(1)]
        public List<CardDto> SelectCardList;
        /// <summary>
        /// 长度
        /// </summary>
        [ProtoMember(2)]
        public int Length;
        /// <summary>
        /// 权值
        /// </summary>
        [ProtoMember(3)]
        public int Weight;
        /// <summary>
        /// 类型
        /// </summary>
        [ProtoMember(4)]
        public int Type;

        /// <summary>
        /// 谁出的牌
        /// </summary>
        [ProtoMember(5)]
        public int UserId;
        /// <summary>
        /// 牌是否合法
        /// </summary>
        [ProtoMember(6)]
        public bool IsRegular;
        /// <summary>
        /// 剩余的手牌
        /// </summary>
        [ProtoMember(7)]
        public List<CardDto> RemainCardList;

        public DealDto()
        {

        }

        public DealDto(List<CardDto> cardList, int uid)
        {
            this.SelectCardList = cardList;
            this.Length = cardList.Count;
            this.Type = CardType.GetCardType(cardList);
                //是不是单牌
                //是不是对儿
                //是不是顺子
                //是不是。。。
            this.Weight = CardWeight.GetWeight(cardList, this.Type);
            this.UserId = uid;
            this.IsRegular = (this.Type != CardType.NONE);
            this.RemainCardList = new List<CardDto>();
        }
    }
}