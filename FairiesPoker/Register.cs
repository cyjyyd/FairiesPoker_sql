using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Protocol.Code;
using Protocol.Dto;

namespace FairiesPoker
{
    public partial class Register : Form
    {
        SocketMsg Msg;
        string nm;
        string pwd;
        int avatarid = 0;
        bool standard = false;
        NetManager netManager;
        config con = new config();
        const string welcomecode = "westerndigital";
        SqlConnection connection;
        Dictionary<string, string> registervalues = new Dictionary<string, string>();
        public Register(NetManager netManager)
        {
            InitializeComponent();
            this.netManager = netManager;
        }
        OpenFileDialog ofd = new OpenFileDialog();

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (checkBox1.Checked)
                {
                    standard = true;
                    comboBox1.Enabled = false;
                    pictureBox1.BackgroundImage = Properties.Resources.Pla;
                }
                else
                {
                    comboBox1.Enabled = true;
                    standard = false;
                    if (comboBox1.Text != "")
                    {
                        pictureBox1.BackgroundImage = Image.FromFile(Application.StartupPath + "\\avatars\\" + comboBox1.Text + ".jpg");
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("尝试载入图像时发生错误！","error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text==welcomecode)
            {
                button1.Enabled = false;
                button2.Enabled = true;
                textBox1.Enabled = false;
                textBox2.Enabled = true;
            }
            else
            {
                MessageBox.Show("不正确的邀请码！","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length >= 4 && textBox2.Text.Length <= 16)
            {
                button2.Enabled = false;
                button3.Enabled = true;
                textBox2.Enabled = false;
                textBox3.Enabled = true;
                nm = textBox2.Text;
            }
            else
            {
                MessageBox.Show("用户名不规范！");
            }
        }

        private void Register_Shown(object sender, EventArgs e)
        {
            try
            {
                List<String> list = new List<string>();
                DirectoryInfo theFolder = new DirectoryInfo(Application.StartupPath + "\\avatars");
                FileInfo[] thefileInfo = theFolder.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                foreach (FileInfo NextFile in thefileInfo)  //遍历文件
                list.Add(NextFile.Name.Substring(0,NextFile.Name.IndexOf('.')));
                comboBox1.DataSource = list;
                comboBox1.SelectedIndex = 0;
            }
            catch (Exception)
            {
                MessageBox.Show("尝试载入图像时发生错误！", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox3.Text.Length>=8&&textBox3.Text.Length<=16)
            {
                pwd = null;
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] username = Encoding.Default.GetBytes(textBox3.Text);
                byte[] encryptname = md5.ComputeHash(username);
                for (int i = 0; i < encryptname.Length; i++)
                {
                    // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符
                    pwd = pwd + encryptname[i].ToString("X");
                }
                button6.Enabled = true;
                button3.Enabled = false;
                checkBox1.Enabled = true;
                textBox3.Enabled = false;
            }
            else
            {
                MessageBox.Show("密码长度不规范！");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            comboBox1.Enabled = false;
            checkBox1.Enabled = false;
            if (standard)
            {
                avatarid = -1;
            }
            else
            {
                if (comboBox1.SelectedIndex==-1)
                {
                    avatarid = -1;
                }
                avatarid = comboBox1.SelectedIndex;
            }
            button4.Enabled = true;
            button6.Enabled = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                AccountDto dto = new AccountDto(nm, pwd, avatarid);
                Msg = new SocketMsg(OpCode.ACCOUNT, AccountCode.REGIST_CREQ, dto);
                netManager.Execute(0, Msg);
                //MessageBox.Show("注册成功！\r\n" + "用户名：" + nm + "\r\n密码MD5:"+pwd+"\r\n头像ID："+ avatarid +"\r\n请牢记您的密码！", "Congratulations!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                reset();
            }
            catch (Exception)
            {
                MessageBox.Show("未知错误，请联系开发者","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }
        private void Register_Load(object sender, EventArgs e)
        {
            timer1.Start();
        }
        private void reset()
        {
            nm = "";
            pwd = "";
            avatarid = -1;
            textBox1.Text = textBox2.Text = textBox3.Text = "";
            textBox1.Enabled = true;
            button1.Enabled = true;
            button4.Enabled = false;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox1.BackgroundImage = Image.FromFile(Application.StartupPath + "\\avatars\\" + comboBox1.Text + ".jpg");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            netManager.Update();
        }
    }
}
