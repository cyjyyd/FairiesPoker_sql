using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FairiesPoker
{
    class User
    {
        private string username;
        private int exp;
        private int expmax;
        private int dandlion;
        private string token;
        public User()
        {
            expmax = 10000;
        }
        public string UserName
        {
            get { return username; }
            set { username = value; }
        }
        public int Exp
        {
            get { return exp; }
            set { exp = value; }
        }
        public int Expmax
        {
            get { return expmax; }
        }
        public int Dandlion
        {
            get { return dandlion; }
            set { dandlion = value; }
        }
        public string Token
        {
            get { return token; }
            set { token = value; }
        }
        public int judgelevel (int exp)
        {
            if (exp <= 100)
            {
                return 1;
            }
            else if (exp > 100 & exp <= 200)
            {
                return 2;
            }
            else if (exp > 200 & exp <= 500)
            {
                return 3;
            }
            else if (exp > 500 & exp <= 1000)
            {
                return 4;
            }
            else if (exp > 1000 & exp <= 2000)
            {
                return 5;
            }
            else if (exp > 2000 & exp <= 3000)
            {
                return 6;
            }
            else if (exp > 3000 & exp <= 6000)
            {
                return 7;
            }
            else if (exp > 6000 & exp <= 10000)
            {
                return 8;
            }
            else return 0;
        }
        public string judgestatus(bool ready)
        {
            if (ready)
            {
                return "准备";
            }
            else
            {
                return "在线";
            }
        }
    }
}
