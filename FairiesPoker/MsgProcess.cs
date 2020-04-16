using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace FairiesPoker
{
    class MsgProcess
    {
        public byte[] SendCon(Dictionary<string,string> dic)
        {
            string jstr = JsonConvert.SerializeObject(dic);
            byte[] send = Encoding.UTF8.GetBytes(jstr);
            return send;
        }
        public Dictionary<string,string> ReCon(byte[] rec,int index,int count)
        {
            string jstr = Encoding.UTF8.GetString(rec, index, count);
            Dictionary<string, string> receive = JsonConvert.DeserializeObject<Dictionary<string, string>>(jstr);
            return receive;
        }
    }
}
