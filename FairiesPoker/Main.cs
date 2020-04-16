using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace FairiesPoker
{
    public partial class Main : Form
    {
        Point mouseOff;//鼠标移动位置变量
        bool leftFlag;//标记是否为左键
        #region 窗体设置
        public Main()
        {
            /// <summary>
            /// 设置控件绘制方式（双缓冲UI绘制）：控件先在缓冲区中自行绘制，并忽略WM_ERASEKGND消息引起的重绘，从而减少闪烁
            /// </summary>
            InitializeComponent();
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            config con = new config(); UI u = new UI();u.setUI(con.UI);
            this.BackgroundImage = u.Background;Opacity = 0;timer1.Start(); 
        }
        #endregion
        #region 所有事件
        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.FPR;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.FP;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            AboutBox1 ab = new AboutBox1();
            ab.ShowDialog();
        }

        private void quit_Click(object sender, EventArgs e)
        {
            CloseWindow();
            Application.Exit();
        }

        private void SinPla_Click(object sender, EventArgs e)
        {
            DdzMian dz = new DdzMian(false);
            CloseWindow();
            dz.Show(); this.Close();
        }

        private void MulPla_Click(object sender, EventArgs e)
        {
            Login log = new Login();
            CloseWindow();
            log.Show();this.Close();
        }

        private void set_Click(object sender, EventArgs e)
        {
            Settings setting = new Settings();
            setting.ShowDialog();
        }

        private void SinPla_MouseEnter(object sender, EventArgs e)
        {
            SinPla.ForeColor = SystemColors.ActiveCaption;
        }

        private void MulPla_MouseEnter(object sender, EventArgs e)
        {
            MulPla.ForeColor = SystemColors.ActiveCaption;
        }

        private void set_MouseEnter(object sender, EventArgs e)
        {
            set.ForeColor = SystemColors.ActiveCaption;
        }

        private void quit_MouseEnter(object sender, EventArgs e)
        {
            quit.ForeColor = SystemColors.ActiveCaption;
        }

        private void quit_MouseLeave(object sender, EventArgs e)
        {
            quit.ForeColor = Color.OrangeRed;
        }

        private void set_MouseLeave(object sender, EventArgs e)
        {
            set.ForeColor = Color.OrangeRed;
        }

        private void MulPla_MouseLeave(object sender, EventArgs e)
        {
            MulPla.ForeColor = Color.OrangeRed;
        }

        private void SinPla_MouseLeave(object sender, EventArgs e)
        {
            SinPla.ForeColor = Color.OrangeRed;
        }

        private void Main_MouseDown(object sender, MouseEventArgs e)
        {
            this.Focus();
            if (e.Button == MouseButtons.Left)
            {
                mouseOff = new Point(-e.X, -e.Y); //得到变量的值
                leftFlag = true;                  //点击左键按下时标注为true;
            }
        }

        private void Main_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftFlag)
            {
                Point mouseSet = Control.MousePosition;
                mouseSet.Offset(mouseOff.X, mouseOff.Y);  //设置移动后的位置
                Location = mouseSet;
            }
        }

        private void Main_MouseUp(object sender, MouseEventArgs e)
        {
            if (leftFlag)
            {
                leftFlag = false;//释放鼠标后标注为false;
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            this.Focus();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Opacity += 0.05;
            if (Opacity==100)
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
        #endregion
    }
}
