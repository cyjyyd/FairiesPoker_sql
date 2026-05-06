using Microsoft.Xna.Framework;

namespace FairiesPoker.MG.Core;

/// <summary>
/// 单张卡牌动画状态
/// 用于记录和管理一张牌从起点飞向目标的动画过程
/// </summary>
public class CardAnimation
{
    /// <summary>
    /// 牌索引（在pai数组中的位置）
    /// </summary>
    public int CardIndex;

    /// <summary>
    /// 起始位置
    /// </summary>
    public Vector2 StartPosition;

    /// <summary>
    /// 目标位置
    /// </summary>
    public Vector2 TargetPosition;

    /// <summary>
    /// 当前位置（动画过程中动态更新）
    /// </summary>
    public Vector2 CurrentPosition;

    /// <summary>
    /// 动画时长（毫秒）
    /// </summary>
    public float DurationMs;

    /// <summary>
    /// 已经过时间（毫秒）
    /// </summary>
    public float ElapsedMs;

    /// <summary>
    /// 是否完成
    /// </summary>
    public bool IsComplete;

    /// <summary>
    /// 是否牌背（用于对手牌，不显示正面）
    /// </summary>
    public bool IsBack;

    /// <summary>
    /// 花色（用于渲染正面）
    /// </summary>
    public string Huase = "";

    /// <summary>
    /// 牌大小（用于渲染正面）
    /// </summary>
    public int Size;

    /// <summary>
    /// 缓动函数类型
    /// </summary>
    public AnimationHelper.EaseType EaseType = AnimationHelper.EaseType.EaseOutQuad;

    /// <summary>
    /// 更新动画进度
    /// </summary>
    /// <param name="dt">时间增量（毫秒）</param>
    public void Update(float dt)
    {
        if (IsComplete) return;

        ElapsedMs += dt;
        float t = MathHelper.Clamp(ElapsedMs / DurationMs, 0f, 1f);
        CurrentPosition = AnimationHelper.Lerp(StartPosition, TargetPosition, t, EaseType);

        if (t >= 1f)
        {
            IsComplete = true;
            CurrentPosition = TargetPosition; // 确保最终位置精确
        }
    }

    /// <summary>
    /// 创建发牌动画
    /// </summary>
    public static CardAnimation CreateDealAnimation(int cardIndex, Vector2 startPos, Vector2 targetPos, string huase, int size, bool isBack = false)
    {
        return new CardAnimation
        {
            CardIndex = cardIndex,
            StartPosition = startPos,
            TargetPosition = targetPos,
            CurrentPosition = startPos,
            DurationMs = 300f, // 发牌动画300ms
            ElapsedMs = 0f,
            IsComplete = false,
            IsBack = isBack,
            Huase = huase,
            Size = size,
            EaseType = AnimationHelper.EaseType.EaseOutQuad
        };
    }

    /// <summary>
    /// 创建出牌动画
    /// </summary>
    public static CardAnimation CreatePlayAnimation(int cardIndex, Vector2 startPos, Vector2 targetPos, string huase, int size)
    {
        return new CardAnimation
        {
            CardIndex = cardIndex,
            StartPosition = startPos,
            TargetPosition = targetPos,
            CurrentPosition = startPos,
            DurationMs = 400f, // 出牌动画400ms
            ElapsedMs = 0f,
            IsComplete = false,
            IsBack = false, // 出牌都是正面
            Huase = huase,
            Size = size,
            EaseType = AnimationHelper.EaseType.EaseOutCubic // 更平滑的减速
        };
    }
}