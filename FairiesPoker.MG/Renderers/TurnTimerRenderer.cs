using FairiesPoker.MG.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FairiesPoker.MG.Renderers;

/// <summary>
/// 回合倒计时渲染器
/// </summary>
public class TurnTimerRenderer
{
    private static readonly Vector2 DefaultPosition = new Vector2(575, 340);

    public int RemainingSeconds { get; set; } = 20;
    public bool IsVisible { get; set; }
    public Vector2 Position { get; set; } = DefaultPosition;

    public void Draw(SpriteBatch sb, SpriteFont font)
    {
        if (!IsVisible) return;

        // 颜色变化: >10s白色, 5-10s黄色, <5s红色
        Color color = RemainingSeconds > 10 ? Color.White :
                       RemainingSeconds > 5 ? Color.Yellow : Color.Red;

        string text = RemainingSeconds.ToString();
        var size = font.MeasureString(text);
        var origin = size / 2;

        sb.DrawString(font, text, Position + new Vector2(1, 1), Color.Black * 0.5f, 0, origin, 1f, SpriteEffects.None, 0);
        sb.DrawString(font, text, Position, color, 0, origin, 1f, SpriteEffects.None, 0);
    }
}
