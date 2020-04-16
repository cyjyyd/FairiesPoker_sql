using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FairiesPoker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Opacity = 0;
            timer2.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }
        public void checkdlls ()
        {

        }
        public void checkfiles ()
        {

        }
        public void checknet ()
        {

        }
        public void settings()
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (pictureBox1.Width==95)
            {
                checkdlls();
            }
            else if (pictureBox1.Width==195)
            {
                checkfiles();
            }
            else if (pictureBox1.Width==285)
            {
                checknet();
            }
            else if (pictureBox1.Width==375)
            {
                settings();
            }
            else if (pictureBox1.Width==475)
            {
                timer1.Enabled = false;
                Main main = new Main();
                CloseWindow();
                main.Show();
            }
            pictureBox1.Width = pictureBox1.Width + 1;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            Opacity += 0.05;
            if (Opacity==100)
            {
                timer2.Enabled = false;
            }
        }
        private void CloseWindow ()
        {
            for (int i = 0; i < 20; i++)
            {
                Opacity -= 0.05;
                Thread.Sleep(50);
            }
            this.Hide();
        }
    }
}
