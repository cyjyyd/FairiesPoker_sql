using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FairiesPoker
{
    /// <summary>
    /// 支持透明背景的ListBox控件
    /// 通过禁用背景绘制实现透明效果
    /// </summary>
    public class TransparentListBox : ListBox
    {
        private string _emptyText = "";
        private Font _emojiFont;

        public TransparentListBox()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.DrawMode = DrawMode.OwnerDrawFixed;
            this.ItemHeight = 25;
            this.BackColor = Color.Transparent;
            this.BorderStyle = BorderStyle.None;

            // 使用支持emoji的字体
            _emojiFont = new Font("Segoe UI Emoji", 9.75f, FontStyle.Regular);
            this.Font = _emojiFont;
        }

        /// <summary>
        /// 背景透明度 (保留属性兼容性)
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Opacity
        {
            get { return 200; }
            set { }
        }

        /// <summary>
        /// 背景基础颜色 (保留属性兼容性)
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BackgroundBaseColor
        {
            get { return Color.FromArgb(30, 35, 45); }
            set { }
        }

        /// <summary>
        /// 内容为空时显示的提示文字
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string EmptyText
        {
            get { return _emptyText; }
            set
            {
                _emptyText = value;
                this.Invalidate();
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20; // WS_EX_TRANSPARENT - 使窗口透明
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // 绘制半透明背景（Alpha=120，约50%透明度）
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(120, 30, 35, 45)))
            {
                e.Graphics.FillRectangle(bgBrush, this.ClientRectangle);
            }

            // 绘制边框
            using (Pen borderPen = new Pen(Color.FromArgb(80, Color.Gray)))
            {
                e.Graphics.DrawRectangle(borderPen, new Rectangle(0, 0, this.Width - 1, this.Height - 1));
            }

            // 如果内容为空，显示提示文字
            if (this.Items.Count == 0 && !string.IsNullOrEmpty(_emptyText))
            {
                TextRenderer.DrawText(e.Graphics, _emptyText, _emojiFont, this.ClientRectangle,
                    Color.FromArgb(150, Color.Gray), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            // 绘制项目
            for (int i = 0; i < this.Items.Count; i++)
            {
                Rectangle itemRect = this.GetItemRectangle(i);

                // 如果项目在可视区域内
                if (itemRect.IntersectsWith(this.ClientRectangle))
                {
                    bool isSelected = (this.SelectedIndex == i);

                    // 绘制选中状态背景
                    if (isSelected)
                    {
                        using (SolidBrush selectBrush = new SolidBrush(Color.FromArgb(150, 70, 130, 180)))
                        {
                            e.Graphics.FillRectangle(selectBrush, itemRect);
                        }
                    }

                    // 绘制项目文本
                    string text = this.GetItemText(this.Items[i]);
                    Color textColor = isSelected ? Color.White : Color.FromArgb(220, Color.White);
                    Rectangle textRect = new Rectangle(itemRect.X + 5, itemRect.Y, itemRect.Width - 10, itemRect.Height);
                    TextRenderer.DrawText(e.Graphics, text, _emojiFont, textRect, textColor,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
            }

            // 绘制滚动条区域指示（如果需要滚动）
            if (this.Items.Count > 0)
            {
                int visibleCount = this.ClientSize.Height / this.ItemHeight;
                if (this.Items.Count > visibleCount)
                {
                    int scrollBarWidth = 8;
                    int scrollBarHeight = Math.Max(20, (int)((float)visibleCount / this.Items.Count * this.ClientSize.Height));
                    int scrollBarTop = (int)((float)this.TopIndex / this.Items.Count * this.ClientSize.Height);

                    using (SolidBrush scrollBrush = new SolidBrush(Color.FromArgb(100, Color.Gray)))
                    {
                        e.Graphics.FillRectangle(scrollBrush,
                            new Rectangle(this.ClientSize.Width - scrollBarWidth - 2, scrollBarTop, scrollBarWidth, scrollBarHeight));
                    }
                }
            }
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            this.Invalidate();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            const int WM_VSCROLL = 0x115;
            const int WM_MOUSEWHEEL = 0x20A;
            if (m.Msg == WM_VSCROLL || m.Msg == WM_MOUSEWHEEL)
            {
                this.Invalidate();
            }
        }
    }
}