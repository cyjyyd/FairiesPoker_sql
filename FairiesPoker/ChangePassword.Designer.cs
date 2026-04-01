namespace FairiesPoker
{
    partial class ChangePassword
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            lblTitle = new System.Windows.Forms.Label();
            lblUsername = new System.Windows.Forms.Label();
            lblOldPassword = new System.Windows.Forms.Label();
            lblNewPassword = new System.Windows.Forms.Label();
            lblConfirmPassword = new System.Windows.Forms.Label();
            txtUsername = new System.Windows.Forms.TextBox();
            txtOldPassword = new System.Windows.Forms.TextBox();
            txtNewPassword = new System.Windows.Forms.TextBox();
            txtConfirmPassword = new System.Windows.Forms.TextBox();
            btnConfirm = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            lblStatus = new System.Windows.Forms.Label();
            timer1 = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.BackColor = System.Drawing.Color.Transparent;
            lblTitle.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.White;
            lblTitle.Location = new System.Drawing.Point(120, 30);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(110, 31);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "修改密码";
            //
            // lblUsername
            //
            lblUsername.AutoSize = true;
            lblUsername.BackColor = System.Drawing.Color.Transparent;
            lblUsername.Font = new System.Drawing.Font("微软雅黑", 12F);
            lblUsername.ForeColor = System.Drawing.Color.White;
            lblUsername.Location = new System.Drawing.Point(40, 90);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new System.Drawing.Size(74, 21);
            lblUsername.TabIndex = 1;
            lblUsername.Text = "用户名：";
            //
            // lblOldPassword
            //
            lblOldPassword.AutoSize = true;
            lblOldPassword.BackColor = System.Drawing.Color.Transparent;
            lblOldPassword.Font = new System.Drawing.Font("微软雅黑", 12F);
            lblOldPassword.ForeColor = System.Drawing.Color.White;
            lblOldPassword.Location = new System.Drawing.Point(40, 140);
            lblOldPassword.Name = "lblOldPassword";
            lblOldPassword.Size = new System.Drawing.Size(74, 21);
            lblOldPassword.TabIndex = 2;
            lblOldPassword.Text = "旧密码：";
            //
            // lblNewPassword
            //
            lblNewPassword.AutoSize = true;
            lblNewPassword.BackColor = System.Drawing.Color.Transparent;
            lblNewPassword.Font = new System.Drawing.Font("微软雅黑", 12F);
            lblNewPassword.ForeColor = System.Drawing.Color.White;
            lblNewPassword.Location = new System.Drawing.Point(40, 190);
            lblNewPassword.Name = "lblNewPassword";
            lblNewPassword.Size = new System.Drawing.Size(74, 21);
            lblNewPassword.TabIndex = 3;
            lblNewPassword.Text = "新密码：";
            //
            // lblConfirmPassword
            //
            lblConfirmPassword.AutoSize = true;
            lblConfirmPassword.BackColor = System.Drawing.Color.Transparent;
            lblConfirmPassword.Font = new System.Drawing.Font("微软雅黑", 12F);
            lblConfirmPassword.ForeColor = System.Drawing.Color.White;
            lblConfirmPassword.Location = new System.Drawing.Point(25, 240);
            lblConfirmPassword.Name = "lblConfirmPassword";
            lblConfirmPassword.Size = new System.Drawing.Size(89, 21);
            lblConfirmPassword.TabIndex = 4;
            lblConfirmPassword.Text = "确认密码：";
            //
            // txtUsername
            //
            txtUsername.Font = new System.Drawing.Font("微软雅黑", 12F);
            txtUsername.Location = new System.Drawing.Point(125, 87);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new System.Drawing.Size(200, 29);
            txtUsername.TabIndex = 5;
            //
            // txtOldPassword
            //
            txtOldPassword.Font = new System.Drawing.Font("微软雅黑", 12F);
            txtOldPassword.Location = new System.Drawing.Point(125, 137);
            txtOldPassword.Name = "txtOldPassword";
            txtOldPassword.Size = new System.Drawing.Size(200, 29);
            txtOldPassword.TabIndex = 6;
            txtOldPassword.UseSystemPasswordChar = true;
            //
            // txtNewPassword
            //
            txtNewPassword.Font = new System.Drawing.Font("微软雅黑", 12F);
            txtNewPassword.Location = new System.Drawing.Point(125, 187);
            txtNewPassword.Name = "txtNewPassword";
            txtNewPassword.Size = new System.Drawing.Size(200, 29);
            txtNewPassword.TabIndex = 7;
            txtNewPassword.UseSystemPasswordChar = true;
            //
            // txtConfirmPassword
            //
            txtConfirmPassword.Font = new System.Drawing.Font("微软雅黑", 12F);
            txtConfirmPassword.Location = new System.Drawing.Point(125, 237);
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.Size = new System.Drawing.Size(200, 29);
            txtConfirmPassword.TabIndex = 8;
            txtConfirmPassword.UseSystemPasswordChar = true;
            //
            // btnConfirm
            //
            btnConfirm.Font = new System.Drawing.Font("微软雅黑", 12F);
            btnConfirm.Location = new System.Drawing.Point(50, 300);
            btnConfirm.Name = "btnConfirm";
            btnConfirm.Size = new System.Drawing.Size(120, 40);
            btnConfirm.TabIndex = 9;
            btnConfirm.Text = "确认修改";
            btnConfirm.UseVisualStyleBackColor = true;
            btnConfirm.Click += btnConfirm_Click;
            //
            // btnCancel
            //
            btnCancel.Font = new System.Drawing.Font("微软雅黑", 12F);
            btnCancel.Location = new System.Drawing.Point(200, 300);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(120, 40);
            btnCancel.TabIndex = 10;
            btnCancel.Text = "取消";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            //
            // lblStatus
            //
            lblStatus.AutoSize = true;
            lblStatus.BackColor = System.Drawing.Color.Transparent;
            lblStatus.Font = new System.Drawing.Font("微软雅黑", 10F);
            lblStatus.ForeColor = System.Drawing.Color.Yellow;
            lblStatus.Location = new System.Drawing.Point(125, 355);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(0, 18);
            lblStatus.TabIndex = 11;
            //
            // timer1
            //
            timer1.Interval = 50;
            timer1.Tick += timer1_Tick;
            //
            // ChangePassword
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(40, 45, 55);
            ClientSize = new System.Drawing.Size(370, 390);
            Controls.Add(lblStatus);
            Controls.Add(btnCancel);
            Controls.Add(btnConfirm);
            Controls.Add(txtConfirmPassword);
            Controls.Add(txtNewPassword);
            Controls.Add(txtOldPassword);
            Controls.Add(txtUsername);
            Controls.Add(lblConfirmPassword);
            Controls.Add(lblNewPassword);
            Controls.Add(lblOldPassword);
            Controls.Add(lblUsername);
            Controls.Add(lblTitle);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Name = "ChangePassword";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "修改密码";
            Load += ChangePassword_Load;
            MouseDown += ChangePassword_MouseDown;
            MouseMove += ChangePassword_MouseMove;
            MouseUp += ChangePassword_MouseUp;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.Label lblOldPassword;
        private System.Windows.Forms.Label lblNewPassword;
        private System.Windows.Forms.Label lblConfirmPassword;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtOldPassword;
        private System.Windows.Forms.TextBox txtNewPassword;
        private System.Windows.Forms.TextBox txtConfirmPassword;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Timer timer1;
    }
}