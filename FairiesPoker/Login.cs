using System;
using System.Security;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Net.Sockets;

namespace FairiesPoker
{
    public partial class Login : Form
    {
        [DllImport("kernel32")]//返回0表示失败，非0为成功
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]//返回取得字符串缓冲区的长度
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        config con = new config();
        string iniFilePath = Application.StartupPath + "\\config.ini";
        Socket socket;
        IPAddress ip;
        Thread th1;
        public Login()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            Opacity = 0; timer1.Start();
            th1 = new Thread(loginset);
            th1.IsBackground = true;
        }
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
        private void label1_Click(object sender, EventArgs e)
        {
            label1.Visible = false;
            textBox1.Focus();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            label2.Visible = false;
            textBox2.Focus();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Main m = new Main();
            CloseWindow();
            m.Show();
            this.Close();
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

        private void Login_Load(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (check())
                {
                    Dictionary<string, string> Log = new Dictionary<string, string>();
                    string nm = textBox1.Text;
                    string pwd=null;
                    MD5 md5 = new MD5CryptoServiceProvider();
                    byte[] password = Encoding.Default.GetBytes(textBox2.Text);
                    byte[] encryptpassword = md5.ComputeHash(password);
                    for (int i = 0;i<encryptpassword.Length;i++)
                    {
                        pwd = pwd + encryptpassword[i].ToString("X");
                    }
                    Log.Add("Type", "Login");
                    Log.Add("UserName", nm);
                    Log.Add("PwdMD5", pwd);
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    ip = IPAddress.Parse(con.IPAddress);
                    string send = JsonConvert.SerializeObject(Log);
                    byte[] newsend = Encoding.UTF8.GetBytes(send);
                    IPEndPoint endPoint = new IPEndPoint(ip, con.Port);
                    socket.SendTo(newsend, endPoint);
                    Thread.Sleep(500);
                    byte[] jbyte = new byte[256];
                    int r = socket.Receive(jbyte);
                    string res = Encoding.UTF8.GetString(jbyte, 0, r);
                    Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(res);
                    if (data["Result"].Equals("IncorrectPwd"))
                    {
                        MessageBox.Show("登录失败!密码错误！");
                    }
                    else if (data["Result"].Equals("Correct"))
                    {
                        ChatHall ch = new ChatHall(nm);
                        CloseWindow();
                        ch.Show();this.Close();
                    }
                    else if (data["Result"].Equals("NosuchUserName"))
                    {
                        MessageBox.Show("用户名不存在！");
                    }
                }
                else
                {
                    MessageBox.Show("请输入用户名或密码！", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error404:找不到服务器","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }
        void loginset()
        {

        }
        private bool check()
        {
            if (textBox1.Text == "")
            {
                return false;
            }
            else if (textBox2.Text == "")
            {
                return false;
            }
            else return true;
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            label1.Visible = false;
            textBox1.Focus();
        }

        private void textBox2_Click(object sender, EventArgs e)
        {
            label2.Visible = false;
            textBox2.Focus();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            label1.Visible = false;
        }

        private void Login_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            label2.Visible = false;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Register reg = new Register();
            reg.ShowDialog();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("该功能还在抢修中，预计下个版本修复");
        }
    }
}
