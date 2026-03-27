namespace FairiesPoker
{
    partial class ImageCropperForm
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
            this.imageCropper1 = new FairiesPoker.ImageCropper();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSelectImage = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblTip = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // imageCropper1
            //
            this.imageCropper1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.imageCropper1.Location = new System.Drawing.Point(12, 12);
            this.imageCropper1.Name = "imageCropper1";
            this.imageCropper1.OutputSize = 200;
            this.imageCropper1.Size = new System.Drawing.Size(380, 380);
            this.imageCropper1.TabIndex = 0;
            //
            // btnConfirm
            //
            this.btnConfirm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConfirm.Enabled = false;
            this.btnConfirm.Location = new System.Drawing.Point(250, 420);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(75, 30);
            this.btnConfirm.TabIndex = 1;
            this.btnConfirm.Text = "确认上传";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(331, 420);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(61, 30);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // btnSelectImage
            //
            this.btnSelectImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSelectImage.Location = new System.Drawing.Point(12, 420);
            this.btnSelectImage.Name = "btnSelectImage";
            this.btnSelectImage.Size = new System.Drawing.Size(90, 30);
            this.btnSelectImage.TabIndex = 3;
            this.btnSelectImage.Text = "选择图片";
            this.btnSelectImage.UseVisualStyleBackColor = true;
            this.btnSelectImage.Click += new System.EventHandler(this.btnSelectImage_Click);
            //
            // lblStatus
            //
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.Location = new System.Drawing.Point(108, 425);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(136, 20);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "请选择图片";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblTip
            //
            this.lblTip.ForeColor = System.Drawing.Color.Gray;
            this.lblTip.Location = new System.Drawing.Point(12, 395);
            this.lblTip.Name = "lblTip";
            this.lblTip.Size = new System.Drawing.Size(380, 22);
            this.lblTip.TabIndex = 5;
            this.lblTip.Text = "提示：拖动裁切框调整位置，裁切框会保持正方形";
            this.lblTip.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // ImageCropperForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(404, 462);
            this.Controls.Add(this.lblTip);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnSelectImage);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.imageCropper1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImageCropperForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "裁切头像";
            this.Load += new System.EventHandler(this.ImageCropperForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private ImageCropper imageCropper1;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSelectImage;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblTip;
    }
}