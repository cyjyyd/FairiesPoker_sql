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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.SinPla = new System.Windows.Forms.Label();
            this.MulPla = new System.Windows.Forms.Label();
            this.set = new System.Windows.Forms.Label();
            this.quit = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // SinPla
            // 
            this.SinPla.AutoSize = true;
            this.SinPla.BackColor = System.Drawing.Color.Transparent;
            this.SinPla.Font = new System.Drawing.Font("黑体", 26.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.SinPla.ForeColor = System.Drawing.Color.OrangeRed;
            this.SinPla.Location = new System.Drawing.Point(271, 332);
            this.SinPla.Name = "SinPla";
            this.SinPla.Size = new System.Drawing.Size(262, 70);
            this.SinPla.TabIndex = 2;
            this.SinPla.Text = "单人模式\r\nSingle Player";
            this.SinPla.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.SinPla.Click += new System.EventHandler(this.SinPla_Click);
            this.SinPla.MouseEnter += new System.EventHandler(this.SinPla_MouseEnter);
            this.SinPla.MouseLeave += new System.EventHandler(this.SinPla_MouseLeave);
            // 
            // MulPla
            // 
            this.MulPla.AutoSize = true;
            this.MulPla.BackColor = System.Drawing.Color.Transparent;
            this.MulPla.Font = new System.Drawing.Font("黑体", 26.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.MulPla.ForeColor = System.Drawing.Color.OrangeRed;
            this.MulPla.Location = new System.Drawing.Point(745, 332);
            this.MulPla.Name = "MulPla";
            this.MulPla.Size = new System.Drawing.Size(262, 70);
            this.MulPla.TabIndex = 3;
            this.MulPla.Text = "多人模式\r\nMulti  Player";
            this.MulPla.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.MulPla.Click += new System.EventHandler(this.MulPla_Click);
            this.MulPla.MouseEnter += new System.EventHandler(this.MulPla_MouseEnter);
            this.MulPla.MouseLeave += new System.EventHandler(this.MulPla_MouseLeave);
            // 
            // set
            // 
            this.set.AutoSize = true;
            this.set.BackColor = System.Drawing.Color.Transparent;
            this.set.Font = new System.Drawing.Font("黑体", 26.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.set.ForeColor = System.Drawing.Color.OrangeRed;
            this.set.Location = new System.Drawing.Point(366, 464);
            this.set.Name = "set";
            this.set.Size = new System.Drawing.Size(167, 70);
            this.set.TabIndex = 4;
            this.set.Text = "设置\r\nSettings";
            this.set.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.set.Click += new System.EventHandler(this.set_Click);
            this.set.MouseEnter += new System.EventHandler(this.set_MouseEnter);
            this.set.MouseLeave += new System.EventHandler(this.set_MouseLeave);
            // 
            // quit
            // 
            this.quit.AutoSize = true;
            this.quit.BackColor = System.Drawing.Color.Transparent;
            this.quit.Font = new System.Drawing.Font("黑体", 26.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.quit.ForeColor = System.Drawing.Color.OrangeRed;
            this.quit.Location = new System.Drawing.Point(745, 464);
            this.quit.Name = "quit";
            this.quit.Size = new System.Drawing.Size(148, 70);
            this.quit.TabIndex = 5;
            this.quit.Text = "退出\r\nQ u i t";
            this.quit.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.quit.Click += new System.EventHandler(this.quit_Click);
            this.quit.MouseEnter += new System.EventHandler(this.quit_MouseEnter);
            this.quit.MouseLeave += new System.EventHandler(this.quit_MouseLeave);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = global::FairiesPoker.Properties.Resources.FP;
            this.pictureBox1.Location = new System.Drawing.Point(268, 136);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(726, 156);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 6;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            this.pictureBox1.MouseEnter += new System.EventHandler(this.pictureBox1_MouseEnter);
            this.pictureBox1.MouseLeave += new System.EventHandler(this.pictureBox1_MouseLeave);
            // 
            // timer1
            // 
            this.timer1.Interval = 50;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.quit);
            this.Controls.Add(this.set);
            this.Controls.Add(this.MulPla);
            this.Controls.Add(this.SinPla);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Main";
            this.Load += new System.EventHandler(this.Main_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Main_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Main_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Main_MouseUp);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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