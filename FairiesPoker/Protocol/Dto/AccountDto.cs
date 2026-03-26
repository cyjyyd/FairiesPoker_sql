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
    public class AccountDto
    {
        [ProtoMember(1)]
        public string Account;
        [ProtoMember(2)]
        public string Password;
        [ProtoMember(3)]
        public int avatarid;
        public AccountDto()
        {

        }

        public AccountDto(string acc, string pwd, int avatarid)
        {
            this.Account = acc;
            this.Password = pwd;
            this.avatarid = avatarid;
        }
        public AccountDto(string acc,string pwd)
        {
            this.Account = acc;
            this.Password = pwd;
        }
    }
}