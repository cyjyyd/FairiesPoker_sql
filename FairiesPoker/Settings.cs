using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace FairiesPoker
{
    public partial class Settings : Form
    {
        Point mouseOff;//鼠标移动位置变量
        bool leftFlag;//标记是否为左键
        config con = new config();
        bool clicked = false;
        bool changed = false;
        public Settings()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            Opacity = 0;timer1.Start();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            refresh();
        }
        private void refresh()
        {
            readset(label5,pictureBox1,con.BackMusic);
            readset(label6,pictureBox2,con.SoundFX);
            readset(label7, pictureBox3, con.FilmMode);
            readset(label10, pictureBox4, con.FullScreen);
            readscrset();readpackage();
            comboBox1.SelectedIndex = con.UI - 1;
            changed = false;
        }
        private void readset (Label lbl,PictureBox pic,bool bl)
        {
            if (bl)
            {
                lbl.Text = "开";
                pic.Image = Properties.Resources.choose1;
                pic.Tag = true;
            }
            else
            {
                lbl.Text = "关";
                pic.Image = Properties.Resources.choose2;
                pic.Tag = false;
            }
        }
        private void readpackage ()
        {
            for (int i = 1;i <= 7;i++)
            {
                string temp = Enum.GetName(typeof(Path),i);
                if (Filecheck(i)!=1)
                {
                    if (Dircheck(i,temp)==2&&Filecheck(i)==2 && Convert.ToString(comboBox1.Items[i - 1]).Contains("不完整") == false)
                    {
                        comboBox1.Items[i - 1] = comboBox1.Items[i - 1] + "(不完整)";
                    }
                    else if (Dircheck(i,temp)==0&&Filecheck(i)==0 && Convert.ToString(comboBox1.Items[i - 1]).Contains("未安装") == false)
                    {
                        comboBox1.Items[i - 1] = comboBox1.Items[i - 1] + "(未安装)";
                    }
                    else if (Convert.ToString(comboBox1.Items[i - 1]).Contains("不完整") == false)
                    {
                        comboBox1.Items[i - 1] = comboBox1.Items[i - 1] + "(不完整)";
                    }
                }
            }
        }
        private int Dircheck (int i,string path)
        {
            if (Directory.Exists(Application.StartupPath + @"\Pokers\" + i) && Directory.Exists(Application.StartupPath + @"\Results\" + i) && Directory.Exists(Application.StartupPath + "\\" + path))
            {
                return 1;
            }
            else if (Directory.Exists(Application.StartupPath + @"\Pokers\" + i) || Directory.Exists(Application.StartupPath + @"\Results\" + i) || Directory.Exists(Application.StartupPath + "\\" + path))
            {
                return 2;
            }
            else return 0;
        }
        private int Filecheck (int i)
        {
            return con.Filecheck(i);
        }
        private bool valuejudge (PictureBox pic)
        {
            if (Convert.ToBoolean(pic.Tag)) return true;
            else return false;
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (Convert.ToBoolean(((PictureBox)sender).Tag) == true)
            {
                ((PictureBox)sender).Tag = false;
                ((PictureBox)sender).Image = Properties.Resources.choose2;
            }
            else
            {
                ((PictureBox)sender).Tag = true;
                ((PictureBox)sender).Image = Properties.Resources.choose1;
            }
            labelset();
            UI_Click(sender,e);
        }
        private void screenset ()
        {
            switch (comboBox2.SelectedIndex)
            {
                case 0:con.Width = 1280;con.Height = 720;
                    break;
                case 1:con.Width = 1366;con.Height = 768;
                    break;
                case 2:con.Width = 1600;con.Height = 900;
                    break;
                case 3:con.Width = 1920;con.Height = 1080;
                    break;
                default:con.Width = 1280;con.Height = 720;
                    break;
            }
        }
        private void readscrset ()
        {
            switch (con.Width)
            {
                case 1280:comboBox2.SelectedIndex = 0;
                    break;
                case 1366:comboBox2.SelectedIndex = 1;
                    break;
                case 1600:comboBox2.SelectedIndex = 2;
                    break;
                case 1920:comboBox2.SelectedIndex = 3;
                    break;
                default:comboBox2.SelectedIndex = 0;
                    break;
            }
        }
        private void labelset ()
        {
            if (Convert.ToBoolean(pictureBox1.Tag)) label5.Text = "开";
            else label5.Text = "关";
            if (Convert.ToBoolean(pictureBox2.Tag)) label6.Text = "开";
            else label6.Text = "关";
            if (Convert.ToBoolean(pictureBox3.Tag)) label7.Text = "开";
            else label7.Text = "关";
            if (Convert.ToBoolean(pictureBox4.Tag)) label10.Text = "开";
            else label10.Text = "关";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            clicked = true;
            con.UI = comboBox1.SelectedIndex + 1;
            con.BackMusic = valuejudge(pictureBox1);
            con.SoundFX = valuejudge(pictureBox2);
            con.FilmMode = valuejudge(pictureBox3);
            con.FullScreen = valuejudge(pictureBox4);
            screenset();
            con.writeset();
            if (scrdec()&&uidec())
            {
                refresh();
            }
            else
            {
                comboBox2.SelectedIndex = 0;
                screenset();
                con.writeset();
                refresh();
            }       
        }
        public static bool scrdec()
        {
            config con = new config();
            Screen screen = Screen.PrimaryScreen;
            int screenWidth = screen.Bounds.Width;
            int screenHeight = screen.Bounds.Height;
            if (con.Width>screenWidth)
            {
                MessageBox.Show("错误，设置分辨率超过屏幕物理分辨率！将重置为默认分辨率！","Oops",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return false;
            }
            else
            {
                return true;
            }
        }
        private bool uidec ()
        {
            if (Convert.ToString(comboBox1.SelectedItem).Contains("不完整")||Convert.ToString(comboBox1.SelectedItem).Contains("未安装"))
            {
                MessageBox.Show("错误，所选择主题未安装或丢失文件，请安装后重试！", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            else return true;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (clicked == false && changed)
            {
                if (MessageBox.Show("是否保存设置?", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    button3_Click(sender, e);
                    CloseWindow();
                }
                else CloseWindow();
            }
            else CloseWindow();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CloseWindow();
        }
        private void UI_Click (object sender,EventArgs e)
        {
            changed = true;
        }

        private void Settings_Shown(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Opacity += 0.05;
            if (Opacity==100)
            {
                timer1.Stop();
            }
        }


        private void CloseWindow ()
        {
            for (int i = 0; i < 20; i++)
            {
                Opacity -= 0.05;
                Thread.Sleep(50);
            }
            this.Close();
        }

        private void Settings_MouseDown(object sender, MouseEventArgs e)
        {
            this.Focus();
            if (e.Button == MouseButtons.Left)
            {
                mouseOff = new Point(-e.X, -e.Y); //得到变量的值
                leftFlag = true;                  //点击左键按下时标注为true;
            }
        }

        private void Settings_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftFlag)
            {
                Point mouseSet = Control.MousePosition;
                mouseSet.Offset(mouseOff.X, mouseOff.Y);  //设置移动后的位置
                Location = mouseSet;
            }
        }

        private void Settings_MouseUp(object sender, MouseEventArgs e)
        {
            if (leftFlag)
            {
                leftFlag = false;//释放鼠标后标注为false;
            }
        }
    }
}
