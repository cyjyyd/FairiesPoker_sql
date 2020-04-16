using System;
using System.Drawing;
using System.Windows.Forms;

namespace FairiesPoker
{
    public partial class win : Form
    {
        private bool leftFlag;//是否左键
        Point mouseOff;//记录鼠标位置
        UI u = new UI();config con = new config();
        public win(bool[] result,string[] name)
        {
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            u.setUI(con.UI);
            InitializeComponent();
            if (result[1]==true)
            {
                if (result[0] == false && result[2] == false)
                {
                    this.BackgroundImage = Image.FromFile(Application.StartupPath + @"\Results\" + u.uiselect + "\\win_dz.png");
                }
                else
                {
                    this.BackgroundImage = Image.FromFile(Application.StartupPath + @"\Results\" + u.uiselect + "\\win_nm.png");
                }
            }
            else if (result[1]==false)
            {
                if (result[0] == true&&result[2] == true)
                {
                    this.BackgroundImage = Image.FromFile(Application.StartupPath + @"\Results\" + u.uiselect + "\\lose_dz.png");
                }
                else
                {
                    this.BackgroundImage = Image.FromFile(Application.StartupPath + @"\Results\" + u.uiselect + "\\lose_nm.png");
                }
            }
            label2.Text = name[0];
            label3.Text = name[1];
            label4.Text = name[2];
            label5.Text = layout(result[0]);
            label6.Text = layout(result[1]);
            label7.Text = layout(result[2]);
        }
        #region 所有事件
        private void win_Load(object sender, EventArgs e)
        {
            button1.BackgroundImage = u.Button;
        }
        private string layout (bool bl)
        {
            if (bl)
            {
                return "胜利";
            }
            else
            {
                return "失败";
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void win_Shown(object sender, EventArgs e)
        {
            this.Focus();
        }

        private void button1_MouseDown(object sender, MouseEventArgs e)
        {
            button1.BackgroundImage = u.Buttonpress;
        }

        private void button1_MouseUp(object sender, MouseEventArgs e)
        {
            button1.BackgroundImage = u.Button;
        }

        private void win_MouseDown(object sender, MouseEventArgs e)
        {
            this.Focus();
            if (e.Button == MouseButtons.Left)
            {
                mouseOff = new Point(-e.X, -e.Y);
                leftFlag = true;
            }
        }

        private void win_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftFlag)
            {
                Point mouseSet = Control.MousePosition;
                mouseSet.Offset(mouseOff.X, mouseOff.Y);
                Location = mouseSet;
            }
        }

        private void win_MouseUp(object sender, MouseEventArgs e)
        {
            if (leftFlag)
            {
                leftFlag = false;
            }
        }
#endregion
    }
}
