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

    private readonly UILabel _titleLabel = new();
    private readonly UILabel[] _toggleLabels = new UILabel[4];
    private readonly UIButton[] _toggles = new UIButton[4];
    private readonly UILabel[] _toggleStatus = new UILabel[4];

    private readonly UILabel _bgmVolumeLabel = new();
    private readonly UIButton _bgmVolumeDown = new();
    private readonly UILabel _bgmVolumeValue = new();
    private readonly UIButton _bgmVolumeUp = new();
    private readonly UILabel _sfxVolumeLabel = new();
    private readonly UIButton _sfxVolumeDown = new();
    private readonly UILabel _sfxVolumeValue = new();
    private readonly UIButton _sfxVolumeUp = new();

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

    private readonly string[] _toggleNames = { "背景音乐", "游戏音效", "无边框窗口", "全屏模式" };
    private bool[] _toggleValues;
    private float _selectedBgmVolume;
    private float _selectedSfxVolume;

    private readonly string[] _themeNames = { "奇妙仙子", "失落的宝藏", "拯救精灵大作战", "羽翼之谜", "海盗仙子", "永无兽传奇" };
    private int _selectedTheme;
    private readonly List<ResolutionOption> _resolutionOptions = new();
    private int _selectedResolutionIndex;

    private Point _mousePos;
    public SettingsScreen(Game1 game, ScreenManager screenManager)
        : base(game, screenManager)
    {
    }

    public override void Initialize()
    {
        base.Initialize();

        // 读取当前设置
        _toggleValues = new bool[] { ConfigManager.BackMusic, ConfigManager.SoundFX, ConfigManager.BorderlessWindow, ConfigManager.FullScreen };
        _selectedBgmVolume = ConfigManager.BackMusicVolume;
        _selectedSfxVolume = ConfigManager.SoundFXVolume;
        _selectedTheme = ConfigManager.UITheme;
        if (_selectedTheme < 1 || _selectedTheme > ConfigManager.ThemeCount)
            _selectedTheme = 5;
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
            int yPos = 82 + i * 40;

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

        InitVolumeControl(_bgmVolumeLabel, _bgmVolumeDown, _bgmVolumeValue, _bgmVolumeUp,
            "音乐音量:", 242, () => ChangeBgmVolume(-0.1f), () => ChangeBgmVolume(0.1f));
        InitVolumeControl(_sfxVolumeLabel, _sfxVolumeDown, _sfxVolumeValue, _sfxVolumeUp,
            "音效音量:", 282, () => ChangeSfxVolume(-0.1f), () => ChangeSfxVolume(0.1f));
        RefreshVolumeValues();

        // 主题选择器
        _themeLabel.Position = WindowPosition(79, 322);
        _themeLabel.Text = "界面主题:";
        _themeLabel.TextColor = Color.White;
        _themeLabel.Size = new Vector2(120, 30);

        _themeUp.Position = WindowPosition(270, 322);
        _themeUp.Size = new Vector2(30, 30);
        _themeUp.Text = "▲";
        _themeUp.TextColor = Color.White;
        _themeUp.OnClick = () => { _selectedTheme = _selectedTheme <= 1 ? ConfigManager.ThemeCount : _selectedTheme - 1; RefreshTheme(); };

        _themeValue.Position = WindowPosition(310, 322);
        _themeValue.Text = _themeNames[_selectedTheme - 1];
        _themeValue.TextColor = Color.White;
        _themeValue.Size = new Vector2(170, 30);
        _themeValue.TextAlignment = UILabel.AlignmentType.Center;

        _themeDown.Position = WindowPosition(500, 322);
        _themeDown.Size = new Vector2(30, 30);
        _themeDown.Text = "▼";
        _themeDown.TextColor = Color.White;
        _themeDown.OnClick = () => { _selectedTheme = _selectedTheme >= ConfigManager.ThemeCount ? 1 : _selectedTheme + 1; RefreshTheme(); };

        // 分辨率选择器
        _resolutionLabel.Position = WindowPosition(79, 362);
        _resolutionLabel.Text = "窗口分辨率:";
        _resolutionLabel.TextColor = Color.White;
        _resolutionLabel.Size = new Vector2(120, 30);

        _resolutionUp.Position = WindowPosition(270, 362);
        _resolutionUp.Size = new Vector2(30, 30);
        _resolutionUp.Text = "▲";
        _resolutionUp.TextColor = Color.White;
        _resolutionUp.OnClick = () => { SelectPreviousResolution(); };

        _resolutionValue.Position = WindowPosition(310, 362);
        _resolutionValue.TextColor = Color.White;
        _resolutionValue.Size = new Vector2(170, 30);
        _resolutionValue.TextAlignment = UILabel.AlignmentType.Center;

        _resolutionDown.Position = WindowPosition(500, 362);
        _resolutionDown.Size = new Vector2(30, 30);
        _resolutionDown.Text = "▼";
        _resolutionDown.TextColor = Color.White;
        _resolutionDown.OnClick = () => { SelectNextResolution(); };
        RefreshResolution();

        // 按钮
        _applyBtn.Position = WindowPosition(85, 420);
        _applyBtn.Size = new Vector2(100, 40);
        _applyBtn.Text = "应用";
        _applyBtn.TextColor = Color.White;
        _applyBtn.OnClick = OnApply;

        _cancelBtn.Position = WindowPosition(275, 420);
        _cancelBtn.Size = new Vector2(100, 40);
        _cancelBtn.Text = "取消";
        _cancelBtn.TextColor = Color.White;
        _cancelBtn.OnClick = () => ScreenManager.Pop();
    }

    private void RefreshTheme()
    {
        _themeValue.Text = _themeNames[_selectedTheme - 1];
    }

    private void InitVolumeControl(
        UILabel label,
        UIButton down,
        UILabel value,
        UIButton up,
        string text,
        int y,
        System.Action decrease,
        System.Action increase)
    {
        label.Position = WindowPosition(79, y);
        label.Text = text;
        label.TextColor = Color.White;
        label.Size = new Vector2(120, 30);

        down.Position = WindowPosition(270, y);
        down.Size = new Vector2(30, 30);
        down.Text = "－";
        down.TextColor = Color.White;
        down.OnClick = decrease;

        value.Position = WindowPosition(310, y);
        value.TextColor = Color.White;
        value.Size = new Vector2(170, 30);
        value.TextAlignment = UILabel.AlignmentType.Center;

        up.Position = WindowPosition(500, y);
        up.Size = new Vector2(30, 30);
        up.Text = "＋";
        up.TextColor = Color.White;
        up.OnClick = increase;
    }

    private void ChangeBgmVolume(float delta)
    {
        _selectedBgmVolume = Clamp01(_selectedBgmVolume + delta);
        RefreshVolumeValues();
        if (Game.AudioManager != null)
            Game.AudioManager.BgmVolume = _selectedBgmVolume;
    }

    private void ChangeSfxVolume(float delta)
    {
        _selectedSfxVolume = Clamp01(_selectedSfxVolume + delta);
        RefreshVolumeValues();
        if (Game.AudioManager != null)
            Game.AudioManager.SfxVolume = _selectedSfxVolume;
    }

    private void RefreshVolumeValues()
    {
        _bgmVolumeValue.Text = VolumeText(_selectedBgmVolume);
        _sfxVolumeValue.Text = VolumeText(_selectedSfxVolume);
    }

    private static string VolumeText(float value)
    {
        return $"{(int)(Clamp01(value) * 100f + 0.5f)}%";
    }

    private static float Clamp01(float value)
    {
        if (value < 0f) return 0f;
        if (value > 1f) return 1f;
        return value;
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

        if (index == 2 && _toggleValues[2])
        {
            _toggleValues[3] = false;
        }
        else if (index == 3 && _toggleValues[3])
        {
            _toggleValues[2] = false;
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
        ConfigManager.BackMusicVolume = _selectedBgmVolume;
        ConfigManager.SoundFXVolume = _selectedSfxVolume;
        ConfigManager.UITheme = _selectedTheme;
        ConfigManager.BorderlessWindow = _toggleValues[2];
        ConfigManager.FullScreen = _toggleValues[3];

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
            Game.AudioManager.ApplySettings(
                ConfigManager.BackMusic,
                ConfigManager.SoundFX,
                ConfigManager.BackMusicVolume,
                ConfigManager.SoundFXVolume);
            Game.AudioManager.PlayThemeBgm();
        }
    }

    public override void Update(GameTime gameTime)
    {
        FadeIn(0.03);
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        // 背景
        spriteBatch.Draw(TextureManager.Get("_white") ?? CreateWhitePixel(),
            new Rectangle((int)WindowOrigin.X, (int)WindowOrigin.Y, WindowWidth, WindowHeight), new Color(40, 45, 55, 240) * Opacity);

        _titleLabel.Draw(spriteBatch);
        for (int i = 0; i < _toggles.Length; i++)
        {
            _toggleLabels[i].Draw(spriteBatch);
            _toggles[i].Draw(spriteBatch);
            _toggleStatus[i].Draw(spriteBatch);
        }
        _bgmVolumeLabel.Draw(spriteBatch);
        _bgmVolumeDown.Draw(spriteBatch);
        _bgmVolumeValue.Draw(spriteBatch);
        _bgmVolumeUp.Draw(spriteBatch);
        _sfxVolumeLabel.Draw(spriteBatch);
        _sfxVolumeDown.Draw(spriteBatch);
        _sfxVolumeValue.Draw(spriteBatch);
        _sfxVolumeUp.Draw(spriteBatch);
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

        // 更新所有控件
        for (int i = 0; i < _toggles.Length; i++) _toggles[i].Update(input);
        _bgmVolumeDown.Update(input);
        _bgmVolumeUp.Update(input);
        _sfxVolumeDown.Update(input);
        _sfxVolumeUp.Update(input);
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
