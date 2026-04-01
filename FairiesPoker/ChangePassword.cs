using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Protocol.Code;
using Protocol.Dto;

namespace FairiesPoker
{
    public partial class ChangePassword : Form
    {
        private NetManager netManager;

        public ChangePassword(NetManager netManager)
        {
            InitializeComponent();
            this.netManager = netManager;
            // 订阅修改密码结果事件
            Models.OnChangePasswordResult += OnChangePasswordResultHandler;
        }

        /// <summary>
        /// 修改密码结果处理
        /// </summary>
        private void OnChangePasswordResultHandler(bool success, string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<bool, string>(OnChangePasswordResultHandler), success, message);
                return;
            }

            if (success)
            {
                MessageBox.Show(message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(message, "失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                btnConfirm.Enabled = true;
                lblStatus.Text = "";
            }
        }

        /// <summary>
        /// 窗口关闭时取消订阅
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Models.OnChangePasswordResult -= OnChangePasswordResultHandler;
            base.OnFormClosed(e);
        }

        private void ChangePassword_Load(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (netManager != null)
            {
                netManager.Update();
            }
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            // 验证用户名
            string username = txtUsername.Text.Trim();
            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("请输入用户名！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            // 验证旧密码
            string oldPassword = txtOldPassword.Text;
            if (string.IsNullOrEmpty(oldPassword))
            {
                MessageBox.Show("请输入旧密码！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOldPassword.Focus();
                return;
            }

            // 验证新密码
            string newPassword = txtNewPassword.Text;
            if (newPassword.Length < 6 || newPassword.Length > 16)
            {
                MessageBox.Show("新密码长度需要在6-16个字符之间！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPassword.Focus();
                return;
            }

            // 确认新密码
            if (newPassword != txtConfirmPassword.Text)
            {
                MessageBox.Show("两次输入的新密码不一致！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPassword.Focus();
                return;
            }

            // 新密码不能与旧密码相同
            if (newPassword == oldPassword)
            {
                MessageBox.Show("新密码不能与旧密码相同！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPassword.Focus();
                return;
            }

            // MD5加密密码
            MD5 md5 = MD5.Create();
            string oldPwdHash = GetMd5Hash(oldPassword, md5);
            string newPwdHash = GetMd5Hash(newPassword, md5);

            try
            {
                btnConfirm.Enabled = false;
                lblStatus.Text = "正在修改...";

                ChangePasswordDto dto = new ChangePasswordDto(username, oldPwdHash, newPwdHash);
                SocketMsg msg = new SocketMsg(OpCode.ACCOUNT, AccountCode.CHANGE_PASSWORD_CREQ, dto);
                netManager.Execute(0, msg);
            }
            catch (Exception)
            {
                MessageBox.Show("未知错误，请联系开发者", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnConfirm.Enabled = true;
                lblStatus.Text = "";
            }
        }

        private string GetMd5Hash(string input, MD5 md5)
        {
            byte[] passwordBytes = Encoding.Default.GetBytes(input);
            byte[] encryptpassword = md5.ComputeHash(passwordBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < encryptpassword.Length; i++)
            {
                sb.Append(encryptpassword[i].ToString("X"));
            }
            return sb.ToString();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // 窗口拖动
        private Point mouseOff;
        private bool leftFlag;

        private void ChangePassword_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseOff = new Point(-e.X, -e.Y);
                leftFlag = true;
            }
        }

        private void ChangePassword_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftFlag)
            {
                Point mouseSet = Control.MousePosition;
                mouseSet.Offset(mouseOff.X, mouseOff.Y);
                Location = mouseSet;
            }
        }

        private void ChangePassword_MouseUp(object sender, MouseEventArgs e)
        {
            if (leftFlag)
            {
                leftFlag = false;
            }
        }
    }
}