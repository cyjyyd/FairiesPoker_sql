using System;
using FairiesPoker.MG.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FairiesPoker.MG.UI;
using Microsoft.Xna.Framework.Input;

namespace FairiesPoker.MG.Screens;

/// <summary>
/// 主菜单屏幕 - 替代Main.cs
/// 单人模式/多人模式/设置/退出
/// </summary>
public class MainMenuScreen : ScreenBase
{
    private Texture2D? _bgTexture;
    private Texture2D? _logoTexture;

    private readonly UILabel _singlePlayer = new();
    private readonly UILabel _multiPlayer = new();
    private readonly UILabel _settingsLabel = new();
    private readonly UILabel _quitLabel = new();

    private Point _mousePos;
    private bool _isDragging;
    private Vector2 _dragOffset;

    public MainMenuScreen(Game1 game, ScreenManager screenManager)
        : base(game, screenManager)
    {
    }

    public override void LoadContent()
    {
        // 背景图 (从主题文件夹加载)
        string bgPath = System.IO.Path.Combine(ConfigManager.ThemePath, "main seq.jpg");
        _bgTexture = TextureManager.Load("_main_bg", bgPath);

        // Logo
        string logoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "FP.png");
        _logoTexture = TextureManager.Load("_logo", logoPath);

        // 初始化菜单文字 (位置对应原Main.cs)
        _singlePlayer.Position = new Vector2(161, 335);
        _singlePlayer.Size = new Vector2(387, 106);
        _singlePlayer.Text = "单人模式\nSingle Player";
        _singlePlayer.TextColor = Color.OrangeRed;
        _singlePlayer.TextAlignment = UILabel.AlignmentType.Center;
        _singlePlayer.Scale = 1.3f;

        _multiPlayer.Position = new Vector2(793, 335);
        _multiPlayer.Size = new Vector2(387, 106);
        _multiPlayer.Text = "多人模式\nMulti Player";
        _multiPlayer.TextColor = Color.OrangeRed;
        _multiPlayer.TextAlignment = UILabel.AlignmentType.Center;
        _multiPlayer.Scale = 1.3f;

        _settingsLabel.Position = new Vector2(288, 500);
        _settingsLabel.Size = new Vector2(247, 106);
        _settingsLabel.Text = "设置\nSettings";
        _settingsLabel.TextColor = Color.OrangeRed;
        _settingsLabel.TextAlignment = UILabel.AlignmentType.Center;
        _settingsLabel.Scale = 1.2f;

        _quitLabel.Position = new Vector2(793, 500);
        _quitLabel.Size = new Vector2(219, 106);
        _quitLabel.Text = "退出\nQuit";
        _quitLabel.TextColor = Color.OrangeRed;
        _quitLabel.TextAlignment = UILabel.AlignmentType.Center;
        _quitLabel.Scale = 1.2f;
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
            spriteBatch.Draw(_bgTexture, new Rectangle(0, 0, 1280, 720), color);
        }
        else
        {
            // 回退: 绘制深色背景 + 菜单区域高亮
            var white = TextureManager.Get("_white");
            if (white != null)
            {
                // 深绿色背景(扑克桌风格)
                spriteBatch.Draw(white, new Rectangle(0, 0, 1280, 720),
                    new Color(20, 60, 20) * Opacity);
                // 菜单选项区域半透明背景
                spriteBatch.Draw(white, new Rectangle(161, 335, 387, 106),
                    new Color(40, 80, 40, 120) * Opacity);
                spriteBatch.Draw(white, new Rectangle(793, 335, 387, 106),
                    new Color(40, 80, 40, 120) * Opacity);
                spriteBatch.Draw(white, new Rectangle(288, 500, 247, 106),
                    new Color(40, 80, 40, 120) * Opacity);
                spriteBatch.Draw(white, new Rectangle(793, 500, 219, 106),
                    new Color(40, 80, 40, 120) * Opacity);
            }
        }

        // Logo
        if (_logoTexture != null)
        {
            spriteBatch.Draw(_logoTexture, new Rectangle(157, 90, 968, 195), color);
        }

        // 菜单选项
        _singlePlayer.Draw(spriteBatch);
        _multiPlayer.Draw(spriteBatch);
        _settingsLabel.Draw(spriteBatch);
        _quitLabel.Draw(spriteBatch);
    }

    public override void HandleInput(InputManager input)
    {
        _mousePos = new Point((int)input.MousePosition.X, (int)input.MousePosition.Y);

        // 窗口拖拽 (与原Main.cs MouseDown/MouseMove逻辑一致)
        if (input.LeftMouseClicked && _mousePos.Y < 90)
        {
            _isDragging = true;
            _dragOffset = input.MousePosition;
        }
        if (_isDragging && input.LeftMouseHeld)
        {
            var delta = input.MousePosition - _dragOffset;
            // TODO: 拖拽窗口位置
        }
        if (input.LeftMouseReleased)
            _isDragging = false;

        // 悬停变色
        bool spHover = _singlePlayer.Bounds.Contains(_mousePos);
        bool mpHover = _multiPlayer.Bounds.Contains(_mousePos);
        bool sHover = _settingsLabel.Bounds.Contains(_mousePos);
        bool qHover = _quitLabel.Bounds.Contains(_mousePos);

        _singlePlayer.TextColor = spHover ? new Color(100, 149, 237) : Color.OrangeRed;
        _multiPlayer.TextColor = mpHover ? new Color(100, 149, 237) : Color.OrangeRed;
        _settingsLabel.TextColor = sHover ? new Color(100, 149, 237) : Color.OrangeRed;
        _quitLabel.TextColor = qHover ? new Color(100, 149, 237) : Color.OrangeRed;

        // 点击事件
        if (input.LeftMouseReleased)
        {
            if (spHover)
                OnSinglePlayer();
            else if (mpHover)
                OnMultiPlayer();
            else if (sHover)
                OnSettings();
            else if (qHover)
                OnQuit();
        }
    }

    private void OnSinglePlayer()
    {
        Game.AudioManager?.StopBgm();
        // 进入单机游戏
        ScreenManager.Push(new GameScreen(Game, ScreenManager, false));
    }

    private void OnMultiPlayer()
    {
        // 进入登录界面
        ScreenManager.Push(new LoginScreen(Game, ScreenManager));
    }

    private void OnSettings()
    {
        ScreenManager.Push(new SettingsScreen(Game, ScreenManager));
    }

    private void OnQuit()
    {
        Game.Exit();
    }
}
