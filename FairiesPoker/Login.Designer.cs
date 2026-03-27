namespace FairiesPoker
{
    partial class Login
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
            button1 = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            textBox1 = new System.Windows.Forms.TextBox();
            textBox2 = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            timer1 = new System.Windows.Forms.Timer(components);
            linkLabel1 = new System.Windows.Forms.LinkLabel();
            linkLabel2 = new System.Windows.Forms.LinkLabel();
            lblConnectionStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.BackColor = System.Drawing.Color.Transparent;
            button1.BackgroundImage = Properties.Resources.btn1;
            button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            button1.Location = new System.Drawing.Point(165, 556);
            button1.Margin = new System.Windows.Forms.Padding(9, 9, 9, 9);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(246, 67);
            button1.TabIndex = 0;
            button1.Text = "登录";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.BackColor = System.Drawing.Color.Transparent;
            button2.BackgroundImage = Properties.Resources.btn1;
            button2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            button2.FlatAppearance.BorderSize = 0;
            button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            button2.Location = new System.Drawing.Point(460, 556);
            button2.Margin = new System.Windows.Forms.Padding(9, 9, 9, 9);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(246, 67);
            button2.TabIndex = 1;
            button2.Text = "取消";
            button2.UseVisualStyleBackColor = false;
            button2.Click += button2_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = System.Drawing.Color.Transparent;
            pictureBox1.BackgroundImage = Properties.Resources.捕获;
            pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            pictureBox1.Location = new System.Drawing.Point(359, 84);
            pictureBox1.Margin = new System.Windows.Forms.Padding(9, 9, 9, 9);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(141, 122);
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;
            // 
            // textBox1
            // 
            textBox1.Font = new System.Drawing.Font("微软雅黑", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            textBox1.Location = new System.Drawing.Point(165, 288);
            textBox1.Margin = new System.Windows.Forms.Padding(9, 9, 9, 9);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(541, 65);
            textBox1.TabIndex = 3;
            textBox1.Click += textBox1_Click;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // textBox2
            // 
            textBox2.Font = new System.Drawing.Font("微软雅黑", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            textBox2.Location = new System.Drawing.Point(165, 383);
            textBox2.Margin = new System.Windows.Forms.Padding(9, 9, 9, 9);
            textBox2.Name = "textBox2";
            textBox2.Size = new System.Drawing.Size(541, 65);
            textBox2.TabIndex = 4;
            textBox2.UseSystemPasswordChar = true;
            textBox2.Click += textBox2_Click;
            textBox2.TextChanged += textBox2_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = System.Drawing.Color.White;
            label1.Font = new System.Drawing.Font("微软雅黑", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
            label1.ForeColor = System.Drawing.Color.LightGray;
            label1.Location = new System.Drawing.Point(174, 294);
            label1.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(157, 58);
            label1.TabIndex = 5;
            label1.Text = "用户名";
            label1.Click += label1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = System.Drawing.Color.White;
            label2.Font = new System.Drawing.Font("微软雅黑", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
            label2.ForeColor = System.Drawing.Color.LightGray;
            label2.Location = new System.Drawing.Point(174, 388);
            label2.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(113, 58);
            label2.TabIndex = 6;
            label2.Text = "密码";
            label2.Click += label2_Click;
            // 
            // timer1
            // 
            timer1.Interval = 50;
            timer1.Tick += timer1_Tick;
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.BackColor = System.Drawing.Color.Transparent;
            linkLabel1.Location = new System.Drawing.Point(738, 318);
            linkLabel1.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new System.Drawing.Size(82, 24);
            linkLabel1.TabIndex = 8;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "注册账号";
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            //
            // linkLabel2
            //
            linkLabel2.AutoSize = true;
            linkLabel2.BackColor = System.Drawing.Color.Transparent;
            linkLabel2.Location = new System.Drawing.Point(738, 403);
            linkLabel2.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            linkLabel2.Name = "linkLabel2";
            linkLabel2.Size = new System.Drawing.Size(82, 24);
            linkLabel2.TabIndex = 9;
            linkLabel2.TabStop = true;
            linkLabel2.Text = "修改密码";
            linkLabel2.LinkClicked += linkLabel2_LinkClicked;
            //
            // lblConnectionStatus
            //
            lblConnectionStatus.AutoSize = true;
            lblConnectionStatus.BackColor = System.Drawing.Color.Transparent;
            lblConnectionStatus.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            lblConnectionStatus.ForeColor = System.Drawing.Color.Gray;
            lblConnectionStatus.Location = new System.Drawing.Point(20, 620);
            lblConnectionStatus.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            lblConnectionStatus.Name = "lblConnectionStatus";
            lblConnectionStatus.Size = new System.Drawing.Size(56, 31);
            lblConnectionStatus.TabIndex = 10;
            lblConnectionStatus.Text = "连接中...";
            //
            // Login
            // 
            AcceptButton = button1;
            AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackgroundImage = Properties.Resources.UILI2;
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            CancelButton = button2;
            ClientSize = new System.Drawing.Size(860, 656);
            Controls.Add(lblConnectionStatus);
            Controls.Add(linkLabel2);
            Controls.Add(linkLabel1);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Controls.Add(pictureBox1);
            Controls.Add(button2);
            Controls.Add(button1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(9, 9, 9, 9);
            Name = "Login";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Login";
            FormClosing += Login_FormClosing;
            Load += Login_Load;
            Shown += Login_Shown;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.Label lblConnectionStatus;
    }
}