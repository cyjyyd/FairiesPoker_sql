using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace FPSever_Program
{
    class MsgProcess
    {
        public byte[] SendCon(Dictionary<string,string> dic)
        {
            string jstr = JsonConvert.SerializeObject(dic);
            byte[] send = Encoding.UTF8.GetBytes(jstr);
            return send;
        }
        public Dictionary<string, string> ReCon(byte[] rec, int index, int count)
        {
            string jstr = Encoding.UTF8.GetString(rec, index, count);
            Dictionary<string, string> receive = JsonConvert.DeserializeObject<Dictionary<string, string>>(jstr);
            return receive;
        }
        /// <summary>
        /// 广播消息给全部用户
        /// </summary>
        /// <param name="comboBoxClient">客户端下拉框</param>
        /// <param name="dicSocket">客户端地址对应Socket的集合</param>
        /// <param name="buffer">要发送的字节数组</param>
        public static void SendAllClient(ComboBox comboBoxClient, Dictionary<string, Socket> dicSocket, byte[] buffer)
        {
            for (int i = 0; i < comboBoxClient.Items.Count; i++)
            {
                dicSocket[comboBoxClient.Items[i].ToString()].Send(buffer);
            }
        }

    }
}
