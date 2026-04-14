using FairiesPoker.MG.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FairiesPoker.MG.UI;

/// <summary>
/// 带标题的面板 - 替代WinForms的GroupBox
/// </summary>
public class UIGroupBox : UIPanel
{
    public string Title { get; set; } = "";
    public Color TitleColor { get; set; } = Color.White;
    public SpriteFont? Font { get; set; }
    public Color BorderColor { get; set; } = new Color(100, 100, 120, 150);

    public override void Draw(SpriteBatch sb)
    {
        if (!Visible) return;

        var font = Font ?? FontManager.Default;
        if (font == null) return;

        // 背景
        sb.Draw(WhitePixel, Bounds, BackgroundColor);

        // 边框
        sb.Draw(WhitePixel, new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, 1), BorderColor); // top
        sb.Draw(WhitePixel, new Rectangle((int)Position.X, (int)Position.Y, 1, (int)Size.Y), BorderColor); // left
        sb.Draw(WhitePixel, new Rectangle((int)Position.X, (int)Position.Y + (int)Size.Y - 1, (int)Size.X, 1), BorderColor); // bottom
        sb.Draw(WhitePixel, new Rectangle((int)Position.X + (int)Size.X - 1, (int)Position.Y, 1, (int)Size.Y), BorderColor); // right

        // 标题
        if (!string.IsNullOrEmpty(Title))
        {
            var titleSize = font.MeasureString(Title);
            sb.DrawString(font, Title, new Vector2(Position.X + 8, Position.Y - titleSize.Y / 2), TitleColor);
        }

        // 子控件
        foreach (var child in Children)
            child.Draw(sb);
    }
}
