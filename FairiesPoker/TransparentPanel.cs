using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FairiesPoker
{
    /// <summary>
    /// 支持透明背景的Panel控件
    /// </summary>
    public class TransparentPanel : Panel
    {
        private int _opacity = 120;
        private Color _baseColor = Color.FromArgb(30, 35, 45);

        public TransparentPanel()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Opacity
        {
            get { return _opacity; }
            set
            {
                _opacity = Math.Max(0, Math.Min(255, value));
                this.Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BaseColor
        {
            get { return _baseColor; }
            set
            {
                _baseColor = value;
                this.Invalidate();
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20; // WS_EX_TRANSPARENT
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // 绘制半透明背景
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(_opacity, _baseColor)))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }

            base.OnPaint(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // 不绘制背景，让父控件背景显示
            // 基类不做任何事
        }
    }
}