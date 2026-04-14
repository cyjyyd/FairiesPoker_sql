using FairiesPoker.MG.Core;
using FairiesPoker.MG.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FairiesPoker.MG.Screens;

/// <summary>
/// 设置屏幕 - 替代Settings.cs
/// 音效开关、主题选择、全屏切换
/// </summary>
public class SettingsScreen : ScreenBase
{
    private Texture2D? _bgTexture;

    private readonly UILabel _titleLabel = new();
    private readonly UILabel[] _toggleLabels = new UILabel[4];
    private readonly UIButton[] _toggles = new UIButton[4];
    private readonly UILabel[] _toggleStatus = new UILabel[4];

    private readonly UILabel _themeLabel = new();
    private readonly UIButton _themeUp = new();
    private readonly UILabel _themeValue = new();
    private readonly UIButton _themeDown = new();

    private readonly UIButton _applyBtn = new();
    private readonly UIButton _cancelBtn = new();

    private readonly string[] _toggleNames = { "背景音乐", "游戏音效", "剧情模式", "全屏模式" };
    private bool[] _toggleValues;

    private readonly string[] _themeNames = { "奇妙仙子", "失去的宝藏", "仙境冒险", "甜蜜糖果", "精灵魔法", "绿叶森林", "奇幻蘑菇" };
    private int _selectedTheme;

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
        _toggleValues = new bool[] { ConfigManager.BackMusic, ConfigManager.SoundFX, true, ConfigManager.FullScreen };
        _selectedTheme = ConfigManager.UITheme;

        // 标题
        _titleLabel.Position = new Vector2(269, 51);
        _titleLabel.Text = "设置";
        _titleLabel.TextColor = Color.White;
        _titleLabel.Scale = 1.5f;
        _titleLabel.Size = new Vector2(100, 40);
        _titleLabel.TextAlignment = UILabel.AlignmentType.Center;

        // 开关项
        for (int i = 0; i < 4; i++)
        {
            int yPos = 110 + i * 55;

            _toggleLabels[i] = new UILabel
            {
                Position = new Vector2(79, yPos),
                Text = _toggleNames[i],
                TextColor = Color.White,
                Size = new Vector2(150, 30)
            };

            _toggleStatus[i] = new UILabel
            {
                Position = new Vector2(449, yPos),
                Text = _toggleValues[i] ? "开" : "关",
                TextColor = _toggleValues[i] ? Color.Green : Color.Red,
                Size = new Vector2(40, 30)
            };

            _toggles[i] = new UIButton
            {
                Position = new Vector2(488, yPos),
                Size = new Vector2(44, 22),
                Tag = _toggleValues[i]
            };
            int idx = i;
            _toggles[i].OnClick = () => OnToggleClick(idx);
        }

        // 主题选择器
        _themeLabel.Position = new Vector2(79, 280);
        _themeLabel.Text = "界面主题:";
        _themeLabel.TextColor = Color.White;
        _themeLabel.Size = new Vector2(120, 30);

        _themeUp.Position = new Vector2(270, 280);
        _themeUp.Size = new Vector2(30, 30);
        _themeUp.Text = "▲";
        _themeUp.TextColor = Color.White;
        _themeUp.OnClick = () => { _selectedTheme = (_selectedTheme + 5) % 7 + 1; RefreshTheme(); };

        _themeValue.Position = new Vector2(310, 280);
        _themeValue.Text = _themeNames[_selectedTheme - 1];
        _themeValue.TextColor = Color.White;
        _themeValue.Size = new Vector2(150, 30);
        _themeValue.TextAlignment = UILabel.AlignmentType.Center;

        _themeDown.Position = new Vector2(480, 280);
        _themeDown.Size = new Vector2(30, 30);
        _themeDown.Text = "▼";
        _themeDown.TextColor = Color.White;
        _themeDown.OnClick = () => { _selectedTheme = _selectedTheme % 7 + 1; RefreshTheme(); };

        // 按钮
        _applyBtn.Position = new Vector2(85, 411);
        _applyBtn.Size = new Vector2(100, 40);
        _applyBtn.Text = "应用";
        _applyBtn.TextColor = Color.White;
        _applyBtn.OnClick = OnApply;

        _cancelBtn.Position = new Vector2(275, 411);
        _cancelBtn.Size = new Vector2(100, 40);
        _cancelBtn.Text = "取消";
        _cancelBtn.TextColor = Color.White;
        _cancelBtn.OnClick = () => ScreenManager.Pop();
    }

    private void RefreshTheme()
    {
        _themeValue.Text = _themeNames[_selectedTheme - 1];
    }

    private void OnToggleClick(int index)
    {
        _toggleValues[index] = !_toggleValues[index];
        _toggles[index].Tag = _toggleValues[index];
        _toggleStatus[index].Text = _toggleValues[index] ? "开" : "关";
        _toggleStatus[index].TextColor = _toggleValues[index] ? Color.Green : Color.Red;
    }

    private void OnApply()
    {
        ConfigManager.BackMusic = _toggleValues[0];
        ConfigManager.SoundFX = _toggleValues[1];
        ConfigManager.UITheme = _selectedTheme;
        ConfigManager.FullScreen = _toggleValues[3];
        ConfigManager.Save();

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
            spriteBatch.Draw(_bgTexture, new Rectangle(0, 0, 640, 480), color);
        }
        else
        {
            spriteBatch.Draw(TextureManager.Get("_white") ?? CreateWhitePixel(),
                new Rectangle(0, 0, 640, 480), new Color(40, 45, 55, 240) * color);
        }

        _titleLabel.Draw(spriteBatch);
        for (int i = 0; i < 4; i++)
        {
            _toggleLabels[i].Draw(spriteBatch);
            _toggles[i].Draw(spriteBatch);
            _toggleStatus[i].Draw(spriteBatch);
        }
        _themeLabel.Draw(spriteBatch);
        _themeUp.Draw(spriteBatch);
        _themeValue.Draw(spriteBatch);
        _themeDown.Draw(spriteBatch);
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
        for (int i = 0; i < 4; i++) _toggles[i].Update(input);
        _themeUp.Update(input);
        _themeDown.Update(input);
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
}
