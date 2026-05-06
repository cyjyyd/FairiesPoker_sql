using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FairiesPoker.MG.Core;

/// <summary>
/// 动画管理器 - 管理所有卡牌动画的队列和并发执行
/// </summary>
public class AnimationManager
{
    /// <summary>
    /// 正在活动的动画列表
    /// </summary>
    private readonly List<CardAnimation> _activeAnimations = new();

    /// <summary>
    /// 待启动的动画队列（用于发牌动画间隔启动）
    /// </summary>
    private readonly Queue<CardAnimation> _pendingAnimations = new();

    /// <summary>
    /// 发牌动画配置：单张牌飞牌时长（毫秒）
    /// </summary>
    public float DealDurationMs = 300f;

    /// <summary>
    /// 发牌动画配置：牌间隔时间（毫秒），与原项目 Thread.Sleep(80) 一致
    /// </summary>
    public float DealIntervalMs = 80f;

    /// <summary>
    /// 出牌动画配置：出牌飞牌时长（毫秒）
    /// </summary>
    public float PlayDurationMs = 400f;

    /// <summary>
    /// 发牌间隔累积器
    /// </summary>
    private float _dealIntervalAccumulator;

    /// <summary>
    /// 是否有动画正在进行或待启动
    /// </summary>
    public bool IsAnimating => _activeAnimations.Count > 0 || _pendingAnimations.Count > 0;

    /// <summary>
    /// 添加发牌动画到队列（将按间隔依次启动）
    /// </summary>
    public void QueueDealAnimation(CardAnimation animation)
    {
        animation.DurationMs = DealDurationMs;
        animation.EaseType = AnimationHelper.EaseType.EaseOutQuad;
        _pendingAnimations.Enqueue(animation);
    }

    /// <summary>
    /// 立即开始出牌动画
    /// </summary>
    public void StartPlayAnimation(CardAnimation animation)
    {
        animation.DurationMs = PlayDurationMs;
        animation.EaseType = AnimationHelper.EaseType.EaseOutCubic;
        _activeAnimations.Add(animation);
    }

    /// <summary>
    /// 更新所有动画
    /// </summary>
    /// <param name="dt">时间增量（毫秒）</param>
    public void Update(float dt)
    {
        // 处理待启动的发牌动画队列
        if (_pendingAnimations.Count > 0)
        {
            _dealIntervalAccumulator += dt;
            while (_dealIntervalAccumulator >= DealIntervalMs && _pendingAnimations.Count > 0)
            {
                _dealIntervalAccumulator -= DealIntervalMs;
                var anim = _pendingAnimations.Dequeue();
                anim.ElapsedMs = 0; // 重置开始时间
                _activeAnimations.Add(anim);
            }
        }

        // 更新所有活动动画
        for (int i = _activeAnimations.Count - 1; i >= 0; i--)
        {
            _activeAnimations[i].Update(dt);
            if (_activeAnimations[i].IsComplete)
            {
                _activeAnimations.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 获取所有活动动画（用于渲染）
    /// </summary>
    public IReadOnlyList<CardAnimation> GetActiveAnimations() => _activeAnimations;

    /// <summary>
    /// 清空所有动画（活动和待启动）
    /// </summary>
    public void Clear()
    {
        _activeAnimations.Clear();
        _pendingAnimations.Clear();
        _dealIntervalAccumulator = 0;
    }

    /// <summary>
    /// 获取活动的动画数量
    /// </summary>
    public int ActiveCount => _activeAnimations.Count;

    /// <summary>
    /// 获取待启动的动画数量
    /// </summary>
    public int PendingCount => _pendingAnimations.Count;

    /// <summary>
    /// 获取总动画数量（活动 + 待启动）
    /// </summary>
    public int TotalCount => _activeAnimations.Count + _pendingAnimations.Count;
}