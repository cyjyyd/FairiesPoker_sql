using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FairiesPoker
{
    /// <summary>
    /// 图片裁切控件 - 支持拖拽裁切框、缩放预览
    /// </summary>
    public class ImageCropper : Control
    {
        private Image _sourceImage;
        private Image _displayImage;
        private float _scale = 1.0f;
        private Point _imageOffset = Point.Empty;
        private Rectangle _cropRect = new Rectangle(50, 50, 100, 100);
        private bool _isDragging = false;
        private bool _isResizing = false;
        private Point _dragStart;
        private int _dragHandle = -1; // 0-7 for corners and edges
        private int _outputSize = 200;

        private const int HandleSize = 10;
        private static readonly Cursor[] HandleCursors = {
            Cursors.SizeNWSE, Cursors.SizeNS, Cursors.SizeNESW,
            Cursors.SizeWE, Cursors.SizeWE,
            Cursors.SizeNESW, Cursors.SizeNS, Cursors.SizeNWSE
        };

        [Category("Appearance")]
        [Description("输出图片尺寸")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int OutputSize
        {
            get => _outputSize;
            set
            {
                _outputSize = value;
                Invalidate();
            }
        }

        [Browsable(false)]
        public bool HasImage => _sourceImage != null;

        public ImageCropper()
        {
            DoubleBuffered = true;
            BackColor = Color.FromArgb(40, 40, 40);
            Size = new Size(400, 400);
        }

        /// <summary>
        /// 加载图片
        /// </summary>
        public void LoadImage(string filePath)
        {
            try
            {
                using var temp = Image.FromFile(filePath);
                _sourceImage = new Bitmap(temp);
                FitImageToControl();
                Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载图片失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 加载图片
        /// </summary>
        public void LoadImage(Image image)
        {
            _sourceImage = new Bitmap(image);
            FitImageToControl();
            Invalidate();
        }

        /// <summary>
        /// 清除图片
        /// </summary>
        public void ClearImage()
        {
            _sourceImage?.Dispose();
            _displayImage?.Dispose();
            _sourceImage = null;
            _displayImage = null;
            Invalidate();
        }

        /// <summary>
        /// 获取裁切后的图片
        /// </summary>
        public Bitmap GetCroppedImage()
        {
            if (_sourceImage == null)
                return null;

            // 计算在原图上的裁切区域
            Rectangle sourceCropRect = GetSourceCropRect();

            // 创建裁切后的图片
            var result = new Bitmap(_outputSize, _outputSize);
            using (var g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(_sourceImage,
                    new Rectangle(0, 0, _outputSize, _outputSize),
                    sourceCropRect,
                    GraphicsUnit.Pixel);
            }
            return result;
        }

        /// <summary>
        /// 适应控件大小
        /// </summary>
        private void FitImageToControl()
        {
            if (_sourceImage == null)
                return;

            // 计算缩放比例，保持宽高比
            float scaleX = (float)(ClientSize.Width - 20) / _sourceImage.Width;
            float scaleY = (float)(ClientSize.Height - 20) / _sourceImage.Height;
            _scale = Math.Min(scaleX, scaleY);

            // 计算居中偏移
            int displayWidth = (int)(_sourceImage.Width * _scale);
            int displayHeight = (int)(_sourceImage.Height * _scale);
            _imageOffset = new Point(
                (ClientSize.Width - displayWidth) / 2,
                (ClientSize.Height - displayHeight) / 2
            );

            // 初始化裁切框（正方形，居中）
            int cropSize = Math.Min(displayWidth, displayHeight) / 2;
            _cropRect = new Rectangle(
                _imageOffset.X + (displayWidth - cropSize) / 2,
                _imageOffset.Y + (displayHeight - cropSize) / 2,
                cropSize,
                cropSize
            );
        }

        /// <summary>
        /// 获取在原图上的裁切区域
        /// </summary>
        private Rectangle GetSourceCropRect()
        {
            int x = (int)((_cropRect.X - _imageOffset.X) / _scale);
            int y = (int)((_cropRect.Y - _imageOffset.Y) / _scale);
            int size = (int)(_cropRect.Width / _scale);
            return new Rectangle(x, y, size, size);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(BackColor);

            if (_sourceImage == null)
            {
                // 显示提示文字
                using var font = new Font("微软雅黑", 12);
                using var brush = new SolidBrush(Color.Gray);
                string text = "点击或拖拽图片到此处";
                var size = g.MeasureString(text, font);
                g.DrawString(text, font, brush, (Width - size.Width) / 2, (Height - size.Height) / 2);
                return;
            }

            // 绘制图片
            g.DrawImage(_sourceImage,
                new Rectangle(_imageOffset, new Size((int)(_sourceImage.Width * _scale), (int)(_sourceImage.Height * _scale))),
                new Rectangle(0, 0, _sourceImage.Width, _sourceImage.Height),
                GraphicsUnit.Pixel);

            // 绘制半透明遮罩
            using var overlayBrush = new SolidBrush(Color.FromArgb(128, Color.Black));
            var imgRect = new Rectangle(_imageOffset, new Size((int)(_sourceImage.Width * _scale), (int)(_sourceImage.Height * _scale)));

            // 上
            g.FillRectangle(overlayBrush, imgRect.Left, imgRect.Top, imgRect.Width, _cropRect.Top - imgRect.Top);
            // 下
            g.FillRectangle(overlayBrush, imgRect.Left, _cropRect.Bottom, imgRect.Width, imgRect.Bottom - _cropRect.Bottom);
            // 左
            g.FillRectangle(overlayBrush, imgRect.Left, _cropRect.Top, _cropRect.Left - imgRect.Left, _cropRect.Height);
            // 右
            g.FillRectangle(overlayBrush, _cropRect.Right, _cropRect.Top, imgRect.Right - _cropRect.Right, _cropRect.Height);

            // 绘制裁切框
            using var cropPen = new Pen(Color.White, 2) { DashStyle = DashStyle.Solid };
            g.DrawRectangle(cropPen, _cropRect);

            // 绘制网格线
            using var gridPen = new Pen(Color.White, 1) { DashStyle = DashStyle.Dash };
            int third = _cropRect.Width / 3;
            g.DrawLine(gridPen, _cropRect.Left + third, _cropRect.Top, _cropRect.Left + third, _cropRect.Bottom);
            g.DrawLine(gridPen, _cropRect.Left + third * 2, _cropRect.Top, _cropRect.Left + third * 2, _cropRect.Bottom);
            g.DrawLine(gridPen, _cropRect.Left, _cropRect.Top + third, _cropRect.Right, _cropRect.Top + third);
            g.DrawLine(gridPen, _cropRect.Left, _cropRect.Top + third * 2, _cropRect.Right, _cropRect.Top + third * 2);

            // 绘制调整手柄
            DrawHandles(g);
        }

        private void DrawHandles(Graphics g)
        {
            using var handleBrush = new SolidBrush(Color.White);
            var handles = GetHandleRects();
            foreach (var rect in handles)
            {
                g.FillRectangle(handleBrush, rect);
            }
        }

        private Rectangle[] GetHandleRects()
        {
            int hs = HandleSize;
            return new Rectangle[]
            {
                // 角落
                new Rectangle(_cropRect.Left - hs/2, _cropRect.Top - hs/2, hs, hs),
                new Rectangle(_cropRect.Right - hs/2, _cropRect.Top - hs/2, hs, hs),
                new Rectangle(_cropRect.Left - hs/2, _cropRect.Bottom - hs/2, hs, hs),
                new Rectangle(_cropRect.Right - hs/2, _cropRect.Bottom - hs/2, hs, hs),
                // 边缘中点
                new Rectangle(_cropRect.Left + _cropRect.Width/2 - hs/2, _cropRect.Top - hs/2, hs, hs),
                new Rectangle(_cropRect.Left + _cropRect.Width/2 - hs/2, _cropRect.Bottom - hs/2, hs, hs),
                new Rectangle(_cropRect.Left - hs/2, _cropRect.Top + _cropRect.Height/2 - hs/2, hs, hs),
                new Rectangle(_cropRect.Right - hs/2, _cropRect.Top + _cropRect.Height/2 - hs/2, hs, hs),
            };
        }

        private int GetHandleAtPoint(Point pt)
        {
            var handles = GetHandleRects();
            for (int i = 0; i < handles.Length; i++)
            {
                if (handles[i].Contains(pt))
                    return i;
            }
            return -1;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (_sourceImage == null || e.Button != MouseButtons.Left)
                return;

            _dragHandle = GetHandleAtPoint(e.Location);
            if (_dragHandle >= 0)
            {
                _isResizing = true;
                _dragStart = e.Location;
            }
            else if (_cropRect.Contains(e.Location))
            {
                _isDragging = true;
                _dragStart = e.Location;
            }

            if (_isDragging || _isResizing)
                Capture = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_sourceImage == null)
                return;

            // 更新光标
            int handle = GetHandleAtPoint(e.Location);
            if (handle >= 0)
            {
                Cursor = HandleCursors[handle < 4 ? handle : (handle < 6 ? 1 : 3)];
            }
            else if (_cropRect.Contains(e.Location))
            {
                Cursor = Cursors.SizeAll;
            }
            else
            {
                Cursor = Cursors.Default;
            }

            if (_isDragging)
            {
                // 移动裁切框
                int dx = e.X - _dragStart.X;
                int dy = e.Y - _dragStart.Y;
                _cropRect.Offset(dx, dy);
                _dragStart = e.Location;
                Invalidate();
            }
            else if (_isResizing)
            {
                // 调整裁切框大小（保持正方形）
                int delta = Math.Max(e.X - _dragStart.X, e.Y - _dragStart.Y);
                int newSize = Math.Max(50, _cropRect.Width + (e.X > _dragStart.X ? delta : -delta));
                newSize = Math.Max(50, newSize);

                int centerX = _cropRect.X + _cropRect.Width / 2;
                int centerY = _cropRect.Y + _cropRect.Height / 2;
                _cropRect = new Rectangle(centerX - newSize / 2, centerY - newSize / 2, newSize, newSize);

                _dragStart = e.Location;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isDragging = false;
            _isResizing = false;
            _dragHandle = -1;
            Capture = false;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_sourceImage != null)
                FitImageToControl();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sourceImage?.Dispose();
                _displayImage?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}