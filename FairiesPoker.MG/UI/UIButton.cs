using System;
using FairiesPoker.MG.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FairiesPoker.MG.UI;

/// <summary>
/// UI按钮 - 三态(正常/悬停/按下),支持主题图片
/// 无纹理时绘制半透明背景+边框
/// </summary>
public class UIButton : UIControl
{
    public string Text { get; set; } = "";
    public Texture2D? NormalTexture { get; set; }
    public Texture2D? HoverTexture { get; set; }
    public Texture2D? PressedTexture { get; set; }
    public Color TextColor { get; set; } = Color.White;

    /// <summary>无纹理时的背景色</summary>
    public Color BackgroundColor { get; set; } = new Color(40, 40, 50, 180);
    /// <summary>悬停时背景色</summary>
    public Color HoverBackgroundColor { get; set; } = new Color(60, 60, 80, 200);
    /// <summary>按下时背景色</summary>
    public Color PressedBackgroundColor { get; set; } = new Color(30, 30, 40, 220);
    /// <summary>边框颜色</summary>
    public Color BorderColor { get; set; } = new Color(120, 120, 140, 200);

    public bool IsHovered { get; private set; }
    public bool IsPressed { get; private set; }

    public Action? OnClick;
    public Action? OnHoverChanged;

    private static Texture2D? _whitePixel;
    private static Texture2D WhitePixel
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

    public override void Update(InputManager input)
    {
        if (!Visible || !Enabled)
        {
            if (IsHovered) { IsHovered = false; OnHoverChanged?.Invoke(); }
            IsPressed = false;
            return;
        }

        var mousePos = new Point((int)input.MousePosition.X, (int)input.MousePosition.Y);
        bool wasHovered = IsHovered;
        IsHovered = ContainsPoint(mousePos);

        if (wasHovered != IsHovered) OnHoverChanged?.Invoke();

        // 按下状态追踪
        if (IsHovered && input.LeftMouseClicked)
        {
            IsPressed = true;
        }
        else if (IsHovered && input.LeftMouseReleased && IsPressed)
        {
            // 在同一个按钮上按下并释放 → 触发点击
            IsPressed = false;
            OnClick?.Invoke();
        }
        else if (!input.LeftMouseHeld)
        {
            IsPressed = false;
        }
    }

    public override void Draw(SpriteBatch sb)
    {
        if (!Visible) return;

        Texture2D? texture = IsPressed ? PressedTexture : (IsHovered ? HoverTexture : NormalTexture);

        if (texture != null)
        {
            sb.Draw(texture, Bounds, Color.White);
        }
        else
        {
            // 回退: 绘制纯色背景 + 边框
            Color bgColor = IsPressed ? PressedBackgroundColor : (IsHovered ? HoverBackgroundColor : BackgroundColor);
            sb.Draw(WhitePixel, Bounds, bgColor);

            // 四边边框 (1px)
            sb.Draw(WhitePixel, new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, 1), BorderColor); // top
            sb.Draw(WhitePixel, new Rectangle((int)Position.X, (int)Position.Y, 1, (int)Size.Y), BorderColor); // left
            sb.Draw(WhitePixel, new Rectangle((int)Position.X, (int)Position.Y + (int)Size.Y - 1, (int)Size.X, 1), BorderColor); // bottom
            sb.Draw(WhitePixel, new Rectangle((int)Position.X + (int)Size.X - 1, (int)Position.Y, 1, (int)Size.Y), BorderColor); // right
        }

        if (!string.IsNullOrEmpty(Text))
        {
            var font = FontManager.Default;
            if (font != null)
            {
                var textSize = font.MeasureString(Text);
                var textPos = Position + (Size - textSize) / 2;
                sb.DrawString(font, Text, textPos, TextColor);
            }
        }
    }
}
