using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Protocol.Code;
using Protocol.Dto;

namespace FairiesPoker
{
    public partial class ImageCropperForm : Form
    {
        private NetManager _netManager;
        private string _selectedFilePath;
        private byte[] _croppedImageData;
        private bool _autoUpload = true; // 是否自动上传

        public byte[] CroppedImageData => _croppedImageData;
        public bool UploadSuccess { get; private set; }

        /// <summary>
        /// 创建裁切窗口（自动上传模式）
        /// </summary>
        public ImageCropperForm(NetManager netManager) : this(netManager, true)
        {
        }

        /// <summary>
        /// 创建裁切窗口
        /// </summary>
        /// <param name="netManager">网络管理器</param>
        /// <param name="autoUpload">是否在确认后自动上传</param>
        public ImageCropperForm(NetManager netManager, bool autoUpload)
        {
            InitializeComponent();
            _netManager = netManager;
            _autoUpload = autoUpload;

            if (_autoUpload)
            {
                // 订阅头像上传结果事件
                Models.OnAvatarUploadResult += OnAvatarUploadResult;
            }
        }

        private void ImageCropperForm_Load(object sender, EventArgs e)
        {
            // 自动弹出文件选择对话框
            SelectImage();
        }

        private void btnSelectImage_Click(object sender, EventArgs e)
        {
            SelectImage();
        }

        private void SelectImage()
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.gif;*.bmp|所有文件|*.*";
            ofd.Title = "选择头像图片";
            ofd.Multiselect = false;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (!ImageHelper.IsValidImageFormat(ofd.FileName))
                {
                    MessageBox.Show("请选择有效的图片文件（JPG/PNG/GIF/BMP）", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 检查文件大小（限制10MB）
                var fileInfo = new FileInfo(ofd.FileName);
                if (fileInfo.Length > 10 * 1024 * 1024)
                {
                    MessageBox.Show("图片文件过大，请选择小于10MB的图片", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _selectedFilePath = ofd.FileName;
                LoadImageToCropper();
            }
            else
            {
                // 用户取消选择，关闭窗口
                if (!imageCropper1.HasImage)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            }
        }

        private void LoadImageToCropper()
        {
            if (string.IsNullOrEmpty(_selectedFilePath) || !File.Exists(_selectedFilePath))
                return;

            try
            {
                using var image = ImageHelper.LoadImageFromFile(_selectedFilePath);
                imageCropper1.LoadImage(image);
                lblStatus.Text = $"已加载: {System.IO.Path.GetFileName(_selectedFilePath)}";
                btnConfirm.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载图片失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (!imageCropper1.HasImage)
            {
                MessageBox.Show("请先选择图片", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 获取裁切后的图片
                using var croppedImage = imageCropper1.GetCroppedImage();
                if (croppedImage == null)
                {
                    MessageBox.Show("裁切失败，请重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 压缩图片
                int quality = ImageHelper.CalculateQuality(100, 200);
                _croppedImageData = ImageHelper.CompressToJpeg(croppedImage, quality);

                // 检查压缩后的大小
                if (_croppedImageData.Length > 500 * 1024)
                {
                    // 再次压缩
                    _croppedImageData = ImageHelper.CompressToJpeg(croppedImage, 60);
                }

                if (_autoUpload)
                {
                    // 自动上传模式
                    lblStatus.Text = "正在上传...";

                    // 发送上传请求
                    var dto = new AvatarDto(_croppedImageData, System.IO.Path.GetFileName(_selectedFilePath));
                    var msg = new SocketMsg(OpCode.AVATAR, AvatarCode.UPLOAD_CREQ, dto);
                    _netManager.Execute(0, msg);

                    btnConfirm.Enabled = false;
                }
                else
                {
                    // 仅获取数据模式，直接返回
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"处理图片失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnConfirm.Enabled = true;
            }
        }

        private void OnAvatarUploadResult(bool success, string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<bool, string>(OnAvatarUploadResult), success, message);
                return;
            }

            if (success)
            {
                UploadSuccess = true;
                MessageBox.Show(message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(message, "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnConfirm.Enabled = true;
                lblStatus.Text = message;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_autoUpload)
            {
                Models.OnAvatarUploadResult -= OnAvatarUploadResult;
            }
            base.OnFormClosed(e);
        }
    }
}