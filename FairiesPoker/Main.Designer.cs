namespace FairiesPoker
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            SinPla = new System.Windows.Forms.Label();
            MulPla = new System.Windows.Forms.Label();
            set = new System.Windows.Forms.Label();
            quit = new System.Windows.Forms.Label();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            timer1 = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // SinPla
            // 
            SinPla.AutoSize = true;
            SinPla.BackColor = System.Drawing.Color.Transparent;
            SinPla.Font = new System.Drawing.Font("黑体", 26.25F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 134);
            SinPla.ForeColor = System.Drawing.Color.OrangeRed;
            SinPla.Location = new System.Drawing.Point(161, 335);
            SinPla.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            SinPla.Name = "SinPla";
            SinPla.Size = new System.Drawing.Size(387, 106);
            SinPla.TabIndex = 2;
            SinPla.Text = "单人模式\r\nSingle Player";
            SinPla.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            SinPla.Click += SinPla_Click;
            SinPla.MouseEnter += SinPla_MouseEnter;
            SinPla.MouseLeave += SinPla_MouseLeave;
            // 
            // MulPla
            // 
            MulPla.AutoSize = true;
            MulPla.BackColor = System.Drawing.Color.Transparent;
            MulPla.Font = new System.Drawing.Font("黑体", 26.25F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 134);
            MulPla.ForeColor = System.Drawing.Color.OrangeRed;
            MulPla.Location = new System.Drawing.Point(793, 335);
            MulPla.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            MulPla.Name = "MulPla";
            MulPla.Size = new System.Drawing.Size(387, 106);
            MulPla.TabIndex = 3;
            MulPla.Text = "多人模式\r\nMulti  Player";
            MulPla.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            MulPla.Click += MulPla_Click;
            MulPla.MouseEnter += MulPla_MouseEnter;
            MulPla.MouseLeave += MulPla_MouseLeave;
            // 
            // set
            // 
            set.AutoSize = true;
            set.BackColor = System.Drawing.Color.Transparent;
            set.Font = new System.Drawing.Font("黑体", 26.25F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 134);
            set.ForeColor = System.Drawing.Color.OrangeRed;
            set.Location = new System.Drawing.Point(288, 500);
            set.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            set.Name = "set";
            set.Size = new System.Drawing.Size(247, 106);
            set.TabIndex = 4;
            set.Text = "设置\r\nSettings";
            set.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            set.Click += set_Click;
            set.MouseEnter += set_MouseEnter;
            set.MouseLeave += set_MouseLeave;
            // 
            // quit
            // 
            quit.AutoSize = true;
            quit.BackColor = System.Drawing.Color.Transparent;
            quit.Font = new System.Drawing.Font("黑体", 26.25F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 134);
            quit.ForeColor = System.Drawing.Color.OrangeRed;
            quit.Location = new System.Drawing.Point(793, 500);
            quit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            quit.Name = "quit";
            quit.Size = new System.Drawing.Size(219, 106);
            quit.TabIndex = 5;
            quit.Text = "退出\r\nQ u i t";
            quit.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            quit.Click += quit_Click;
            quit.MouseEnter += quit_MouseEnter;
            quit.MouseLeave += quit_MouseLeave;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = System.Drawing.Color.Transparent;
            pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            pictureBox1.Image = Properties.Resources.FP;
            pictureBox1.Location = new System.Drawing.Point(157, 90);
            pictureBox1.Margin = new System.Windows.Forms.Padding(4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(968, 195);
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 6;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            pictureBox1.MouseEnter += pictureBox1_MouseEnter;
            pictureBox1.MouseLeave += pictureBox1_MouseLeave;
            // 
            // timer1
            // 
            timer1.Interval = 50;
            timer1.Tick += timer1_Tick;
            // 
            // Main
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            ClientSize = new System.Drawing.Size(1280, 720);
            Controls.Add(pictureBox1);
            Controls.Add(quit);
            Controls.Add(set);
            Controls.Add(MulPla);
            Controls.Add(SinPla);
            Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, 134);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "Main";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Main";
            Load += Main_Load;
            Shown += Main_Shown;
            MouseDown += Main_MouseDown;
            MouseMove += Main_MouseMove;
            MouseUp += Main_MouseUp;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label SinPla;
        private System.Windows.Forms.Label MulPla;
        private System.Windows.Forms.Label set;
        private System.Windows.Forms.Label quit;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Timer timer1;
    }
}