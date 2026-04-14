using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FairiesPoker.MG.Core;

/// <summary>
/// 屏幕管理器 - 管理屏幕栈(Push/Pop/Replace)
/// </summary>
public class ScreenManager
{
    private readonly Game1 _game;
    private readonly SpriteBatch _spriteBatch;
    private readonly List<ScreenBase> _screens = new();
    private readonly List<Action> _pendingActions = new();

    public ScreenManager(Game1 game, SpriteBatch spriteBatch)
    {
        _game = game;
        _spriteBatch = spriteBatch;
    }

    /// <summary>
    /// 压入新屏幕(保留当前屏幕在后台)
    /// </summary>
    public void Push(ScreenBase screen)
    {
        _pendingActions.Add(() =>
        {
            screen.Initialize();
            screen.LoadContent();
            _screens.Add(screen);
        });
    }

    /// <summary>
    /// 弹出当前屏幕(移除并返回上一个)
    /// </summary>
    public void Pop()
    {
        if (_screens.Count <= 1) return;
        _pendingActions.Add(() =>
        {
            var top = _screens[^1];
            top.UnloadContent();
            _screens.RemoveAt(_screens.Count - 1);
        });
    }

    /// <summary>
    /// 替换当前屏幕(移除旧的,添加新的)
    /// </summary>
    public void Replace(ScreenBase screen)
    {
        _pendingActions.Add(() =>
        {
            if (_screens.Count > 0)
            {
                _screens[^1].UnloadContent();
                _screens.RemoveAt(_screens.Count - 1);
            }
            screen.Initialize();
            screen.LoadContent();
            _screens.Add(screen);
        });
    }

    /// <summary>
    /// 清空所有屏幕
    /// </summary>
    public void Clear()
    {
        _pendingActions.Add(() =>
        {
            foreach (var s in _screens)
                s.UnloadContent();
            _screens.Clear();
        });
    }

    /// <summary>
    /// 获取最顶层屏幕
    /// </summary>
    public ScreenBase? TopScreen => _screens.Count > 0 ? _screens[^1] : null;

    public void Update(GameTime gameTime, InputManager input)
    {
        // 执行待处理操作
        foreach (var action in _pendingActions)
            action();
        _pendingActions.Clear();

        // 只更新最顶层屏幕
        if (_screens.Count > 0)
        {
            _screens[^1].Update(gameTime);
            _screens[^1].HandleInput(input);
        }
    }

    public void Draw(GameTime gameTime)
    {
        // 从底到顶渲染所有可见屏幕
        foreach (var screen in _screens)
        {
            if (screen.Opacity <= 0) continue;

            _spriteBatch.Begin(SpriteSortMode.Deferred,
                new BlendState { AlphaSourceBlend = Blend.SourceAlpha, ColorSourceBlend = Blend.One, AlphaDestinationBlend = Blend.InverseSourceAlpha, ColorDestinationBlend = Blend.InverseSourceAlpha });
            screen.Draw(gameTime, _spriteBatch);
            _spriteBatch.End();
        }
    }
}
