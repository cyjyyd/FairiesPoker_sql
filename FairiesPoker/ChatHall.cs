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
public delegate void FangJianWeituo1(string name, int level, int status, int exp, bool visible, int cst);
public delegate void IconWeituo(Image img, PictureBox pic, bool stat);
public delegate void StateWeituo(Label label,string state);

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
        private FangJianWeituo1 RoomUI;
        private IconWeituo IconUI;
        private StateWeituo stateUI;
        Sqlconn db = new Sqlconn();
        Socket SocketClient;
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
            }
            byte[] send = mp.SendCon(logout);
            SocketClient.Send(send);
        }

        private void button11_Click(object sender, EventArgs e)
        {

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
                button5.Text = "邀请";
                RoomID = roomidgen();
                while (roomidcheck(RoomID))
                {
                    RoomID = roomidgen();
                }
                string sqlstr = string.Format("insert into RoomT values({0},'{1}',null,null,{2},null,null,1,0,0,null,null,null,null)", RoomID, u.UserName, UID);//TO DO
                db.getsqlcom(sqlstr);

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
            toolStripStatusLabel1.Text = "正在刷新在线玩家以及房间数据......";
            toolStripProgressBar1.Value = 70;
            string sqlstr = "select UserName From UserT where State=1";
            string sqlstr2 = "select RoomCName From RoomT";
            SqlDataReader UR = db.getcom(sqlstr);
            SqlDataReader rr = db.getcom(sqlstr2);
            if (UR.HasRows)
            {
                while (UR.Read())
                {
                    if (onlineListBox1.listBox.Items.Contains(UR[0].ToString()))
                    {

                    }
                    else
                    {
                        onlineListBox1.listBox.Items.Add(UR[0]);
                    }
                }
            }
            toolStripStatusLabel1.Text = "在线玩家数据刷新完成，正在刷新房间数据......";
            toolStripProgressBar1.Value = 80;
            Thread.Sleep(100);
            if (rr.HasRows)
            {
                while (rr.Read())
                {
                    if (roomListbox1.listBox.Items.Contains(rr[0].ToString()))
                    {

                    }
                    else
                    {
                        roomListbox1.listBox.Items.Add(rr[0]);
                    }

                }
            }
            toolStripStatusLabel1.Text = "已经就绪！";
            toolStripProgressBar1.Value = 100;
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
            th1 = new Thread(TClient);
            th1.IsBackground = true;
            th1.Start(SocketClient);
            toolStripStatusLabel1.Text = "已经建立与服务器的连接！正在传输数据.......";
            toolStripProgressBar1.Value = 60;timer2.Start();
        }
        void TClient(object o)
        {
            Socket SocketReceive = o as Socket;
            while (true)
            {
                byte[] jbyte = new byte[1024 * 1024 * 5];
                int r = SocketReceive.Receive(jbyte);
                Dictionary<string, string> data = mp.ReCon(jbyte, 0, r);
                if (data["Type"]=="Message1")
                {
                    ShowChatMsg(data["Time"] + ":" + data["From"] + ":" + data["Message"]);
                }
                else if (data["Type"]=="Message2")
                {
                    ShowChatMsg(data["Time"] + ":" + data["From"] + "→" + data["TO"] + ":" + data["Message"]);
                }
                else if (data["Type"]=="Message3")
                {
                    ShowChatMsg(data["Time"] + ":" + "系统消息:" + data["Message"]);
                }
                else if (data["Type"]=="JoinRoom")
                {
                    List<string> member = new List<string>();
                    string sqlstr = string.Format("select RoomCName,RoomPName,RoomP2Name from RoomT where ID = {0}", data["RoomID"]);
                    SqlDataReader dataReader = db.getcom(sqlstr);
                    dataReader.Read();
                    for (int i = 0; i < 3; i++)
                    {
                        if (dataReader[i] != System.DBNull.Value)
                        {
                            member.Add(dataReader[i].ToString().Trim());
                        }
                    }
                    for (int i = 0; i < member.Count; i++)
                    {
                        string sqlstr2 = string.Format("select UserName,Exp,CurrentLevel,State from UserT where UserName='{0}'", member[i]);
                        SqlDataReader dataReader1 = db.getcom(sqlstr2);
                        dataReader1.Read();
                        switch (i)
                        {
                            case 0:
                                labelp1.Invoke(RoomUI, dataReader1[0].ToString().Trim(), Convert.ToInt32(dataReader1[2]), Convert.ToInt32(dataReader1[3]), Convert.ToInt32(dataReader1[1]), true, 1);
                                if (File.Exists(Application.StartupPath + "\\ico\\" + dataReader1[0].ToString().Trim() + ".jpg"))
                                {
                                    pictureBoxp1.Invoke(IconUI, Image.FromFile(Application.StartupPath + "\\ico\\" + dataReader1[0].ToString().Trim() + ".jpg"), pictureBoxp1, true);
                                }
                                else icondown(dataReader1[0].ToString().Trim());
                                break;
                            case 1:
                                labelp2.Invoke(RoomUI, dataReader1[0].ToString().Trim(), Convert.ToInt32(dataReader1[2]), Convert.ToInt32(dataReader1[3]), Convert.ToInt32(dataReader1[1]), true, 2);
                                if (File.Exists(Application.StartupPath + "\\ico\\" + dataReader1[0].ToString().Trim() + ".jpg"))
                                {
                                    pictureBoxp1.Invoke(IconUI, Image.FromFile(Application.StartupPath + "\\ico\\" + dataReader1[0].ToString().Trim() + ".jpg"), pictureBoxp2, true);
                                }
                                else icondown(dataReader1[0].ToString().Trim());
                                break;
                            case 2:
                                labelp3.Invoke(RoomUI, dataReader1[0].ToString().Trim(), Convert.ToInt32(dataReader1[2]), Convert.ToInt32(dataReader1[3]), Convert.ToInt32(dataReader1[1]), true, 3);
                                if (File.Exists(Application.StartupPath + "\\ico\\" + dataReader1[0].ToString().Trim() + ".jpg"))
                                {
                                    pictureBoxp1.Invoke(IconUI, Image.FromFile(Application.StartupPath + "\\ico\\" + dataReader1[0].ToString().Trim() + ".jpg"), pictureBoxp3, true);
                                }
                                else icondown(dataReader1[0].ToString().Trim());
                                break;
                            default:
                                break;
                        }
                    }
                }
                else if (data["Type"] == "QuitRoom")
                {
                    if (data["UserName"]==labelnm1.Text)
                    {
                        labelp1.Invoke(RoomUI, "", 0, "空闲", 0, false, 1);                        
                        pictureBoxp1.Invoke(IconUI, Properties.Resources.backIMG, pictureBoxp1, false);
                    }
                    else if (data["UserName"]==labelnm2.Text)
                    {
                        labelp2.Invoke(RoomUI, "", 0, "空闲", 0, false, 2);
                        pictureBoxp2.Invoke(IconUI, Properties.Resources.backIMG, pictureBoxp2, false);
                    }
                    else if (data["UserName"]==labelnm3.Text)
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
        }
        void icondown(string uname)
        {
            string icopath = Application.StartupPath + "\\ico\\" + uname + ".jpg";
            byte[] imagebytes = null;
            string sqlstr = $"select Picture from UserT where UserName = '{uname}'";
            SqlDataReader dr = db.getcom(sqlstr);
            while (dr.Read())
            {
                imagebytes = (byte[])dr.GetValue(1);
            }
            dr.Close();
            MemoryStream ms = new MemoryStream(imagebytes);
            Bitmap bmpt = new Bitmap(ms);
            pictureBox1.BackgroundImage = bmpt;
            FileStream fs = new FileStream(icopath, FileMode.Create, FileAccess.ReadWrite);
            fs.Write(imagebytes, 0, imagebytes.Length); fs.Close();
        }
        void onlinecheck()
        {

        }
        int roomidgen()
        {
            Random r = new Random();
            return r.Next(10000, 99999);
        }
        bool roomidcheck(int rid)
        {
            string sqlstr = string.Format("select * from RoomT where RoomID={0}", rid.ToString());
            SqlDataReader dataReader = db.getcom(sqlstr);
            dataReader.Read();
            if (dataReader.HasRows)
            {
                return true;
            }
            else return false;
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
    }
}
