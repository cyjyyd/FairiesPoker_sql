using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Protocol.Dto
{
    /// <summary>
    /// 用户数据的传输模型
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class UserDto
    {
        [ProtoMember(1)]
        public int Id;//由于游戏满足不了需求 所以定义了这个id
        [ProtoMember(2)]
        public string Name;//角色名字
        [ProtoMember(3)]
        public int Been;//豆子的数量
        [ProtoMember(4)]
        public int WinCount;//胜场
        [ProtoMember(5)]
        public int LoseCount;//负场
        [ProtoMember(6)]
        public int RunCount;//逃跑场
        [ProtoMember(7)]
        public int Lv;//等级
        [ProtoMember(8)]
        public int Exp;//经验

        public UserDto()
        {

        }

        public UserDto(int id, string name, int been, int winCount, int loseCount, int runCount, int lv, int exp)
        {
            this.Id = id;
            this.Name = name;
            this.Been = been;
            this.WinCount = winCount;
            this.LoseCount = loseCount;
            this.RunCount = runCount;
            this.Lv = lv;
            this.Exp = exp;
        }
    }
}