using FairiesPoker.MG.Core;
using FairiesPoker.MG.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace FairiesPoker.MG.Screens;

/// <summary>
/// 设置屏幕 - 替代Settings.cs
/// 音效开关、主题选择、全屏切换
/// </summary>
public class SettingsScreen : ScreenBase
{
    private const int WindowWidth = 640;
    private const int WindowHeight = 480;
    private static readonly Vector2 WindowOrigin = new(
        (DisplayManager.DesignWidth - WindowWidth) / 2f,
        (DisplayManager.DesignHeight - WindowHeight) / 2f);

    private Texture2D? _bgTexture;

    private readonly UILabel _titleLabel = new();
    private readonly UILabel[] _toggleLabels = new UILabel[5];
    private readonly UIButton[] _toggles = new UIButton[5];
    private readonly UILabel[] _toggleStatus = new UILabel[5];

    private readonly UILabel _themeLabel = new();
    private readonly UIButton _themeUp = new();
    private readonly UILabel _themeValue = new();
    private readonly UIButton _themeDown = new();

    private readonly UILabel _resolutionLabel = new();
    private readonly UIButton _resolutionUp = new();
    private readonly UILabel _resolutionValue = new();
    private readonly UIButton _resolutionDown = new();

    private readonly UIButton _applyBtn = new();
    private readonly UIButton _cancelBtn = new();

    private readonly string[] _toggleNames = { "背景音乐", "游戏音效", "剧情模式", "无边框窗口", "全屏模式" };
    private bool[] _toggleValues;

    private readonly string[] _themeNames = { "奇妙仙子", "失去的宝藏", "仙境冒险", "甜蜜糖果", "精灵魔法", "绿叶森林", "奇幻蘑菇" };
    private int _selectedTheme;
    private readonly List<ResolutionOption> _resolutionOptions = new();
    private int _selectedResolutionIndex;

    private Point _mousePos;
    private bool _isDragging;

    public SettingsScreen(Game1 game, ScreenManager screenManager)
        : base(game, screenManager)
    {
    }

    public override void Initialize()
    {
        base.Initialize();

        // 读取当前设置
        _toggleValues = new bool[] { ConfigManager.BackMusic, ConfigManager.SoundFX, true, ConfigManager.BorderlessWindow, ConfigManager.FullScreen };
        _selectedTheme = ConfigManager.UITheme;
        BuildResolutionOptions();

        // 标题
        _titleLabel.Position = WindowPosition(269, 40);
        _titleLabel.Text = "设置";
        _titleLabel.TextColor = Color.White;
        _titleLabel.Scale = 1.5f;
        _titleLabel.Size = new Vector2(100, 40);
        _titleLabel.TextAlignment = UILabel.AlignmentType.Center;

        // 开关项
        for (int i = 0; i < _toggles.Length; i++)
        {
            int yPos = 92 + i * 40;

            _toggleLabels[i] = new UILabel
            {
                Position = WindowPosition(79, yPos),
                Text = _toggleNames[i],
                TextColor = Color.White,
                Size = new Vector2(150, 30)
            };

            _toggleStatus[i] = new UILabel
            {
                Position = WindowPosition(449, yPos),
                Text = _toggleValues[i] ? "开" : "关",
                TextColor = _toggleValues[i] ? Color.Green : Color.Red,
                Size = new Vector2(40, 30)
            };

            _toggles[i] = new UIButton
            {
                Position = WindowPosition(488, yPos),
                Size = new Vector2(44, 22),
                Text = _toggleValues[i] ? "开" : "关",
                Tag = _toggleValues[i]
            };
            int idx = i;
            _toggles[i].OnClick = () => OnToggleClick(idx);
        }

        // 主题选择器
        _themeLabel.Position = WindowPosition(79, 304);
        _themeLabel.Text = "界面主题:";
        _themeLabel.TextColor = Color.White;
        _themeLabel.Size = new Vector2(120, 30);

        _themeUp.Position = WindowPosition(270, 304);
        _themeUp.Size = new Vector2(30, 30);
        _themeUp.Text = "▲";
        _themeUp.TextColor = Color.White;
        _themeUp.OnClick = () => { _selectedTheme = (_selectedTheme + 5) % 7 + 1; RefreshTheme(); };

        _themeValue.Position = WindowPosition(310, 304);
        _themeValue.Text = _themeNames[_selectedTheme - 1];
        _themeValue.TextColor = Color.White;
        _themeValue.Size = new Vector2(150, 30);
        _themeValue.TextAlignment = UILabel.AlignmentType.Center;

        _themeDown.Position = WindowPosition(480, 304);
        _themeDown.Size = new Vector2(30, 30);
        _themeDown.Text = "▼";
        _themeDown.TextColor = Color.White;
        _themeDown.OnClick = () => { _selectedTheme = _selectedTheme % 7 + 1; RefreshTheme(); };

        // 分辨率选择器
        _resolutionLabel.Position = WindowPosition(79, 349);
        _resolutionLabel.Text = "窗口分辨率:";
        _resolutionLabel.TextColor = Color.White;
        _resolutionLabel.Size = new Vector2(120, 30);

        _resolutionUp.Position = WindowPosition(270, 349);
        _resolutionUp.Size = new Vector2(30, 30);
        _resolutionUp.Text = "▲";
        _resolutionUp.TextColor = Color.White;
        _resolutionUp.OnClick = () => { SelectPreviousResolution(); };

        _resolutionValue.Position = WindowPosition(310, 349);
        _resolutionValue.TextColor = Color.White;
        _resolutionValue.Size = new Vector2(170, 30);
        _resolutionValue.TextAlignment = UILabel.AlignmentType.Center;

        _resolutionDown.Position = WindowPosition(500, 349);
        _resolutionDown.Size = new Vector2(30, 30);
        _resolutionDown.Text = "▼";
        _resolutionDown.TextColor = Color.White;
        _resolutionDown.OnClick = () => { SelectNextResolution(); };
        RefreshResolution();

        // 按钮
        _applyBtn.Position = WindowPosition(85, 415);
        _applyBtn.Size = new Vector2(100, 40);
        _applyBtn.Text = "应用";
        _applyBtn.TextColor = Color.White;
        _applyBtn.OnClick = OnApply;

        _cancelBtn.Position = WindowPosition(275, 415);
        _cancelBtn.Size = new Vector2(100, 40);
        _cancelBtn.Text = "取消";
        _cancelBtn.TextColor = Color.White;
        _cancelBtn.OnClick = () => ScreenManager.Pop();
    }

    private void RefreshTheme()
    {
        _themeValue.Text = _themeNames[_selectedTheme - 1];
    }

    private void BuildResolutionOptions()
    {
        _resolutionOptions.Clear();

        var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        AddResolutionOption(displayMode.Width, displayMode.Height, $"当前 {displayMode.Width} x {displayMode.Height}");

        foreach (var preset in ConfigManager.ResolutionPresets)
        {
            AddResolutionOption(preset.Width, preset.Height, $"{preset.Width} x {preset.Height}");
        }

        _selectedResolutionIndex = _resolutionOptions.FindIndex(r =>
            r.Width == ConfigManager.WindowWidth && r.Height == ConfigManager.WindowHeight);

        if (_selectedResolutionIndex < 0)
        {
            _resolutionOptions.Add(new ResolutionOption(
                ConfigManager.WindowWidth,
                ConfigManager.WindowHeight,
                $"{ConfigManager.WindowWidth} x {ConfigManager.WindowHeight}"));
            _selectedResolutionIndex = _resolutionOptions.Count - 1;
        }
    }

    private void AddResolutionOption(int width, int height, string label)
    {
        if (_resolutionOptions.Exists(r => r.Width == width && r.Height == height))
            return;

        _resolutionOptions.Add(new ResolutionOption(width, height, label));
    }

    private void SelectPreviousResolution()
    {
        if (_resolutionOptions.Count == 0) return;
        _selectedResolutionIndex = (_selectedResolutionIndex + _resolutionOptions.Count - 1) % _resolutionOptions.Count;
        RefreshResolution();
    }

    private void SelectNextResolution()
    {
        if (_resolutionOptions.Count == 0) return;
        _selectedResolutionIndex = (_selectedResolutionIndex + 1) % _resolutionOptions.Count;
        RefreshResolution();
    }

    private void RefreshResolution()
    {
        if (_resolutionOptions.Count == 0)
        {
            _resolutionValue.Text = "";
            return;
        }

        _resolutionValue.Text = _resolutionOptions[_selectedResolutionIndex].Label;
    }

    private void OnToggleClick(int index)
    {
        _toggleValues[index] = !_toggleValues[index];

        if (index == 3 && _toggleValues[3])
        {
            _toggleValues[4] = false;
        }
        else if (index == 4 && _toggleValues[4])
        {
            _toggleValues[3] = false;
        }

        RefreshToggleStatus();
    }

    private void RefreshToggleStatus()
    {
        for (int i = 0; i < _toggles.Length; i++)
        {
            _toggles[i].Tag = _toggleValues[i];
            _toggles[i].Text = _toggleValues[i] ? "开" : "关";
            _toggleStatus[i].Text = _toggleValues[i] ? "开" : "关";
            _toggleStatus[i].TextColor = _toggleValues[i] ? Color.Green : Color.Red;
        }
    }

    private void OnApply()
    {
        ConfigManager.BackMusic = _toggleValues[0];
        ConfigManager.SoundFX = _toggleValues[1];
        ConfigManager.UITheme = _selectedTheme;
        ConfigManager.BorderlessWindow = _toggleValues[3];
        ConfigManager.FullScreen = _toggleValues[4];

        if (_resolutionOptions.Count > 0)
        {
            var resolution = _resolutionOptions[_selectedResolutionIndex];
            ConfigManager.WindowWidth = resolution.Width;
            ConfigManager.WindowHeight = resolution.Height;
        }

        ConfigManager.Save();
        UIResourceManager.SetTheme(ConfigManager.UITheme);
        Game.ApplyDisplaySettings();

        // 应用音频设置
        if (Game.AudioManager != null)
        {
            Game.AudioManager.BackMusicEnabled = ConfigManager.BackMusic;
            Game.AudioManager.SoundFXEnabled = ConfigManager.SoundFX;
        }
    }

    public override void Update(GameTime gameTime)
    {
        FadeIn(0.03);
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var color = Color.White * Opacity;

        // 背景
        if (_bgTexture != null)
        {
            spriteBatch.Draw(_bgTexture, new Rectangle((int)WindowOrigin.X, (int)WindowOrigin.Y, WindowWidth, WindowHeight), color);
        }
        else
        {
            spriteBatch.Draw(TextureManager.Get("_white") ?? CreateWhitePixel(),
                new Rectangle((int)WindowOrigin.X, (int)WindowOrigin.Y, WindowWidth, WindowHeight), new Color(40, 45, 55, 240) * color);
        }

        _titleLabel.Draw(spriteBatch);
        for (int i = 0; i < _toggles.Length; i++)
        {
            _toggleLabels[i].Draw(spriteBatch);
            _toggles[i].Draw(spriteBatch);
            _toggleStatus[i].Draw(spriteBatch);
        }
        _themeLabel.Draw(spriteBatch);
        _themeUp.Draw(spriteBatch);
        _themeValue.Draw(spriteBatch);
        _themeDown.Draw(spriteBatch);
        _resolutionLabel.Draw(spriteBatch);
        _resolutionUp.Draw(spriteBatch);
        _resolutionValue.Draw(spriteBatch);
        _resolutionDown.Draw(spriteBatch);
        _applyBtn.Draw(spriteBatch);
        _cancelBtn.Draw(spriteBatch);
    }

    public override void HandleInput(InputManager input)
    {
        _mousePos = new Point((int)input.MousePosition.X, (int)input.MousePosition.Y);

        // 窗口拖拽
        if (input.LeftMouseClicked && _mousePos.Y < 50)
        {
            _isDragging = true;
        }
        if (input.LeftMouseReleased) _isDragging = false;

        // 更新所有控件
        for (int i = 0; i < _toggles.Length; i++) _toggles[i].Update(input);
        _themeUp.Update(input);
        _themeDown.Update(input);
        _resolutionUp.Update(input);
        _resolutionDown.Update(input);
        _applyBtn.Update(input);
        _cancelBtn.Update(input);

        if (input.KeyPressed(Keys.Escape))
            ScreenManager.Pop();
    }

    private static Texture2D? _whitePixel;
    private static Texture2D CreateWhitePixel()
    {
        if (_whitePixel != null) return _whitePixel;
        _whitePixel = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { Color.White });
        TextureManager.Load("_white", "");
        return _whitePixel;
    }

    private static Vector2 WindowPosition(float x, float y)
    {
        return WindowOrigin + new Vector2(x, y);
    }

    private readonly struct ResolutionOption
    {
        public int Width { get; }
        public int Height { get; }
        public string Label { get; }

        public ResolutionOption(int width, int height, string label)
        {
            Width = width;
            Height = height;
            Label = label;
        }
    }
}
