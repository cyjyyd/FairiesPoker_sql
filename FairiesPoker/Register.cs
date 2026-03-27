using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Protocol.Code;
using Protocol.Dto;

namespace FairiesPoker
{
    public partial class Register : Form
    {
        NetManager netManager;
        byte[] _customAvatarData = null; // 自定义头像数据

        public Register(NetManager netManager)
        {
            InitializeComponent();
            this.netManager = netManager;
            // 订阅注册结果事件
            Models.OnRegisterResult += OnRegisterResultHandler;
        }

        /// <summary>
        /// 注册结果处理
        /// </summary>
        private void OnRegisterResultHandler(bool success)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<bool>(OnRegisterResultHandler), success);
                return;
            }

            if (success)
            {
                // 保存头像数据到临时文件，登录后上传
                if (_customAvatarData != null)
                {
                    try
                    {
                        string tempPath = System.IO.Path.Combine(Application.StartupPath, "temp_avatar.dat");
                        System.IO.File.WriteAllBytes(tempPath, _customAvatarData);
                    }
                    catch { }
                }

                MessageBox.Show("注册成功！请使用新账号登录。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // 关闭注册窗口，返回登录页面
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("注册失败，用户名可能已存在，请重试", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                btnRegister.Enabled = true;
                lblStatus.Text = "";
            }
        }

        /// <summary>
        /// 窗口关闭时取消订阅
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Models.OnRegisterResult -= OnRegisterResultHandler;
            base.OnFormClosed(e);
        }

        private void Register_Shown(object sender, EventArgs e)
        {
            // 不再加载预设头像列表
        }

        private void btnUploadAvatar_Click(object sender, EventArgs e)
        {
            // 打开图片裁切窗口
            using var form = new ImageCropperForm(netManager, false);
            if (form.ShowDialog() == DialogResult.OK && form.CroppedImageData != null)
            {
                _customAvatarData = form.CroppedImageData;
                // 显示预览
                using var ms = new MemoryStream(_customAvatarData);
                pictureBox1.BackgroundImage = Image.FromStream(ms);
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            // 验证用户名
            string username = txtUsername.Text.Trim();
            if (username.Length < 4 || username.Length > 16)
            {
                MessageBox.Show("用户名长度需要在4-16个字符之间！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            // 验证密码
            string password = txtPassword.Text;
            if (password.Length < 6 || password.Length > 16)
            {
                MessageBox.Show("密码长度需要在6-16个字符之间！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            // 确认密码
            if (password != txtConfirmPassword.Text)
            {
                MessageBox.Show("两次输入的密码不一致！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPassword.Focus();
                return;
            }

            // MD5加密密码
            string pwdHash = null;
            MD5 md5 = MD5.Create();
            byte[] passwordBytes = Encoding.Default.GetBytes(password);
            byte[] encryptpassword = md5.ComputeHash(passwordBytes);
            for (int i = 0; i < encryptpassword.Length; i++)
            {
                pwdHash = pwdHash + encryptpassword[i].ToString("X");
            }

            try
            {
                btnRegister.Enabled = false;
                lblStatus.Text = "正在注册...";

                AccountDto dto = new AccountDto(username, pwdHash);
                SocketMsg msg = new SocketMsg(OpCode.ACCOUNT, AccountCode.REGIST_CREQ, dto);
                netManager.Execute(0, msg);
            }
            catch (Exception)
            {
                MessageBox.Show("未知错误，请联系开发者", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnRegister.Enabled = true;
                lblStatus.Text = "";
            }
        }

        private void Register_Load(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            netManager.Update();
        }
    }
}