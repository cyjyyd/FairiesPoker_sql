using System;
using Microsoft.Xna.Framework;

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
    public static Vector2 Offset { get; private set; } = Vector2.Zero;
    public static Matrix TransformMatrix { get; private set; } = Matrix.Identity;

    public static Rectangle VirtualBounds => new(0, 0, DesignWidth, DesignHeight);

    public static void Update(int backBufferWidth, int backBufferHeight)
    {
        BackBufferWidth = Math.Max(1, backBufferWidth);
        BackBufferHeight = Math.Max(1, backBufferHeight);

        Scale = Math.Min(
            BackBufferWidth / (float)DesignWidth,
            BackBufferHeight / (float)DesignHeight);

        if (Scale <= 0f || float.IsNaN(Scale) || float.IsInfinity(Scale))
        {
            Scale = 1f;
        }

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
            (screenPosition.X - Offset.X) / Scale,
            (screenPosition.Y - Offset.Y) / Scale);
    }
}
