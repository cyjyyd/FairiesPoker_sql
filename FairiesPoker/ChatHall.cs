using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Data.SqlClient;
using Nancy.Json;
using FairiesPoker;

public delegate void FangJianWeituo1(string name, int level, int status, int exp, bool visible, int cst);
public delegate void IconWeituo(Image img, PictureBox pic, bool stat);
public delegate void StateWeituo(Label label,string state);
public delegate void StrWeituo(string str);

namespace FairiesPoker
{
    public partial class ChatHall : Form
    {
        #region 所有变量
        [DllImport("kernel32")]//返回0表示失败，非0为成功
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]//返回取得字符串缓冲区的长度
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        Dictionary<string, string> userdata;
        Dictionary<string, string> chathalldata;
        Dictionary<int, string> RoomJsonData;
        Dictionary<string, string> playerdata;
        string iniFilePath = Application.StartupPath + "\\config.ini";
        string UserName;
        string roomposition;
        User u = new User();
        UI ui = new UI();
        MsgProcess mp = new MsgProcess();
        config con = new config();
        string filePath = "";
        int RoomID;
        int UID;
        bool inroom;
        bool isconnected;
        bool isfirstconfig=true;
        private FangJianWeituo1 RoomUI;
        private IconWeituo IconUI;
        private StateWeituo stateUI;
        private StrWeituo Strapp;
        Socket SocketClient;
        Socket SocketSend;
        IPAddress ip;
        IPEndPoint ep;
        Thread th1;
        Thread th2;
        #endregion
        #region 读Ini文件
        public string ReadIniData(string Section, string Key, string NoText, string iniFilePath)
        {
            if (File.Exists(iniFilePath))
            {
                StringBuilder temp = new StringBuilder(1024);
                GetPrivateProfileString(Section, Key, NoText, temp, 1024, iniFilePath);
                return temp.ToString();
            }
            else
            {
                return String.Empty;
            }
        }

        #endregion
        #region 写Ini文件
        public bool WriteIniData(string Section, string Key, string Value, string iniFilePath)
        {
            if (File.Exists(iniFilePath))
            {
                long OpStation = WritePrivateProfileString(Section, Key, Value, iniFilePath);
                if (OpStation == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion
        #region 窗体事件
        public ChatHall(string UName)
        {
            InitializeComponent();
            UserName = UName;
            u.UserName = UName;
            RoomUI = new FangJianWeituo1(RoomUISet);
            IconUI = new IconWeituo(Iconset);
            stateUI = new StateWeituo(Statset);
            Strapp = new StrWeituo(ShowChatMsg);
            comboBox1.SelectedIndex = 0;
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        private void ChatHall_Load(object sender, EventArgs e)
        {
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            ui.setUI(con.UI);
            int i = 1;
            for (i = 1; i <= 12; i++)
            {
                Control[] a = this.Controls.Find("button" + i.ToString(), true);
                ((Button)(a[0])).BackgroundImage = ui.Button;
                ((Button)(a[0])).MouseDown += WindowMouseDown;
                ((Button)(a[0])).MouseUp += WindowMouseUp;
            }
            Opacity = 0; timer1.Start();
            th2 = new Thread(onlinecheck);
            th2.IsBackground = true;
            Initialization();
        }
        private void ChatHall_FormClosing(object sender, FormClosingEventArgs e)
        {
            Dictionary<string, string> logout = new Dictionary<string, string>();
            logout.Add("Type", "Logout");
            if (inroom)
            {
                logout.Add("RoomPosition", roomposition);
                logout.Add("UserName", u.UserName);//sql查询所在列名
            }
            else
            {
                logout.Add("RoomPosition", "Null");
                logout.Add("UserName", u.UserName);
            }
            byte[] send = mp.SendCon(logout);
            SocketClient.Send(send);
            SocketClient.Disconnect(false);
            SocketClient.Dispose();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                Dictionary<string, string> chat = new Dictionary<string, string>();
                if (comboBox1.SelectedItem.ToString() == "所有人")
                {
                    chat.Add("Type", "Message");
                    chat.Add("UserName", u.UserName);
                    chat.Add("RoomID", "1");
                    chat.Add("Chat", textBox1.Text);
                    chat.Add("Bgroup", "EveryOne");
                    byte[] send = mp.SendCon(chat);
                    SocketClient.Send(send);
                }
                else if (comboBox1.SelectedItem.ToString() == "房间频道")
                {
                    chat.Add("Type", "Message");
                    chat.Add("UserName", u.UserName);
                    chat.Add("RoomID", Convert.ToString(RoomID));
                    chat.Add("Chat", textBox1.Text);
                    chat.Add("Bgroup", "Room");
                    byte[] send = mp.SendCon(chat);
                    SocketClient.Send(send);
                }
                else
                {
                    chat.Add("Type", "Message");
                    chat.Add("UserName", u.UserName);
                    chat.Add("TOUser", comboBox1.SelectedItem.ToString());
                    chat.Add("RoomID", "0");
                    chat.Add("Chat", textBox1.Text);
                    chat.Add("Bgroup", "Private");
                    byte[] send = mp.SendCon(chat);
                    SocketClient.Send(send);
                }
                textBox1.Clear();
            }
            catch
            {
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (button8.Text == "准备")
            {
                button8.Text = "取消准备";

            }
            else if (button8.Text == "取消准备")
            {
                button8.Text = "准备";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (button5.Text == "开房间")
            {
                Dictionary<string, string> temp = new Dictionary<string, string>();
                temp.Add("Type","NewRoom");
                temp.Add("UserName", u.UserName);

                button5.Text = "邀请";
            }
            else if (button5.Text == "邀请")
            {
                //TODO:直发消息

            }
        }

        private void button12_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            CloseWindow();
            Application.Exit();
        }
        public void ShowChatMsg(string str)
        {
            chatTextBox1.richTextBox.AppendText(str + "\r\n");
        }
        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("更多玩法开发中，敬请期待！", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void WindowMouseDown(object sender, MouseEventArgs e)
        {
            ((Button)sender).BackgroundImage = ui.Buttonpress;
        }

        private void WindowMouseUp(object sender, MouseEventArgs e)
        {
            ((Button)sender).BackgroundImage = ui.Button;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Opacity += 0.05;
            if (Opacity == 100)
            {
                timer1.Stop();
            }
        }
        private void CloseWindow()
        {
            for (int i = 0; i < 20; i++)
            {
                Opacity -= 0.05;
                Thread.Sleep(50);
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {

        }
        #endregion
        #region 后台线程及委托
        void Initialization()
        {
            SocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse(con.IPAddress);
            IPEndPoint endPoint = new IPEndPoint(ip, con.Port);
            SocketClient.Connect(endPoint);
            label1.Text = u.UserName;
            isconnected = true;
            string icopath = Application.StartupPath + "\\ico\\" + u.UserName + ".jpg";
            toolStripStatusLabel1.Text = "正在获取头像......";
            toolStripProgressBar1.Value = 20;
            if (File.Exists(icopath))
            {
                pictureBox1.BackgroundImage = Image.FromFile(icopath);
            }
            else
            {
                icondown(u.UserName);
            }
            toolStripStatusLabel1.Text = "正在获取用户信息......";
            toolStripProgressBar1.Value = 40;
            playerdata = new Dictionary<string, string>();
            Dictionary<string, string> Data = new Dictionary<string, string>();
            Data.Add("Type", "Userdata");
            Data.Add("UserName", u.UserName);
            byte[] newsend = mp.SendCon(Data);
            SocketClient.Send(newsend);
            byte[] rbyte = new byte[1024];
            int r = SocketClient.Receive(rbyte);
            userdata = mp.ReCon(rbyte, 0, r);
            label1.Text = userdata["UserName"];
            label4.Text = userdata["Exp"] + "/10000";
            label2.Text = userdata["Coin"];
            label3.Text = "Lv:" + userdata["CurrentLevel"];
            UID = Convert.ToInt32(userdata["ID"]);
            toolStripStatusLabel1.Text = "已经建立与服务器的连接！正在传输数据.......";
            toolStripProgressBar1.Value = 60;
            Dictionary<string, string> request = new Dictionary<string, string>();
            request.Add("Type", "Request");
            request.Add("UserName", u.UserName);
            byte[] nsendr = mp.SendCon(request);
            SocketClient.Send(nsendr);
            byte[] recbyte = new byte[2048];
            int rr = SocketClient.Receive(recbyte);
            chathalldata = mp.ReCon(recbyte, 0, rr);
            toolStripProgressBar1.Value = 80;
            toolStripStatusLabel1.Text = "正在解析数据";
            configcht(chathalldata);
            toolStripProgressBar1.Value = 100;
            isfirstconfig = false;
            th1 = new Thread(TClient);
            th1.IsBackground = true;
            th1.Start(SocketClient);
        }
        void configcht(Dictionary<string,string> data)
        {
            toolStripStatusLabel1.Text = "数据最后更新于：" + data["LastUpdate"];
            onlineListBox1.listBox.Items.Clear();int count = 0;
            RoomJsonData = new Dictionary<int, string>();
            foreach (KeyValuePair<string,string> item in data)
            {
                if(isnumeric(item.Key))
                {
                    onlineListBox1.listBox.Items.Add(item.Key + ":" + item.Value);
                }
                if (item.Key.Equals("RoomData" + count.ToString()))
                {
                    RoomJsonData.Add(count, item.Value);
                    count++;
                }
            }
            configroom(RoomJsonData);
        }
        void configroom(Dictionary<int,string>roomdata)
        {
            foreach (KeyValuePair<int,string> item in roomdata)
            {
                Dictionary<string, string> room = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.Value);
                roomListbox1.listBox.Items.Add(room["RoomID"] + ":" + room["RoomCName"]+":"+statjudge(Convert.ToInt32(room["GameStatus"])));
            }
        }
        public string statjudge(int i)
        {
            switch (i)
            {
                case 0:return "满员";
                    break;
                case 1:return "游戏中";
                    break;
                case 2:return "可加入";
                    break;
                default:return "未知";
                    break;
            }
        }
        public bool isnumeric(string str)
        {
            char[] ch = new char[str.Length];
            ch = str.ToCharArray();
            for (int i = 0; i < str.Length; i++)
            {
                if (ch[i] < 48 || ch[i] > 57)
                {
                    return false;
                }
            }
            return true;
        }
        void TClient(object o)
        {
                Socket SocketReceive = o as Socket;
            while (true)
            {
                try
                {
                    byte[] jbyte = new byte[1024 * 1024 * 5];
                    int r = SocketReceive.Receive(jbyte);
                    Dictionary<string, string> data = mp.ReCon(jbyte, 0, r);
                    if (data["Type"] == "Message1")
                    {
                        label1.Invoke(Strapp, data["Time"] + ":" + data["From"] + ":" + data["Message"]);
                    }
                    else if (data["Type"] == "Message2")
                    {
                        ShowChatMsg(data["Time"] + ":" + data["From"] + "→" + data["TO"] + ":" + data["Message"]);
                    }
                    else if (data["Type"] == "Message3")
                    {
                        ShowChatMsg(data["Time"] + ":" + "系统消息:" + data["Message"]);
                    }
                    else if (data["Type"] == "JoinRoom")
                    {
                        if (data["State"]=="Failed")
                        {
                            toolStripStatusLabel1.Text = "进入房间失败！房间已经满员！";
                            inroom = false;
                        }
                        else if (data["State"]=="Success")
                        {
                            playerdata.Add("M1", data["RoomCID"]);
                            if (data.ContainsKey("RoomPID"))
                            {
                                playerdata.Add("P1",data["RoomPID"]);
                            }
                            if (data.ContainsKey("RoomP2ID"))
                            {
                                playerdata.Add("P2", data["RoomP2ID"]);
                            }
                        }
                        //to do:UI界面显示
                    }
                    else if (data["Type"] == "QuitRoom")
                    {
                        if (data["UserName"] == labelnm1.Text)
                        {
                            labelp1.Invoke(RoomUI, "", 0, "空闲", 0, false, 1);
                            pictureBoxp1.Invoke(IconUI, Properties.Resources.backIMG, pictureBoxp1, false);
                        }
                        else if (data["UserName"] == labelnm2.Text)
                        {
                            labelp2.Invoke(RoomUI, "", 0, "空闲", 0, false, 2);
                            pictureBoxp2.Invoke(IconUI, Properties.Resources.backIMG, pictureBoxp2, false);
                        }
                        else if (data["UserName"] == labelnm3.Text)
                        {
                            labelp3.Invoke(RoomUI, "", 0, "空闲", 0, false, 3);
                            pictureBoxp3.Invoke(IconUI, Properties.Resources.backIMG, pictureBoxp3, false);
                        }
                    }
                    else if (data["Type"] == "CState")//更改状态
                    {
                        if (data["UserName"] == labelnm1.Text)
                        {

                        }
                        else if (data["UserName"] == labelnm2.Text)
                        {

                        }
                        else if (data["UserName"] == labelnm3.Text)
                        {

                        }
                    }
                    else if (data["Type"] == "GState")
                    {
                        if (data["UserName"] == labelnm1.Text)
                        {

                        }
                        else if (data["UserName"] == labelnm2.Text)
                        {

                        }
                        else if (data["UserName"] == labelnm3.Text)
                        {

                        }
                    }
                    else if (data["Type"] == "OnlineCheck")
                    {

                    }
                }
                catch
                {

                }
            }
        }
        void icondown(string uname)
        {
            pictureBox1.BackgroundImage = Image.FromFile(Application.StartupPath + "\\ico\\default.jpg");
        }
        void onlinecheck()
        {

        }
        public void RoomUISet(string name, int level, int status, int exp, bool visible, int cst)
        {
            if (cst == 1)
            {
                labelp1.Text = stat(status); labelnm1.Text = name;
                labelnm1.Visible = visible; labellv1.Visible = visible; labelex1.Visible = visible;
                labelex1.Text = exp + "/10000"; labellv1.Text = "Lv:" + level;
            }
            else if (cst == 2)
            {
                labelp2.Text = stat(status); labelnm2.Text = name;
                labelnm2.Visible = visible; labellv2.Visible = visible; labelex2.Visible = visible;
                labelex2.Text = exp + "/10000"; labellv2.Text = "Lv:" + level;
            }
            else if (cst == 3)
            {
                labelp3.Text = stat(status); labelnm3.Text = name;
                labelnm3.Visible = visible; labellv3.Visible = visible; labelex3.Visible = visible;
                labelex3.Text = exp + "/10000"; labellv3.Text = "Lv:" + level;
            }
        }
        public static string stat(int i)
        {
            switch (i)
            {
                case 0:return "离线";
                case 1:return "在线";
                case 17:return "准备";
                default: return "未知";
            }
        }
        private void Iconset(Image img, PictureBox pic, bool stat)
        {
            pic.BackgroundImage = img;
            pic.Visible = stat;
        }
        private void Statset(Label label,string state)
        {
            label.Text = state;
        }
        #endregion

        private void button7_Click(object sender, EventArgs e)
        {
            if (roomListbox1.listBox.SelectedIndex==-1)
            {
                MessageBox.Show("请在左侧选择你要加入的房间！","Info",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
            else if (roomListbox1.listBox.SelectedItem.ToString().Contains("满员"))
            {
                MessageBox.Show("该房间已经满员！", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (roomListbox1.listBox.SelectedItem.ToString().Contains("游戏中"))
            {
                MessageBox.Show("该房间正在游戏中！", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {

                string[] selectedata = roomListbox1.listBox.SelectedItem.ToString().Split(':');
                Dictionary<string, string> theroom = JsonConvert.DeserializeObject<Dictionary<string, string>>(RoomJsonData[Convert.ToInt32(selectedata[0])]);//bug：选择正确的房间
                Dictionary<string, string> send = new Dictionary<string, string>();
                send.Add("Type", "JoinRoom");
                send.Add("RoomID",theroom["RoomID"]);
                send.Add("UserName", u.UserName);
                send.Add("ID", UID.ToString());
                SocketClient.Send(mp.SendCon(send));
                button8.Enabled = true;
                button10.Enabled = true;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            CloseWindow();
            Main m = new Main();
            m.Show();
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            button8.Enabled = false;
            button9.Enabled = false;
            button10.Enabled = false;
            u2.clear();
            u3.clear();
        }

        private void button9_Click(object sender, EventArgs e)
        {

        }
    }
}
