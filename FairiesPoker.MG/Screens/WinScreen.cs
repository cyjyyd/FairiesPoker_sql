using FairiesPoker.MG.Core;
using FairiesPoker.MG.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace FairiesPoker.MG.Screens;

/// <summary>
/// 胜负画面 - 替代win.cs
/// 显示比赛结果、玩家名、胜负状态，点击返回
/// </summary>
public class WinScreen : ScreenBase
{
    private Texture2D? _resultTexture;

    private readonly UILabel[] _nameLabels = new UILabel[3];
    private readonly UILabel[] _resultLabels = new UILabel[3];
    private readonly UIButton _closeBtn = new();

    private readonly string[] _playerNames = new string[3];
    private readonly bool[] _results = new bool[3]; // true=胜利, false=失败

    private Point _mousePos;
    private bool _isDragging;

    // 动画
    private float _scaleAnim;
    private float _fadeTimer;

    public WinScreen(Game1 game, ScreenManager screenManager, bool[] results, string[] names)
        : base(game, screenManager)
    {
        Array.Copy(results, _results, 3);
        Array.Copy(names, _playerNames, 3);
    }

    public override void Initialize()
    {
        base.Initialize();
        Opacity = 0f;
        _scaleAnim = 0.5f;

        // 加载结果背景图
        string themeSuffix = GetThemeSuffix();
        string bgName = GetResultImageName();
        string bgPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Results", themeSuffix, bgName);
        if (File.Exists(bgPath))
            _resultTexture = TextureManager.Load("_result", bgPath);

        // 玩家名标签 (左/自己/右)
        int[] xPos = { 60, 580, 1100 };
        int[] yPos = { 200, 200, 200 };
        for (int i = 0; i < 3; i++)
        {
            _nameLabels[i] = new UILabel
            {
                Position = new Vector2(xPos[i], yPos[i]),
                Text = _playerNames[i],
                TextColor = Color.White,
                Size = new Vector2(150, 40),
                TextAlignment = UILabel.AlignmentType.Center,
                Scale = 1.2f
            };

            _resultLabels[i] = new UILabel
            {
                Position = new Vector2(xPos[i], yPos[i] + 50),
                Text = _results[i] ? "胜利" : "失败",
                TextColor = _results[i] ? Color.GreenYellow : Color.Red,
                Size = new Vector2(150, 40),
                TextAlignment = UILabel.AlignmentType.Center,
                Scale = 1.5f
            };
        }

        // 关闭按钮
        _closeBtn.Position = new Vector2(590, 500);
        _closeBtn.Size = new Vector2(120, 50);
        _closeBtn.Text = "返回";
        _closeBtn.TextColor = Color.White;
        _closeBtn.OnClick = () => ScreenManager.Pop();
    }

    private string GetThemeSuffix()
    {
        return ConfigManager.UITheme switch
        {
            1 => "TB", 2 => "LT", 3 => "FR", 4 => "SW",
            5 => "PF", 6 => "LN", 7 => "PG",
            _ => "PF"
        };
    }

    private string GetResultImageName()
    {
        // 自己(索引1)是胜利还是失败
        if (_results[1])
        {
            // 胜利: 如果自己是唯一地主胜利则win_dz, 否则win_nm
            bool othersWin = _results[0] || _results[2];
            return othersWin ? "win_nm.png" : "win_dz.png";
        }
        else
        {
            // 失败: 如果其他人都胜利则lose_dz, 否则lose_nm
            bool othersWin = _results[0] && _results[2];
            return othersWin ? "lose_dz.png" : "lose_nm.png";
        }
    }

    public override void Update(GameTime gameTime)
    {
        // 淡入动画
        if (_fadeTimer < 1f)
            _fadeTimer = Math.Min(1f, _fadeTimer + 0.02f);
        Opacity = _fadeTimer;

        // 缩放动画
        if (_scaleAnim < 1f)
            _scaleAnim = Math.Min(1f, _scaleAnim + 0.05f);
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var color = Color.White * Opacity;

        // 半透明背景
        spriteBatch.Draw(TextureManager.Get("_white") ?? CreateWhitePixel(),
            new Rectangle(0, 0, 1280, 720), new Color(0, 0, 0, 180) * Opacity);

        // 结果图片
        if (_resultTexture != null)
        {
            int w = (int)(400 * _scaleAnim);
            int h = (int)(300 * _scaleAnim);
            int x = (1280 - w) / 2;
            int y = (720 - h) / 2 - 50;
            spriteBatch.Draw(_resultTexture, new Rectangle(x, y, w, h), color);
        }

        // 玩家名和结果
        for (int i = 0; i < 3; i++)
        {
            _nameLabels[i].Draw(spriteBatch);
            _resultLabels[i].Draw(spriteBatch);
        }

        _closeBtn.Draw(spriteBatch);
    }

    public override void HandleInput(InputManager input)
    {
        _mousePos = new Point((int)input.MousePosition.X, (int)input.MousePosition.Y);

        // 窗口拖拽
        if (input.LeftMouseClicked && _mousePos.Y < 60)
            _isDragging = true;
        if (input.LeftMouseReleased) _isDragging = false;

        _closeBtn.Update(input);

        if (input.KeyPressed(Keys.Escape) || input.KeyPressed(Keys.Enter))
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
