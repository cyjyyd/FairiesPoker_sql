using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace FairiesPoker
{
    class Room
    {
        private string Fangzhu="";
        private string Player1="";
        private string Player2="";
        private bool Fready = false;
        private bool Pready = false;
        private bool P2ready = false;
        private Socket Fangzhusocket;
        private Socket P1socket;
        private Socket P2socket;
        private int Fangzhuexp=0;
        private int P1exp=0;
        private int P2exp=0;
        public Room(string s)
        {
            Fangzhu = s;
        }
        public Room()
        {

        }
        public string fangzhu
        {
            get { return Fangzhu; }
            set { Fangzhu = value; }
        }
        public string player1
        {
            get { return Player1; }
            set { Player1 = value;}
        }
        public string player2
        {
            get { return Player2; }
            set { Player2 = value; }
        }
        public bool fready
        {
            get { return Fready; }
            set { Fready = value; }
        }
        public bool pready
        {
            get { return Pready; }
            set { Pready = value; }
        }
        public bool p2ready
        {
            get { return P2ready; }
            set { P2ready = value; }
        }
        public Socket fangzhusocket
        {
            get { return Fangzhusocket; }
            set { Fangzhusocket = value; }
        }
        public Socket p1socket
        {
            get { return P1socket; }
            set { P1socket = value; }
        }
        public Socket p2socket
        {
            get { return P2socket; }
            set { P2socket = value; }
        }
        public int fangzhuexp
        {
            get { return Fangzhuexp; }
            set { Fangzhuexp = value; }
        }
        public int p1exp
        {
            get { return P1exp; }
            set { P1exp = value; }
        }
        public int p2exp
        {
            get { return P2exp; }
            set { P2exp = value; }
        }
        public bool readycheck()
        {
            if (fangzhu == "" || player1 == "" || player2 == "") return false;
            else if (fready && pready && p2ready) return true;
            else return false;
        }
    }
}
