using FairiesPoker.MG.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FairiesPoker.MG.UI;

/// <summary>
/// UI文字标签 - SpriteFont渲染
/// 可选背景色和边框
/// </summary>
public class UILabel : UIControl
{
    public string Text { get; set; } = "";
    public Color TextColor { get; set; } = Color.White;
    public SpriteFont? Font { get; set; }
    public float Scale { get; set; } = 1f;

    public enum AlignmentType { Left, Center, Right }
    public AlignmentType TextAlignment { get; set; } = AlignmentType.Left;

    /// <summary>背景色(默认透明)</summary>
    public Color BackgroundColor { get; set; } = Color.Transparent;
    /// <summary>边框颜色(默认透明=无边框)</summary>
    public Color BorderColor { get; set; } = Color.Transparent;

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

    public override void Draw(SpriteBatch sb)
    {
        if (!Visible || string.IsNullOrEmpty(Text)) return;

        var font = Font ?? FontManager.Default;
        if (font == null) return;

        // 背景
        if (BackgroundColor != Color.Transparent && Size.X > 0 && Size.Y > 0)
        {
            sb.Draw(WhitePixel, Bounds, BackgroundColor);
        }

        // 边框
        if (BorderColor != Color.Transparent && Size.X > 0 && Size.Y > 0)
        {
            sb.Draw(WhitePixel, new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, 1), BorderColor);
            sb.Draw(WhitePixel, new Rectangle((int)Position.X, (int)Position.Y, 1, (int)Size.Y), BorderColor);
            sb.Draw(WhitePixel, new Rectangle((int)Position.X, (int)Position.Y + (int)Size.Y - 1, (int)Size.X, 1), BorderColor);
            sb.Draw(WhitePixel, new Rectangle((int)Position.X + (int)Size.X - 1, (int)Position.Y, 1, (int)Size.Y), BorderColor);
        }

        var textSize = font.MeasureString(Text) * Scale;
        float x = TextAlignment switch
        {
            AlignmentType.Center => Position.X + (Size.X - textSize.X) / 2,
            AlignmentType.Right => Position.X + Size.X - textSize.X,
            _ => Position.X
        };

        sb.DrawString(font, Text, new Vector2(x, Position.Y), TextColor, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
    }
}
