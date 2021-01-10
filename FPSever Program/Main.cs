using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
namespace FPSever_Program
{
    public partial class Main : Form
    {

        #region 所有变量
        [DllImport("kernel32")]//返回0表示失败，非0为成功
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]//返回取得字符串缓冲区的长度
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        Dictionary<int, string> IDName = new Dictionary<int, string>();
        Dictionary<string, int> NameID = new Dictionary<string, int>();
        Dictionary<string, DateTime> NameOnline = new Dictionary<string, DateTime>();
        Dictionary<string, Socket> dicSocket = new Dictionary<string, Socket>(); //地址对应昵称的集合，存放连接的客户端地址和昵称
        Dictionary<string, string> dicClientAndName = new Dictionary<string, string>();//昵称对应地址的集合，存放连接的客户端地址和昵称
        Dictionary<string, string> dicNameAndClient = new Dictionary<string, string>();
        MsgProcess mp = new MsgProcess();
        const int max = 60;
        int ran = 0;
        Thread th1;
        Thread th2;
        Sqlconn db = null;
        bool first = true;
        List<float> list = null;
        List<float> list1 = null;
        List<float> list2 = null;
        List<float> list3 = null;
        PerformanceCounter counter = null;
        PerformanceCounter counter1 = null;
        PerformanceCounter counter2 = null;
        PerformanceCounter counter3 = null;
        System.Windows.Forms.Timer timer = null;
        Random r = new Random();
        List<int> rlist = new List<int>();
        UdpClient Server = null;
        IPEndPoint endPoint = null;
        Socket socketData = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        string iniFilePath = Application.StartupPath + "\\userdata.ini";
        string logFilePath = Application.StartupPath + "\\RunTime.log";
        #endregion
        #region 性能监视器
        public Main()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            float cur = counter.NextValue();
            float cur1 = counter1.NextValue();
            float cur2 = counter2.NextValue();
            float cur3 = counter3.NextValue();
            list.Add(cur);
            list1.Add(cur1);
            list2.Add(cur2);
            list3.Add(cur3);
            if (++ran > 10)
            {
                ran = 0;
            }

            while (list.Count > max)
            {
                list.RemoveAt(0);
                list1.RemoveAt(0);
                list2.RemoveAt(0);
                list3.RemoveAt(0);
            }

            draw(pictureBox1,list);
            draw(pictureBox2, list1);
            draw(pictureBox3, list2);
            draw(pictureBox4, list3);
        }
        private void draw(PictureBox pic,List <float> list)
        {
            if (pic.Width < 1 || pic.Height < 1)
            {
                return;
            }

            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float single = 1.0F * bmp.Width / (max - 1);
            float division = 1.0F * bmp.Height / 100;
            float offset = 1.0F * (max - list.Count) * single;

            Color forge = Color.FromArgb(217, 234, 244);
            Color line = Color.FromArgb(17, 125, 187);
            Color back = Color.FromArgb(241, 246, 250);

            for (int i = 1; i < 10; i++)
            {
                int y = (int)(i * 10 * division);
                g.DrawLine(new Pen(forge), 1, y, bmp.Width - 2, y);
            }

            for (int i = 1; i <= 6; i++)
            {
                int x = (int)((i * 10 - ran) * single);
                g.DrawLine(new Pen(forge), x, 1, x, bmp.Height - 2);
            }

            List<PointF> ps = new List<PointF>();
            ps.Add(new PointF(0, bmp.Height));
            ps.Add(new PointF(offset + 0 * single, bmp.Height - list[0] * division));

            for (int i = 1; i < list.Count; i++)
            {
                ps.Add(new PointF(offset + i * single, bmp.Height - list[i] * division));
                g.DrawLine(new Pen(line), ps[ps.Count - 2], ps[ps.Count - 1]);
            }

            ps.Add(new PointF(bmp.Width, bmp.Height));

            g.FillClosedCurve(new SolidBrush(Color.FromArgb(0x80, forge)), ps.ToArray());
            g.DrawRectangle(new Pen(line), new Rectangle(0, 0, bmp.Width - 1, bmp.Height - 1));
            g.Dispose();

            Image gc = pic.Image;
            pic.Image = bmp;
            if (gc != null)
            {
                gc.Dispose();
            }
        }
        #endregion
        #region 消息显示
        private void showlogmsg(string str)
        {
            richTextBox1.AppendText(str+"\r\n");
        }
        private void showlogmsg2(string str)
        {
            richTextBox2.AppendText(str + "\r\n");
        }
        private void ShowGroupMsg(string v)
        {
            richTextBox3.AppendText(v + "\r\n");
        }
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
        private void Main_Load(object sender, EventArgs e)
        {
            list = new List<float>();
            list1 = new List<float>();
            list2 = new List<float>();
            list3 = new List<float>();
            list.AddRange(new float[] { 0, 0 });
            list1.AddRange(new float[] { 0, 0 });
            list2.AddRange(new float[] { 0, 0 });
            list3.AddRange(new float[] { 0, 0 });
            counter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            counter1 = new PerformanceCounter("Memory", "% Committed Bytes In Use", "");
            counter2 = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
            counter3 = new PerformanceCounter("TCPv4", "Segments/sec", "");
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += timer_Tick;
            timer.Start();
            new Thread(() => {
                GC.Collect();
                Thread.Sleep(10000);
            })
            { IsBackground = true }.Start();
            Server = new UdpClient(12588);
            endPoint = new IPEndPoint(IPAddress.Any, 12588);
            db = new Sqlconn();
            th1 = new Thread(UDPServer);
            th1.IsBackground = true;
            th2 = new Thread(TransMission);
            th2.IsBackground = true;
        }
        private void TXTPort_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            MessageBox.Show("格式不正确！", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void maskedTextBox3_TextChanged(object sender, EventArgs e)
        {
            MessageBox.Show("不建议修改端口号！", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (BtnStart.Text == "Launch!")
                {
                    BtnStart.Text = "Stop!";
                    if (first)
                    {
                        first = false;
                        if (MessageBox.Show("服务器确认运行后端口号和IP将不能更改，您确认继续？", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            IPEndPoint data = new IPEndPoint(IPAddress.Any, Convert.ToInt32(TXTPort.Text));
                            socketData.Bind(data);
                            socketData.Listen(64);
                            string sqlstr = "select ID,UserName from UserT";
                            SqlDataReader reader = db.getcom(sqlstr);
                            while(reader.Read())
                            {
                                IDName.Add(Convert.ToInt32(reader[0]), reader[1].ToString().Trim());
                                NameID.Add(reader[1].ToString().Trim(),Convert.ToInt32(reader[0]));
                            }
                            th1.Start();
                            th2.Start(socketData);
                            showlogmsg(DateTime.Now+":服务器已经启动，正在全频段监听");
                            showlogmsg2(DateTime.Now+":服务器已经启动，正在全频段监听");
                        }
                        else
                        {
                            first = true;
                            BtnStart.Text = "Launch!";
                        } 
                    }
                    else
                    {
                        th1.Resume();
                        showlogmsg(DateTime.Now+":服务器状态已经恢复");
                        showlogmsg2(DateTime.Now + ":服务器状态已经恢复");
                    }
                }
                else
                {
                    BtnStart.Text = "Launch!";
                    th1.Suspend();
                    showlogmsg(DateTime.Now + "服务器已经暂停服务");
                    showlogmsg2(DateTime.Now + "服务器已经暂停服务");
                }
            }
            catch (Exception ex)
            {
                showlogmsg(ex.Message);
                showlogmsg2(ex.Message);
                throw;
            }

        }
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (string item in dicClientAndName.Values)
            {
                string str = string.Format("Update UserT set State=0 where UserName='{0}'", item);
                db.getsqlcom(str);
            }
            Dictionary<string, string> quit = new Dictionary<string, string>();
            quit.Add("Type", "Quit");
            byte[] send = mp.SendCon(quit);
            MsgProcess.SendAllClient(comboBoxClient, dicSocket, send);
            Server.Client.Shutdown(SocketShutdown.Both);
            System.Environment.Exit(0);
        }
        OpenFileDialog ofd = new OpenFileDialog();
        private void btnSelect_Click(object sender, EventArgs e)
        {
            ofd.Title = "请选择您要发送的文件";
            ofd.Filter = "所有文件|*.*";
            ofd.ShowDialog();
            textPath.Text = ofd.FileName;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
           /* foreach (string item in NameOnline.Keys)
            {
                TimeSpan ts = DateTime.Now - NameOnline[item];//issue
                if (ts.TotalSeconds > 30)
                {
                    string sqlstr = string.Format("update UserT set State=0 where UserName='{0}'", item);
                    db.getsqlcom(sqlstr);
                    comboBoxClientName.Items.Remove(item);
                }
                else if(ts.TotalSeconds<30)
                {
                    string sqlstr = string.Format("update UserT set State=1 where UserName='{0}'", item);
                    db.getsqlcom(sqlstr);
                    if (comboBoxClientName.Items.Contains(item))
                    {

                    }
                    else
                    {
                        comboBoxClientName.Items.Add(item);
                    }
                }
            }*/
        }
        private void btnSendFile_Click(object sender, EventArgs e)
        {

        }
        private void btnFormDD_Click(object sender, EventArgs e)
        {

        }
        private void btnSendText_Click(object sender, EventArgs e)
        {

        }
        private void RichTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
        #endregion
        #region 后台线程
        void UDPServer()
        {
            while (true)
            {
                byte[] jbyte = Server.Receive(ref endPoint);
                string strinfo = Encoding.UTF8.GetString(jbyte, 0, jbyte.Length);
                Dictionary<string,string> data = JsonConvert.DeserializeObject<Dictionary<string,string>>(strinfo);
                if (data["Type"] == "Register")
                {
                    int id = int.Parse(data["ID"]);
                    string Uname = data["UserName"];
                    string Pwd = data["PwdMD5"];
                    string Regtime =DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    int coin = 0;
                    int exp = 0;
                    int level = Judgelevel(exp);
                    string sqlstr = String.Format("insert into UserT values ({0},'{1}','{2}','{3}',{4},{5},{6},null,0);",id,Uname,Pwd,Regtime,exp,coin,level);
                    db.getsqlcom(sqlstr);
                }
                else if (data["Type"] == "Login")
                {
                    string sqlstr = string.Format("select Password from UserT where UserName='{0}'", data["UserName"]);
                    SqlDataReader read = db.getcom(sqlstr);
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    Dictionary<string, string> result = new Dictionary<string, string>();
                    if (read.HasRows)
                    {
                        read.Read();
                        if (read.GetString(0).Trim().Equals(data["PwdMD5"]))
                        {
                            result.Add("Result", "Correct");
                            string send = JsonConvert.SerializeObject(result);
                            byte[] newsend = Encoding.UTF8.GetBytes(send);
                            socket.SendTo(newsend, endPoint);
                            NameOnline.Add(data["UserName"], DateTime.Now);
                            string sqlstr2 = string.Format("update UserT set State=1 where UserName='{0}'",data["UserName"]);
                            showlogmsg(DateTime.Now.ToString()+":"+data["UserName"]+":已登录");

                        }
                        else
                        {
                            result.Add("Result", "IncorrectPwd");
                            string send = JsonConvert.SerializeObject(result);
                            byte[] newsend = Encoding.UTF8.GetBytes(send);
                            socket.SendTo(newsend, endPoint);
                            showlogmsg(DateTime.Now.ToString() + ":" + data["UserName"] + ":登录失败：密码不正确");
                        }
                    }
                    else
                    {
                        result.Add("Result", "NosuchUserName");
                        string send = JsonConvert.SerializeObject(result);
                        byte[] newsend = Encoding.UTF8.GetBytes(send);
                        socket.SendTo(newsend, endPoint);
                        showlogmsg(DateTime.Now.ToString() + ":" + data["UserName"] + ":登录失败：未找到该用户");
                    }
                } 
            }
        }
        void TransMission(object o)
        {
            try
            {
                Socket socketListen;
                Socket SocketData = o as Socket;
                int AccClientNum = 0;
                List<string> ClientAddress = new List<string>();
                while (true)
                {
                    //每次循环都新建一个socket,让下一个用户接入
                    socketListen = SocketData.Accept();
                    //把当前接入的客户端地址和socket放入集合内，客户端地址作为键
                    dicSocket.Add(socketListen.RemoteEndPoint.ToString(), socketListen);
                    //存储当前客户端的地址到下拉框
                    comboBoxClient.Items.Add(socketListen.RemoteEndPoint.ToString());
                    string clientAdd = socketListen.RemoteEndPoint.ToString();
                    showlogmsg(DateTime.Now + ":" + "客户端:" + clientAdd + ":尝试连接！");
                    ClientNum(socketListen, ClientAddress, ref AccClientNum);
                    Thread th3 = new Thread(TCPServer);
                    th3.IsBackground = true;
                    th3.Start(socketListen);
                }
            }
            catch
            {
            }
        }
        void ClientNum(Socket socketWatch, List<string> ClientAddress, ref int ClientNum)
        {
            //如果当前的客户端地址不存在集合内，在线数量+1
            int n = socketWatch.RemoteEndPoint.ToString().IndexOf(":");
            //存储当前客户端的IP地址
            string clientIp = socketWatch.RemoteEndPoint.ToString().Remove(n);
            if (!ClientAddress.Contains(clientIp))
            {
                ClientNum++;
                ClientAddress.Add(clientIp);
            }
            lalClientNum.Text = "客户端在线数量：" + ClientNum.ToString();
        }
        void TCPServer(object o)
        {
            Socket SocketClient = o as Socket;
            while (true)
            {
                byte[] jbyte = new byte[1024];
                int r = SocketClient.Receive(jbyte);
                Dictionary<string, string> data = mp.ReCon(jbyte, 0, r);
                if (data["Type"] == "Userdata")
                {
                    string sqlstr = string.Format("select ID,UserName,GameCount,Exp,Coin,CurrentLevel from UserT where UserName='{0}'", data["UserName"]);
                    SqlDataReader dataReader = db.getcom(sqlstr);
                    string sql2 = string.Format("update UserT set State=1 where UserName='{0}'", data["UserName"]);
                    db.getsqlcom(sql2);
                    Dictionary<string, string> result = new Dictionary<string, string>();
                    if (dataReader.HasRows)
                    {
                        dataReader.Read();
                        result.Add("ID", dataReader[0].ToString().Trim());
                        result.Add("UserName", dataReader[1].ToString().Trim());
                        result.Add("GameCount", dataReader[2].ToString().Trim());
                        result.Add("Exp", dataReader[3].ToString().Trim());
                        result.Add("Coin", dataReader[4].ToString().Trim());
                        result.Add("CurrentLevel", dataReader[5].ToString().Trim());
                        byte[] newsend = mp.SendCon(result);
                        SocketClient.Send(newsend);
                        string sqlstr3 = string.Format("update UserT set Lastlogin='{0}' where UserName = '{1}'", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), data["UserName"]);
                        db.getsqlcom(sqlstr3);
                    }
                    else
                    {
                        result.Add("Error", "404");
                        string send = JsonConvert.SerializeObject(result);
                        byte[] newsend = Encoding.UTF8.GetBytes(send);
                        SocketClient.Send(newsend);
                    }
                    dicNameAndClient.Add(data["UserName"], SocketClient.RemoteEndPoint.ToString());
                    dicClientAndName.Add(SocketClient.RemoteEndPoint.ToString(), data["UserName"]);
                }
                else if (data["Type"] == "Message")
                {
                    if (data.ContainsKey("ToUser"))
                    {
                        string sqlstr = string.Format("insert into ChatT values({0},{1},{2},'{3}','{4}','{5}')", NameID[data["UserName"]], NameID[data["TOUser"]], data["RoomID"], DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), data["Chat"], data["Bgroup"]);
                        db.getsqlcom(sqlstr);
                        Dictionary<string, string> send = new Dictionary<string, string>();
                        send.Add("Type", "Message2");
                        send.Add("From", data["UserName"]);
                        send.Add("TO", data["TOUser"]);
                        send.Add("Message", data["Chat"]);
                        send.Add("Time", DateTime.Now.ToString());
                        byte[] newsend = mp.SendCon(send);
                        dicSocket[dicNameAndClient[data["TOUser"]]].Send(newsend);
                        SocketClient.Send(newsend);
                        ShowGroupMsg(DateTime.Now.ToString() + ":转发消息:" + data["UserName"] + "→" + data["TOUser:"] + data["Chat"]);
                    }
                    else
                    {
                        string sqlstr = string.Format("insert into ChatT values({0},{1},{2},'{3}','{4}','{5}')", NameID[data["UserName"]], "null", data["RoomID"], DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), data["Chat"], data["Bgroup"]);
                        db.getsqlcom(sqlstr);
                        Dictionary<string, string> send = new Dictionary<string, string>();
                        send.Add("Type", "Message1");
                        send.Add("From", data["UserName"]);
                        send.Add("Message", data["Chat"]);
                        send.Add("Time", DateTime.Now.ToString());
                        byte[] newsend = mp.SendCon(send);
                        MsgProcess.SendAllClient(comboBoxClient, dicSocket,newsend);
                        ShowGroupMsg(DateTime.Now.ToString() + ":" + data["UserName"] + ":" + data["Chat"]);
                    }
                }
                else if (data["Type"] == "JoinRoom")
                {
                    string sqlstr = string.Format("update RoomT set {0}='{1}',{2}={3},{4}={5} where RoomID = {6}", data["Position"], data["UserName"], data["PositionID"], NameID[data["UserName"]], data["PositionState"], 1, data["RoomID"]);
                    db.getsqlcom(sqlstr);
                    Dictionary<string, string> send = new Dictionary<string, string>();
                    send.Add("Type", "JoinRoom");
                    send.Add("State", "Success");
                    send.Add("RoomID", data["RoomID"]);
                    send.Add("UserName", data["UserName"]);
                    byte[] newsend = mp.SendCon(send);
                    string sqlstr2 = string.Format("select RoomCName,RoomPName,RoomP2Name from RoomT where RoomID = {0}", data["RoomID"]);
                    SqlDataReader dataReader = db.getcom(sqlstr2);
                    dataReader.Read();
                    if (dataReader.HasRows)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (dataReader[i]!=System.DBNull.Value)
                            {
                                dicSocket[dicNameAndClient[dataReader[i].ToString().Trim()]].Send(newsend);
                            }
                        }
                    }
                }
                else if (data["Type"] == "QuitRoom")
                {
                    string sqlstr = string.Format("update RoomT set {0}= null,{1}= null,{2}= 0 where RoomID = {3}", data["Position"], data["PositionID"], data["PositionState"], data["RoomID"]);
                    db.getsqlcom(sqlstr);
                    string sqlstr2 = "exec RoomCheck";
                    db.getsqlcom(sqlstr2);
                    Dictionary<string, string> send = new Dictionary<string, string>();
                    send.Add("Type", "QuitRoom");
                    send.Add("State", "Success");
                    send.Add("RoomID", data["RoomID"]);
                    send.Add("UserName", data["UserName"]);
                    byte[] newsend = mp.SendCon(send);
                    string sqlstr3 = string.Format("select RoomCName,RoomPName,RoomP2Name from RoomT where RoomID = {0}", data["RoomID"]);
                    SqlDataReader dataReader = db.getcom(sqlstr2);
                    dataReader.Read();
                    if (dataReader.HasRows)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (dataReader[i] != System.DBNull.Value)
                            {
                                dicSocket[dicNameAndClient[dataReader[i].ToString().Trim()]].Send(newsend);
                            }
                        }
                    }
                }
                else if (data["Type"] == "ChangeState")
                {
                    string sqlstr = string.Format("update RoomT set {0}={1} where RoomID = {2}", data["Position"], data["State"], data["RoomID"]);
                    db.getsqlcom(sqlstr);
                    Dictionary<string, string> send = new Dictionary<string, string>();
                    send.Add("Type", "CState");
                    send.Add("State", data["State"]);
                    byte[] newsend = mp.SendCon(send);
                    string sqlstr2 = string.Format("select RoomCName,RoomPName,RoomP2Name from RoomT where RoomID = {0}", data["RoomID"]);
                    SqlDataReader dataReader = db.getcom(sqlstr2);
                    dataReader.Read();
                    if (dataReader.HasRows)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (dataReader[i] != System.DBNull.Value)
                            {
                                dicSocket[dicNameAndClient[dataReader[i].ToString().Trim()]].Send(newsend);
                            }
                        }
                    }
                }
                else if (data["Type"] == "NewGame")
                {
                    string sqlstr = string.Format("insert into GameT values()");
                    string sqlstr2 = string.Format("insert into GameT values()");
                    string sqlstr3 = string.Format("insert into GameT values()");
                    string sqlstr4 = string.Format("update RoomT set RoomCState=17,RoomPState=17,RoomP2State=17 where RoomID = {0}", data["RoomID"]);
                    string sqlstr5 = string.Format("select RoomCName,RoomPName,RoomP2Name from RoomT where RoomID = {0}", data["RoomID"]);
                    db.getsqlcom(sqlstr);
                    db.getsqlcom(sqlstr2);
                    db.getsqlcom(sqlstr3);
                    db.getsqlcom(sqlstr4);
                    SqlDataReader reader = db.getcom(sqlstr5);
                    if (reader.HasRows)
                    {
                        Dictionary<string, IPEndPoint> newgame = new Dictionary<string, IPEndPoint>();
                        Thread th1 = new Thread(GameProcess);
                        th1.IsBackground = true;
                        th1.Start(newgame);
                    }
                    else
                    {
                        Dictionary<string, string> send = new Dictionary<string, string>();
                        send.Add("Type", "GState");
                        send.Add("State", "Failed");
                        byte[] newsend = mp.SendCon(send);
                        SocketClient.Send(newsend);
                    }
                }
                else if (data["Type"] == "Logout")
                {
                    IDName.Remove(NameID[data["UserName"]]);
                    NameID.Remove(data["UserName"]);
                    if (data["RoomPosition"] != "Null")
                    {
                        string sqlstr = string.Format("update RoomT set {0}={1},{2}={3} where RoomID={4}", data["RoomPosition"], "null", data["PositionID"], "null", data["RoomID"]);
                        string sqlstr2 = string.Format("exec RoomCheck");
                        string sqlstrx = string.Format("delete from RoomT where RoomCName is null and RoomPName is null and RoomP2Name is null");
                        db.getsqlcom(sqlstr);
                        db.getsqlcom(sqlstr2);
                        db.getsqlcom(sqlstrx);
                        Dictionary<string, string> send = new Dictionary<string, string>();
                        send.Add("Type", "QuitRoom");
                        send.Add("State", "Success");
                        send.Add("RoomID", data["RoomID"]);
                        send.Add("UserName", data["UserName"]);
                        byte[] newsend = mp.SendCon(send);
                        string sqlstr5 = string.Format("select RoomCName,RoomPName,RoomP2Name from RoomT where RoomID = {0}", data["RoomID"]);
                        SqlDataReader dataReader = db.getcom(sqlstr5);
                        dataReader.Read();
                        if (dataReader.HasRows)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                if (dataReader[i] != System.DBNull.Value)
                                {
                                    dicSocket[dicNameAndClient[dataReader[i].ToString().Trim()]].Send(newsend);
                                }
                            }
                        }
                    }
                    string sqlstr3 = string.Format("update UserT set State=0 where UserName='{0}'", data["UserName"]);
                    db.getsqlcom(sqlstr3);

                }
                else if (data["Type"] == "OnlineCheck")
                {
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    Dictionary<string, string> send = new Dictionary<string, string>();
                    send.Add("Type", "OnlineCheck");
                    send.Add("State", "1");
                    byte[] newsend = mp.SendCon(send);
                    socket.SendTo(newsend, endPoint);
                    if (NameOnline.ContainsKey(data["UserName"]))
                    {
                        NameOnline[data["UserName"]] = DateTime.Now;
                    }
                }
            }          
        }

        public static int Judgelevel(int exp)
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

        void GameProcess(object o)
        {
            Dictionary<string, IPEndPoint> Newgame = o as Dictionary<string, IPEndPoint>;//TO DO:新UDP监听端口完成消息处理
        }
        #endregion
    }
}
