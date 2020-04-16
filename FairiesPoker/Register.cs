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
using Newtonsoft.Json;
namespace FairiesPoker
{
    public partial class Register : Form
    {
        Socket socket;
        IPAddress ip;
        IPEndPoint endPoint;
        string nm;
        string pwd;
        bool standard = false;
        config con = new config();
        const string welcomecode = "westerndigital";
        SqlConnection connection;
        Dictionary<string, string> registervalues = new Dictionary<string, string>();
        public Register()
        {
            InitializeComponent();
        }
        OpenFileDialog ofd = new OpenFileDialog();
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                ofd.Title = "请选择您要发送的文件";
                ofd.Filter = "JPEG图像文件|*.jpg";
                ofd.ShowDialog();
                textBox4.Text = ofd.FileName;
                pictureBox1.BackgroundImage = Image.FromFile(ofd.FileName);
            }
            catch (Exception)
            {
                MessageBox.Show("读取文件时发生错误！","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                standard = true;
                button5.Enabled = false;
                textBox4.Enabled = false;
                pictureBox1.BackgroundImage = Properties.Resources.Pla;
            }
            else
            {
                button5.Enabled = true;
                textBox4.Enabled = true;
                standard = false;
                if (textBox4.Text!="")
                {
                    pictureBox1.BackgroundImage = Image.FromFile(textBox4.Text);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text==welcomecode)
            {
                button2.Enabled = true;
                textBox1.Enabled = false;
            }
            else
            {
                MessageBox.Show("不正确的邀请码！","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            nm = textBox2.Text;
            SqlCommand com = new SqlCommand("select * from UserT where UserName=@Name");
            com.Parameters.Add("Name", SqlDbType.NVarChar);
            com.Parameters["Name"].Value = nm;
            SqlDataReader reader = com.ExecuteReader();
            if (reader.HasRows)
            {
                MessageBox.Show("该用户名已被占用，请换一个试试呢","Warning",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
            else
            {
                button2.Enabled = false;
                button3.Enabled = true;
            }
        }

        private void Register_Shown(object sender, EventArgs e)
        {
            try
            {
                connection = new SqlConnection("Server=AMD-R9-3900x;Database=FPServerDB;User id=sa;PWD=CYJjzr2014*");
                connection.Open();
            }
            catch (Exception)
            {
                MessageBox.Show("未能连接到数据库服务器！","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void button3_Click(object sender, EventArgs e)
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
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ip = IPAddress.Parse(con.IPAddress);
            endPoint = new IPEndPoint(ip, 8088);
            Dictionary<string, string> Reg = new Dictionary<string, string>();
            Reg.Add("Type","Register");
            Reg.Add("UserName", nm);
            Reg.Add("PwdMD5", pwd);
            string send = JsonConvert.SerializeObject(Reg);
            byte[] newsend = Encoding.UTF8.GetBytes(send);
            socket.SendTo(newsend, endPoint);
            textBox3.Enabled = false;
            button5.Enabled = true;
            button6.Enabled = true;
            textBox4.Enabled = true;
            checkBox1.Enabled = true;
            Thread.Sleep(300);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            button5.Enabled = false;
            textBox4.Enabled = false;
            if (standard)
            {
                SqlCommand com = new SqlCommand("update UserT set Picture = @ImageList where UserName = @Uname", connection);
                com.Parameters.Add("ImageList", SqlDbType.Image);
                com.Parameters["ImageList"].Value = pictureBox1.BackgroundImage;
                com.Parameters.Add("Uname", SqlDbType.NVarChar);
                com.Parameters["Uname"].Value = textBox2.Text;
                com.ExecuteNonQuery();
                connection.Close();
            }
            else
            {
                string fullpath = ofd.FileName;//文件路径
                FileStream fs = new FileStream(fullpath, FileMode.Open);
                byte[] imagebytes = new byte[fs.Length];
                BinaryReader br = new BinaryReader(fs);
                imagebytes = br.ReadBytes(Convert.ToInt32(fs.Length));
                SqlCommand com = new SqlCommand("update UserT set Picture = @ImageList where UserName = @Uname", connection);
                com.Parameters.Add("ImageList", SqlDbType.Image);
                com.Parameters["ImageList"].Value = imagebytes;
                com.Parameters.Add("Uname", SqlDbType.NVarChar);
                com.Parameters["Uname"].Value = textBox2.Text;
                com.ExecuteNonQuery();
                connection.Close();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show("注册成功！\r\n"+"用户名："+textBox2.Text+"\r\n请牢记您的密码！","Congratulations!",MessageBoxButtons.OK,MessageBoxIcon.Information);
                this.Close();
            }
            catch 
            {

            }
        }
        private void Register_Load(object sender, EventArgs e)
        {

        }
    }
}
