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
    public class OverDto
    {
        [ProtoMember(1)]
        public int WinIdentity;
        [ProtoMember(2)]
        public List<int> WinUIdList;
        [ProtoMember(3)]
        public int BeenCount;

        public OverDto()
        {

        }
    }
}