using System;
using System.Collections.Generic;
using FairiesPoker.MG.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FairiesPoker.MG.UI;

/// <summary>
/// UI面板 - 容器控件
/// </summary>
public class UIPanel : UIControl
{
    public Color BackgroundColor { get; set; } = Color.Transparent;
    public List<UIControl> Children { get; } = new();

    protected Texture2D? _whitePixel;
    protected Texture2D WhitePixel
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

    public void Add(UIControl control) => Children.Add(control);
    public void Clear() => Children.Clear();

    public override void Update(InputManager input)
    {
        if (!Visible) return;
        foreach (var child in Children)
            child.Update(input);
    }

    public override void Draw(SpriteBatch sb)
    {
        if (!Visible) return;

        // 绘制背景
        if (BackgroundColor != Color.Transparent && Size.X > 0 && Size.Y > 0)
        {
            sb.Draw(WhitePixel, Bounds, BackgroundColor);
        }

        // 绘制子控件
        foreach (var child in Children)
            child.Draw(sb);
    }
}
