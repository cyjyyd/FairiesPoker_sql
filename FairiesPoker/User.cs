using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FairiesPoker
{
    class User
    {
        private string username="";
        private int id;
        private int exp = 0;
        private int expmax;
        private int dandlion = 0;
        private int currentlevel = 0;
        private int gamecount = 0;
        private int state = 0;
        public User()
        {
            expmax = 10000;
        }
        public int ID
        {
            get { return id; }
            set { id = value; }
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
        public int State
        {
            get { return state; }
            set { state = value; }
        }
        public int Gamecount
        {
            get { return gamecount; }
            set { gamecount = value; }
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
        public int Currentlevel
        {
            get { return currentlevel; }
            set { currentlevel = value; }
        }
        public void clear()
        {
            username = "";
            id=0;
            exp = 0;
            dandlion = 0;
            currentlevel = 0;
            gamecount = 0;
            state = 0;
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
