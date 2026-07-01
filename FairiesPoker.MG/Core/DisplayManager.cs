using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FairiesPoker.MG.Core;

/// <summary>
/// 全局显示换算：游戏逻辑和UI保持1280x720虚拟坐标，输出到实际窗口分辨率。
/// </summary>
public static class DisplayManager
{
    public const int DesignWidth = 1280;
    public const int DesignHeight = 720;

    public static int BackBufferWidth { get; private set; } = DesignWidth;
    public static int BackBufferHeight { get; private set; } = DesignHeight;
    public static float Scale { get; private set; } = 1f;
    public static float ScaleX { get; private set; } = 1f;
    public static float ScaleY { get; private set; } = 1f;
    public static Vector2 Offset { get; private set; } = Vector2.Zero;
    public static Matrix TransformMatrix { get; private set; } = Matrix.Identity;

    public static Rectangle VirtualBounds => new(0, 0, DesignWidth, DesignHeight);
    public static float ViewportAspect => BackBufferHeight > 0 ? BackBufferWidth / (float)BackBufferHeight : DesignWidth / (float)DesignHeight;
    public static bool IsUltrawide => ViewportAspect > DesignWidth / (float)DesignHeight + 0.15f;

    public static Rectangle FullViewportVirtualBounds
    {
        get
        {
            float safeScaleX = IsValidScale(ScaleX) ? ScaleX : 1f;
            float safeScaleY = IsValidScale(ScaleY) ? ScaleY : 1f;
            float x = -Offset.X / safeScaleX;
            float y = -Offset.Y / safeScaleY;
            float width = BackBufferWidth / safeScaleX;
            float height = BackBufferHeight / safeScaleY;

            return new Rectangle(
                (int)Math.Floor(x),
                (int)Math.Floor(y),
                (int)Math.Ceiling(width),
                (int)Math.Ceiling(height));
        }
    }

    public static void Update(int backBufferWidth, int backBufferHeight)
    {
        BackBufferWidth = Math.Max(1, backBufferWidth);
        BackBufferHeight = Math.Max(1, backBufferHeight);

        Scale = Math.Min(
            BackBufferWidth / (float)DesignWidth,
            BackBufferHeight / (float)DesignHeight);
        if (!IsValidScale(Scale)) Scale = 1f;
        ScaleX = Scale;
        ScaleY = Scale;

        Offset = new Vector2(
            (BackBufferWidth - DesignWidth * Scale) / 2f,
            (BackBufferHeight - DesignHeight * Scale) / 2f);

        TransformMatrix =
            Matrix.CreateScale(Scale, Scale, 1f) *
            Matrix.CreateTranslation(Offset.X, Offset.Y, 0f);
    }

    public static Vector2 ToVirtual(Vector2 screenPosition)
    {
        return new Vector2(
            (screenPosition.X - Offset.X) / (IsValidScale(ScaleX) ? ScaleX : 1f),
            (screenPosition.Y - Offset.Y) / (IsValidScale(ScaleY) ? ScaleY : 1f));
    }

    public static float AnchorToVisibleLeft(float defaultX, float margin)
    {
        if (!IsUltrawide)
            return defaultX;

        return Math.Min(defaultX, FullViewportVirtualBounds.Left + margin);
    }

    public static float AnchorToVisibleRight(float defaultX, float elementWidth, float margin)
    {
        if (!IsUltrawide)
            return defaultX;

        return Math.Max(defaultX, FullViewportVirtualBounds.Right - margin - elementWidth);
    }

    public static void DrawFullViewportBackground(SpriteBatch spriteBatch, Texture2D texture, Color color)
    {
        Rectangle bounds = FullViewportVirtualBounds;
        spriteBatch.Draw(texture, bounds, GetCoverSourceRectangle(texture, bounds), color);
    }

    private static Rectangle GetCoverSourceRectangle(Texture2D texture, Rectangle destination)
    {
        if (destination.Width <= 0 || destination.Height <= 0 || texture.Width <= 0 || texture.Height <= 0)
            return new Rectangle(0, 0, Math.Max(1, texture.Width), Math.Max(1, texture.Height));

        float destinationAspect = destination.Width / (float)destination.Height;
        float textureAspect = texture.Width / (float)texture.Height;

        if (textureAspect > destinationAspect)
        {
            int sourceWidth = Math.Max(1, (int)Math.Round(texture.Height * destinationAspect));
            int sourceX = Math.Max(0, (texture.Width - sourceWidth) / 2);
            return new Rectangle(sourceX, 0, Math.Min(sourceWidth, texture.Width - sourceX), texture.Height);
        }

        int sourceHeight = Math.Max(1, (int)Math.Round(texture.Width / destinationAspect));
        int sourceY = Math.Max(0, (texture.Height - sourceHeight) / 2);
        return new Rectangle(0, sourceY, texture.Width, Math.Min(sourceHeight, texture.Height - sourceY));
    }

    private static bool IsValidScale(float scale)
    {
        return scale > 0f && !float.IsNaN(scale) && !float.IsInfinity(scale);
    }
}
