using System;
using System.Collections.Generic;
using FairiesPoker.MG.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace FairiesPoker.MG.UI;

/// <summary>
/// 可滚动列表 - 半透明背景,替代TransparentListBox
/// </summary>
public class UIListBox : UIControl
{
    public List<string> Items { get; } = new();
    public int SelectedIndex { get; set; } = -1;
    public SpriteFont? Font { get; set; }

    public int ItemHeight { get; set; } = 24;
    public Color BackgroundColor { get; set; } = new Color(30, 35, 45, 120);
    public Color TextColor { get; set; } = Color.White;
    public Color SelectedTextColor { get; set; } = new Color(100, 180, 255);
    public Color ScrollBarColor { get; set; } = new Color(100, 100, 120, 180);

    public int ScrollOffset { get; set; }

    public Action<int>? OnItemSelected;

    private Texture2D? _whitePixel;
    private Texture2D WhitePixel
    {
        get
        {
            if (_whitePixel == null)
            {
                _whitePixel = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
                _whitePixel.SetData(new[] { Color.White });
            }
            return _whitePixel;
        }
    }

    public void AddItem(string text) => Items.Add(text);
    public void Clear() { Items.Clear(); SelectedIndex = -1; ScrollOffset = 0; }

    public override void Update(InputManager input)
    {
        if (!Visible || !Enabled) return;

        var mousePos = new Point((int)input.MousePosition.X, (int)input.MousePosition.Y);

        // 滚轮滚动
        if (ContainsPoint(mousePos) && input.MouseWheelDelta != 0)
        {
            ScrollOffset -= input.MouseWheelDelta / 120;
            ScrollOffset = Math.Max(0, Math.Min(ScrollOffset, Math.Max(0, Items.Count - (int)(Size.Y / ItemHeight))));
        }

        // 点击选中
        if (input.LeftMouseClicked && ContainsPoint(mousePos))
        {
            int relativeY = mousePos.Y - (int)Position.Y + ScrollOffset * ItemHeight;
            int index = relativeY / ItemHeight;
            if (index >= 0 && index < Items.Count)
            {
                SelectedIndex = index;
                OnItemSelected?.Invoke(index);
            }
        }
    }

    public override void Draw(SpriteBatch sb)
    {
        if (!Visible) return;

        var font = Font ?? FontManager.Default;
        if (font == null) return;

        // 背景
        sb.Draw(WhitePixel, Bounds, BackgroundColor);

        // 绘制可见项
        int visibleItems = (int)(Size.Y / ItemHeight);
        for (int i = 0; i < visibleItems; i++)
        {
            int index = ScrollOffset + i;
            if (index >= Items.Count) break;

            var itemRect = new Rectangle(
                (int)Position.X + 4,
                (int)Position.Y + i * ItemHeight + 2,
                (int)Size.X - 8,
                ItemHeight - 4
            );

            if (index == SelectedIndex)
            {
                sb.Draw(WhitePixel, itemRect, new Color(50, 100, 180, 80));
            }

            sb.DrawString(font, Items[index], new Vector2(itemRect.X, itemRect.Y),
                index == SelectedIndex ? SelectedTextColor : TextColor);
        }

        // 滚动条
        if (Items.Count > visibleItems)
        {
            int scrollBarWidth = 6;
            int scrollBarHeight = (int)(Size.Y * visibleItems / (float)Items.Count);
            int scrollBarY = (int)(Position.Y + ScrollOffset * ItemHeight * visibleItems / (float)Items.Count);
            sb.Draw(WhitePixel, new Rectangle((int)Position.X + (int)Size.X - scrollBarWidth - 2, scrollBarY, scrollBarWidth, scrollBarHeight), ScrollBarColor);
        }
    }
}
