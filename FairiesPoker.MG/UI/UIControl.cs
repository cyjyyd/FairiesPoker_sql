using FairiesPoker.MG.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FairiesPoker.MG.UI;

/// <summary>
/// UI控件基类
/// </summary>
public abstract class UIControl
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Size { get; set; } = Vector2.Zero;
    public bool Visible { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public object? Tag { get; set; }

    public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);

    public virtual bool ContainsPoint(Point point) => Bounds.Contains(point);
    public virtual void Update(InputManager input) { }
    public virtual void Draw(SpriteBatch sb) { }
}
