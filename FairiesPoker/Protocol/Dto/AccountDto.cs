using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Dto
{
    [Serializable]
    public class AccountDto
    {
        public string Account;
        public string Password;
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
