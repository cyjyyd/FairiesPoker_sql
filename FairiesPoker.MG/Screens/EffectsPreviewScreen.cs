using System;
using FairiesPoker.MG.Core;
using FairiesPoker.MG.Core.Effects;
using FairiesPoker.MG.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FairiesPoker.MG.Screens;

/// <summary>A server-free place to inspect and replay the same effects used by GameScreen.</summary>
public sealed class EffectsPreviewScreen : ScreenBase
{
    private readonly PlayEffectManager _effects = new();
    private readonly UIButton[] _buttons = new UIButton[5];
    private readonly UIButton _back = new() { UseThemeTextures = false };
    private readonly UIButton _pause = new() { UseThemeTextures = false };
    private PlayEffectKind _selected = PlayEffectKind.Straight;
    private bool _paused;
    private float _replayMs;

    public EffectsPreviewScreen(Game1 game, ScreenManager screenManager) : base(game, screenManager) { }

    public override void LoadContent()
    {
        for (int i = 0; i < _buttons.Length; i++)
        {
            var kind = (PlayEffectKind)(i + 1);
            _buttons[i] = MakeButton(new Vector2(175 + i * 190, 560), new Vector2(170, 48),
                PlayEffectManager.GetTitle(kind), () => Select(kind));
        }
        _back.Position = new Vector2(1090, 40);
        _back.Size = new Vector2(130, 42);
        _back.Text = "返回";
        _back.OnClick = () => ScreenManager.Pop();
        _pause.Position = new Vector2(565, 635);
        _pause.Size = new Vector2(150, 40);
        _pause.OnClick = TogglePause;
        _pause.Text = "暂停";
        Select(_selected);
    }

    private static UIButton MakeButton(Vector2 position, Vector2 size, string text, Action onClick) => new()
    {
        Position = position, Size = size, Text = text, OnClick = onClick, UseThemeTextures = false,
        BackgroundColor = new Color(20, 53, 48, 235),
        HoverBackgroundColor = new Color(40, 90, 76, 240),
        BorderColor = new Color(169, 143, 86), TextColor = new Color(255, 225, 155)
    };

    private void Select(PlayEffectKind kind)
    {
        _selected = kind;
        _effects.Clear();
        _effects.Trigger(kind, new Vector2(640, 310));
        _replayMs = 0;
        _paused = false;
        _pause.Text = "暂停";
    }

    private void TogglePause()
    {
        _paused = !_paused;
        _pause.Text = _paused ? "继续" : "暂停";
    }

    public override void UpdatePresentation(GameTime gameTime)
    {
        if (!_paused) _effects.Update((float)gameTime.ElapsedGameTime.TotalMilliseconds);
    }

    public override void Update(GameTime gameTime)
    {
        if (_paused) return;
        _replayMs += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        if (_replayMs >= 2400) Select(_selected);
    }

    public override void HandleInput(InputManager input)
    {
        foreach (var button in _buttons) button.Update(input);
        _back.Update(input);
        _pause.Update(input);
        for (int i = 0; i < 5; i++)
            if (input.KeyPressed((Keys)((int)Keys.D1 + i))) Select((PlayEffectKind)(i + 1));
        if (input.KeyPressed(Keys.Space)) TogglePause();
    }

    public override void Draw(GameTime gameTime, SpriteBatch batch)
    {
        var pixel = UIResourceManager.WhitePixel;
        batch.Draw(pixel, DisplayManager.FullViewportVirtualBounds, new Color(9, 26, 27));
        batch.Draw(pixel, new Rectangle(110, 140, 1060, 360), new Color(16, 46, 42));
        batch.Draw(pixel, new Rectangle(110, 140, 1060, 1), new Color(144, 123, 77));
        batch.Draw(pixel, new Rectangle(110, 499, 1060, 1), new Color(144, 123, 77));
        FontManager.DrawString(batch, "出牌特效预览", new Vector2(110, 52), new Color(255, 225, 155), 1.5f);
        string subtitle = _selected switch
        {
            PlayEffectKind.Airplane => "金色仙尘 · 羽翼流光 · 粉尘拖尾",
            PlayEffectKind.ConsecutivePairs => "金色仙尘 · 成对光环 · 依次绽放",
            _ => "金色光尘 · 翡翠柔光 · 原创程序特效"
        };
        var subtitleColor = _selected is PlayEffectKind.Airplane or PlayEffectKind.ConsecutivePairs
            ? new Color(225, 195, 128) : new Color(160, 197, 182);
        FontManager.DrawString(batch, subtitle, new Vector2(110, 102), subtitleColor, 0.85f);
        // Neutral card shapes make the preview independent of third-party card art.
        for (int i = 0; i < 5; i++)
        {
            var rect = new Rectangle(510 + i * 49, 365, 63, 91);
            batch.Draw(pixel, new Rectangle(rect.X + 3, rect.Y + 5, rect.Width, rect.Height), Color.Black * 0.2f);
            batch.Draw(pixel, rect, new Color(227, 222, 198));
            batch.Draw(pixel, new Rectangle(rect.X + 5, rect.Y + 5, rect.Width - 10, rect.Height - 10), new Color(30, 69, 59));
            FontManager.DrawString(batch, (i + 3).ToString(), new Vector2(rect.X + 10, rect.Y + 8), new Color(255, 225, 155));
        }
        _effects.Draw(batch);
        for (int i = 0; i < _buttons.Length; i++)
        {
            _buttons[i].BorderColor = (PlayEffectKind)(i + 1) == _selected ? Color.Gold : new Color(100, 120, 109);
            _buttons[i].Draw(batch);
        }
        _back.Draw(batch);
        _pause.Draw(batch);
        FontManager.DrawString(batch, "点击重播 · 数字键 1–5 切换 · 空格暂停", new Vector2(375, 520), new Color(160, 197, 182), 0.85f);
    }

    public override void UnloadContent() => _effects.Dispose();
}
