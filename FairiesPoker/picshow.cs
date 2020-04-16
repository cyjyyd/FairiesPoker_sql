using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FairiesPoker
{
    public partial class picshow : Form
    {
        config con = new config();
        UI u = new UI();
        public picshow()
        {
            InitializeComponent();
            u.setUI(con.UI);
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
        }

        private void picshow_Load(object sender, EventArgs e)
        {
            button1.BackgroundImage = u.Button;
            try
            {
                pictureBox1.Image = Image.FromFile(Application.StartupPath + @"\Pokers\"+u.uiselect+"\\fangkuai3.png");
            }
            catch (Exception)
            {
                MessageBox.Show("错误！文件不存在","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                this.Close();
            }   
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex!=-1)
            {
                try
                {
                    switch (comboBox1.SelectedIndex)
                    {
                        case 0:
                            pictureBox1.Image = Image.FromFile(Application.StartupPath + @"\Pokers\" + u.uiselect + "\\fangkuai" + (comboBox2.SelectedIndex + 3) + ".png");
                            break;
                        case 1:
                            pictureBox1.Image = Image.FromFile(Application.StartupPath + @"\Pokers\" + u.uiselect + "\\meihua" + (comboBox2.SelectedIndex + 3) + ".png");
                            break;
                        case 2:
                            pictureBox1.Image = Image.FromFile(Application.StartupPath + @"\Pokers\" + u.uiselect + "\\hongtao" + (comboBox2.SelectedIndex + 3) + ".png");
                            break;
                        case 3:
                            pictureBox1.Image = Image.FromFile(Application.StartupPath + @"\Pokers\" + u.uiselect + "\\heitao" + (comboBox2.SelectedIndex + 3) + ".png");
                            break;
                        case 4:
                            pictureBox1.Image = Image.FromFile(Application.StartupPath + @"\Pokers\" + u.uiselect + "\\16.png");
                            break;
                        case 5:
                            pictureBox1.Image = Image.FromFile(Application.StartupPath + @"\Pokers\" + u.uiselect + "\\17.png");
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("错误！文件不存在", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1_SelectedIndexChanged(sender,e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_MouseDown(object sender, MouseEventArgs e)
        {
            button1.BackgroundImage = u.Buttonpress;
        }

        private void button1_MouseUp(object sender, MouseEventArgs e)
        {
            button1.BackgroundImage = u.Button;
        }
    }
}
