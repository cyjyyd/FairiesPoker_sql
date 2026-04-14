using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FairiesPoker.MG.Core;

/// <summary>
/// 屏幕基类 - 所有UI屏幕的抽象基类
/// </summary>
public abstract class ScreenBase
{
    protected Game1 Game { get; }
    protected ScreenManager ScreenManager { get; }

    public bool IsInitialized { get; private set; }
    public float Opacity { get; set; } = 1f;

    protected ScreenBase(Game1 game, ScreenManager screenManager)
    {
        Game = game;
        ScreenManager = screenManager;
    }

    public virtual void Initialize() { IsInitialized = true; }
    public virtual void LoadContent() { }
    public virtual void UnloadContent() { }

    /// <summary>
    /// 每帧更新逻辑
    /// </summary>
    public abstract void Update(GameTime gameTime);

    /// <summary>
    /// 每帧渲染
    /// </summary>
    public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

    /// <summary>
    /// 输入处理
    /// </summary>
    public abstract void HandleInput(InputManager input);

    /// <summary>
    /// 淡入动画
    /// </summary>
    public void FadeIn(double amount)
    {
        Opacity = (float)System.Math.Min(1.0, Opacity + amount);
    }

    /// <summary>
    /// 淡出动画
    /// </summary>
    public void FadeOut(double amount)
    {
        Opacity = (float)System.Math.Max(0.0, Opacity - amount);
    }
}
