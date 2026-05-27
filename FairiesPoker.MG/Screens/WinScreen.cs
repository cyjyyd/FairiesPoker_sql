using FairiesPoker.MG.Core;
using FairiesPoker.MG.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace FairiesPoker.MG.Screens;

/// <summary>
/// 胜负画面 - 替代win.cs
/// 显示比赛结果、玩家名、胜负状态，点击返回
/// 对齐原项目UI：使用Results图片背景、主题按钮图片
/// </summary>
public class WinScreen : ScreenBase
{
    // 结算背景图片
    private Texture2D? _resultBgTexture;

    // 按钮纹理
    private Texture2D? _btnNormalTexture;
    private Texture2D? _btnPressedTexture;

    // UI控件
    private readonly UILabel _titleLabel = new();      // "结果"标题
    private readonly UILabel[] _nameLabels = new UILabel[3];   // 玩家名
    private readonly UILabel[] _resultLabels = new UILabel[3]; // 胜负状态
    private readonly UIButton _closeBtn = new();

    // 数据
    private readonly string[] _playerNames = new string[3];
    private readonly bool[] _results = new bool[3]; // true=胜利, false=失败

    /// <summary>
    /// 关闭时的回调函数（用于通知GameScreen重置状态）
    /// </summary>
    private readonly Action? _onCloseCallback;

    // 窗口尺寸 (原win.cs: 822x508)
    private const int WindowWidth = 822;
    private const int WindowHeight = 508;

    // 按钮状态
    private bool _btnPressed;
    private bool _isClosing;

    public WinScreen(Game1 game, ScreenManager screenManager, bool[] results, string[] names, Action? onCloseCallback = null)
        : base(game, screenManager)
    {
        Array.Copy(results, _results, 3);
        Array.Copy(names, _playerNames, 3);
        _onCloseCallback = onCloseCallback;
    }

    public override void Initialize()
    {
        base.Initialize();
        Opacity = 0f;

        // 加载结算背景图片
        LoadResultBackground();

        // 加载按钮纹理
        _btnNormalTexture = UIResourceManager.ButtonNormal;
        _btnPressedTexture = UIResourceManager.ButtonPressed;

        // 初始化UI控件 (位置对应原win.Designer.cs)
        // 标题"结果" - 黑体42号, 橙红色, Position(295, 28)
        _titleLabel.Position = new Vector2(295, 28);
        _titleLabel.Text = "结果";
        _titleLabel.TextColor = new Color(255, 69, 0); // OrangeRed
        _titleLabel.Scale = 2.6f; // 约42号字体
        _titleLabel.Size = new Vector2(205, 84);

        // 玩家名标签 (对应原label2/label3/label4)
        // 原位置: label2(122,174), label3(122,266), label4(122,347)
        int[] nameYPos = { 174, 266, 347 };
        for (int i = 0; i < 3; i++)
        {
            _nameLabels[i] = new UILabel
            {
                Position = new Vector2(122, nameYPos[i]),
                Text = _playerNames[i],
                TextColor = new Color(255, 69, 0), // OrangeRed
                Scale = 1.5f, // 约24号字体
                Size = new Vector2(300, 48)
            };
        }

        // 胜负状态标签 (对应原label5/label6/label7)
        // 原位置: label5(579,174), label6(579,266), label7(579,347)
        int[] resultYPos = { 174, 266, 347 };
        for (int i = 0; i < 3; i++)
        {
            _resultLabels[i] = new UILabel
            {
                Position = new Vector2(579, resultYPos[i]),
                Text = _results[i] ? "胜利" : "失败",
                TextColor = _results[i] ? new Color(50, 205, 50) : new Color(255, 69, 0), // 胜利绿色，失败橙红
                Scale = 1.5f,
                Size = new Vector2(100, 48)
            };
        }

        // 确定按钮 (原button1: 位置(321,429), 尺寸(179,48))
        _closeBtn.Position = new Vector2(321, 429);
        _closeBtn.Size = new Vector2(179, 48);
        _closeBtn.Text = "确定";
        _closeBtn.TextColor = Color.White;
        _closeBtn.NormalTexture = _btnNormalTexture;
        _closeBtn.PressedTexture = _btnPressedTexture;
        _closeBtn.HoverTexture = _btnNormalTexture; // 悬停时也用正常纹理
        _closeBtn.OnClick = OnClose;
    }

    private void LoadResultBackground()
    {
        // 根据胜负结果选择正确的背景图片
        // 原win.cs逻辑:
        // - result[1]==true (自己是地主胜利): 如果result[0]和result[2]都是false → win_dz.png, 否则win_nm.png
        // - result[1]==false (自己失败): 如果result[0]和result[2]都是true → lose_dz.png, 否则lose_nm.png
        string imageName;
        if (_results[1])
        {
            // 自己胜利
            if (!_results[0] && !_results[2])
            {
                // 自己是唯一胜利的地主
                imageName = "win_dz.png";
            }
            else
            {
                // 自己作为农民胜利
                imageName = "win_nm.png";
            }
        }
        else
        {
            // 自己失败
            if (_results[0] && _results[2])
            {
                // 其他两人都胜利(自己作为地主失败)
                imageName = "lose_dz.png";
            }
            else
            {
                // 自己作为农民失败
                imageName = "lose_nm.png";
            }
        }

        _resultBgTexture = UIResourceManager.LoadResultImage(imageName);
    }

    public override void Update(GameTime gameTime)
    {
        // 淡入动画
        FadeIn(0.05);
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var color = Color.White * Opacity;
        var layout = GetWindowLayout();

        // 绘制结算背景图片
        if (_resultBgTexture != null)
        {
            spriteBatch.Draw(_resultBgTexture,
                layout.Bounds,
                color);
        }
        else
        {
            // 无图片时绘制半透明背景
            spriteBatch.Draw(UIResourceManager.WhitePixel,
                layout.Bounds,
                new Color(40, 45, 55, 200) * Opacity);
        }

        // 绘制标题
        DrawLabel(spriteBatch, _titleLabel, layout);

        // 绘制玩家名和胜负状态
        for (int i = 0; i < 3; i++)
        {
            DrawLabel(spriteBatch, _nameLabels[i], layout);
            DrawLabel(spriteBatch, _resultLabels[i], layout);
        }

        // 绘制按钮
        DrawButton(spriteBatch, _closeBtn, layout);
    }

    public override void HandleInput(InputManager input)
    {
        var layout = GetWindowLayout();
        var localMousePos = layout.ToLocal(input.MousePosition);
        bool closeHovered = _closeBtn.Bounds.Contains(localMousePos);

        if (input.LeftMouseClicked)
        {
            _btnPressed = closeHovered;
        }

        if (input.LeftMouseReleased)
        {
            bool shouldClose = _btnPressed && closeHovered;
            _btnPressed = false;

            if (shouldClose)
            {
                OnClose();
                return;
            }
        }
        else if (!input.LeftMouseHeld)
        {
            _btnPressed = false;
        }

        // ESC或Enter关闭
        if (input.KeyPressed(Keys.Escape) || input.KeyPressed(Keys.Enter))
        {
            OnClose();
        }
    }

    private void OnClose()
    {
        if (_isClosing) return;

        _isClosing = true;
        ScreenManager.Pop();
        _onCloseCallback?.Invoke();
    }

    private WindowLayout GetWindowLayout()
    {
        return WindowLayout.Create(DisplayManager.DesignWidth, DisplayManager.DesignHeight, WindowWidth, WindowHeight);
    }

    private void DrawLabel(SpriteBatch spriteBatch, UILabel label, WindowLayout layout)
    {
        if (!label.Visible || string.IsNullOrEmpty(label.Text)) return;

        var font = label.Font ?? FontManager.Default;
        if (font == null) return;

        float scale = label.Scale * layout.Scale;
        var size = label.Size * layout.Scale;
        var textSize = FontManager.MeasureString(label.Text, font, scale);
        var position = layout.ToScreen(label.Position);

        float x = label.TextAlignment switch
        {
            UILabel.AlignmentType.Center => position.X + (size.X - textSize.X) / 2f,
            UILabel.AlignmentType.Right => position.X + size.X - textSize.X,
            _ => position.X
        };

        FontManager.DrawString(spriteBatch, font, label.Text, new Vector2(x, position.Y),
            label.TextColor * Opacity, scale);
    }

    private void DrawButton(SpriteBatch spriteBatch, UIButton button, WindowLayout layout)
    {
        var rect = layout.ToScreenRectangle(button.Position, button.Size);
        var texture = _btnPressed ? button.PressedTexture : button.NormalTexture;

        if (texture != null)
        {
            spriteBatch.Draw(texture, rect, Color.White * Opacity);
        }
        else
        {
            spriteBatch.Draw(UIResourceManager.WhitePixel, rect,
                new Color(100, 100, 120, 200) * Opacity);
        }

        var font = FontManager.Default;
        if (font == null || string.IsNullOrEmpty(button.Text)) return;

        float scale = layout.Scale;
        var textSize = FontManager.MeasureString(button.Text, font, scale);
        var textPos = new Vector2(
            rect.X + (rect.Width - textSize.X) / 2f,
            rect.Y + (rect.Height - textSize.Y) / 2f);

        FontManager.DrawString(spriteBatch, font, button.Text, textPos, button.TextColor * Opacity, scale);
    }

    private readonly struct WindowLayout
    {
        public Rectangle Bounds { get; }
        public float Scale { get; }

        private WindowLayout(Rectangle bounds, float scale)
        {
            Bounds = bounds;
            Scale = scale;
        }

        public static WindowLayout Create(int viewportWidth, int viewportHeight, int windowWidth, int windowHeight)
        {
            float scale = Math.Min(1f, Math.Min(
                viewportWidth / (float)windowWidth,
                viewportHeight / (float)windowHeight));

            if (scale <= 0f || float.IsNaN(scale) || float.IsInfinity(scale))
            {
                scale = 1f;
            }

            int width = (int)Math.Round(windowWidth * scale);
            int height = (int)Math.Round(windowHeight * scale);
            int x = (viewportWidth - width) / 2;
            int y = (viewportHeight - height) / 2;

            return new WindowLayout(new Rectangle(x, y, width, height), scale);
        }

        public Vector2 ToScreen(Vector2 localPosition)
        {
            return new Vector2(
                Bounds.X + localPosition.X * Scale,
                Bounds.Y + localPosition.Y * Scale);
        }

        public Point ToLocal(Vector2 screenPosition)
        {
            return new Point(
                (int)((screenPosition.X - Bounds.X) / Scale),
                (int)((screenPosition.Y - Bounds.Y) / Scale));
        }

        public Rectangle ToScreenRectangle(Vector2 localPosition, Vector2 localSize)
        {
            return new Rectangle(
                (int)Math.Round(Bounds.X + localPosition.X * Scale),
                (int)Math.Round(Bounds.Y + localPosition.Y * Scale),
                (int)Math.Round(localSize.X * Scale),
                (int)Math.Round(localSize.Y * Scale));
        }
    }
}
