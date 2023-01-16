using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace FairiesPoker
{
    public static class MsgProcess
    {
        public static byte[] SendCon(object value)
        {
            string jstr = JsonConvert.SerializeObject(value);
            byte[] send = Encoding.UTF8.GetBytes(jstr);
            return send;
        }
        public static object ReCon(byte[] rec)
        {
            string jstr = Encoding.UTF8.GetString(rec);
            object receive = JsonConvert.DeserializeObject(jstr);
            return receive;
        }
    }
}
