using Microsoft.Xna.Framework;
using System;

namespace FairiesPoker.MG.Core;

/// <summary>
/// 动画辅助类 - 提供缓动函数和插值计算
/// </summary>
public static class AnimationHelper
{
    /// <summary>
    /// 缓动函数类型
    /// </summary>
    public enum EaseType
    {
        Linear,         // 线性
        EaseOutQuad,    // 二次缓出（开始快，结束慢）
        EaseInOutQuad,  // 二次缓入缓出
        EaseOutCubic    // 三次缓出（更平滑的减速）
    }

    /// <summary>
    /// Vector2 插值动画
    /// </summary>
    public static Vector2 Lerp(Vector2 start, Vector2 end, float t, EaseType ease = EaseType.EaseOutQuad)
    {
        float easedT = ApplyEasing(t, ease);
        return Vector2.Lerp(start, end, easedT);
    }

    /// <summary>
    /// 应用缓动函数
    /// </summary>
    private static float ApplyEasing(float t, EaseType ease)
    {
        // 确保 t 在 0-1 范围内
        t = MathHelper.Clamp(t, 0f, 1f);

        return ease switch
        {
            EaseType.Linear => t,
            EaseType.EaseOutQuad => 1f - (1f - t) * (1f - t),           // f(t) = 1 - (1 - t)^2
            EaseType.EaseInOutQuad => t < 0.5f ? 2f * t * t : 1f - (float)Math.Pow(-2f * t + 2f, 2) / 2f,
            EaseType.EaseOutCubic => 1f - (float)Math.Pow(1f - t, 3),  // f(t) = 1 - (1 - t)^3
            _ => t
        };
    }

    /// <summary>
    /// 浮点数插值动画
    /// </summary>
    public static float Lerp(float start, float end, float t, EaseType ease = EaseType.EaseOutQuad)
    {
        float easedT = ApplyEasing(t, ease);
        return start + (end - start) * easedT;
    }
}