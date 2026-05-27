using FairiesPoker.MG.Core;
using FairiesPoker.MG.Network;
using FairiesPoker.MG.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Protocol.Code;
using Protocol.Dto;
using System;
using System.Security.Cryptography;
using System.Text;

namespace FairiesPoker.MG.Screens;

/// <summary>
/// 登录屏幕 - 替代Login.cs
/// 用户名/密码输入 + 网络登录 + 服务器连接
/// 对齐原项目UI：使用UILI2.png背景、btn1.png按钮、捕获.PNG头像
/// </summary>
public class LoginScreen : ScreenBase
{
    // 背景纹理
    private Texture2D? _bgTexture;
    // 头像纹理
    private Texture2D? _avatarTexture;
    // 按钮纹理
    private Texture2D? _btnNormalTexture;
    private Texture2D? _btnPressedTexture;

    // UI控件
    private readonly UITextBox _usernameBox = new();
    private readonly UITextBox _passwordBox = new();
    private readonly UILabel _usernameLabel = new();  // 占位提示"用户名"
    private readonly UILabel _passwordLabel = new();  // 占位提示"密码"
    private readonly UILabel _connStatus = new();
    private readonly UIButton _loginBtn = new();
    private readonly UIButton _offlineBtn = new();
    private readonly UILabel _registerLink = new();
    private readonly UILabel _changePwdLink = new();

    private NetManager? _netManager;
    private bool _isConnected;
    private bool _loginSuccess;
    private bool _loginBtnPressed;
    private bool _offlineBtnPressed;
    private string _pendingUsername = "";
    private string _pendingPassword = "";
    private bool _triedPaddedHashFallback;

    // 窗口尺寸 (原Login.cs: 860x656)
    private const int WindowWidth = 860;
    private const int WindowHeight = 656;
    private static readonly Rectangle UsernameBoxRect = new(165, 288, 541, 65);
    private static readonly Rectangle PasswordBoxRect = new(165, 383, 541, 65);
    private static readonly Rectangle RegisterLinkRect = new(738, 318, 100, 30);
    private static readonly Rectangle ChangePwdLinkRect = new(738, 403, 100, 30);

    public LoginScreen(Game1 game, ScreenManager screenManager)
        : base(game, screenManager)
    {
    }

    public override void Initialize()
    {
        base.Initialize();
        Opacity = 0f;

        // 初始化网络管理器
        _netManager = new NetManager();
        _netManager.OnConnectionStateChanged += OnConnectionStateChanged;

        // 加载资源
        LoadResources();

        // 初始化UI控件 (位置对应原Login.Designer.cs)
        // 头像pictureBox1: 位置(359,84), 尺寸(141,122)
        // 登录按钮button1: 位置(165,556), 尺寸(246,67)
        // 取消按钮button2: 位置(460,556), 尺寸(246,67)
        // 用户名textBox1: 位置(165,288), 尺寸(541,65)
        // 密码textBox2: 位置(165,383), 尺寸(541,65)
        // 用户名提示label1: 位置(174,294)
        // 密码提示label2: 位置(174,388)
        // 注册链接linkLabel1: 位置(738,318)
        // 修改密码链接linkLabel2: 位置(738,403)
        // 连接状态lblConnectionStatus: 位置(20,620)

        _usernameBox.Position = new Vector2(165, 288);
        _usernameBox.Size = new Vector2(541, 65);
        _usernameBox.Placeholder = "";
        _usernameBox.BackgroundColor = Color.White;

        _passwordBox.Position = new Vector2(165, 383);
        _passwordBox.Size = new Vector2(541, 65);
        _passwordBox.Placeholder = "";
        _passwordBox.BackgroundColor = Color.White;
        _passwordBox.IsPassword = true;

        // 占位提示标签 (当输入框为空时显示)
        _usernameLabel.Position = new Vector2(174, 294);
        _usernameLabel.Text = "用户名";
        _usernameLabel.TextColor = Color.LightGray;
        _usernameLabel.BackgroundColor = Color.White;
        _usernameLabel.Scale = 1.4f;

        _passwordLabel.Position = new Vector2(174, 388);
        _passwordLabel.Text = "密码";
        _passwordLabel.TextColor = Color.LightGray;
        _passwordLabel.BackgroundColor = Color.White;
        _passwordLabel.Scale = 1.4f;

        // 登录按钮
        _loginBtn.Position = new Vector2(165, 556);
        _loginBtn.Size = new Vector2(246, 67);
        _loginBtn.Text = "登录";
        _loginBtn.TextColor = Color.Snow;
        _loginBtn.NormalTexture = _btnNormalTexture;
        _loginBtn.PressedTexture = _btnPressedTexture;
        _loginBtn.HoverTexture = _btnNormalTexture;
        _loginBtn.OnClick = OnLogin;

        // 离线模式按钮
        _offlineBtn.Position = new Vector2(460, 556);
        _offlineBtn.Size = new Vector2(246, 67);
        _offlineBtn.Text = "离线模式";
        _offlineBtn.TextColor = Color.Snow;
        _offlineBtn.NormalTexture = _btnNormalTexture;
        _offlineBtn.PressedTexture = _btnPressedTexture;
        _offlineBtn.HoverTexture = _btnNormalTexture;
        _offlineBtn.OnClick = OnOffline;

        // 连接状态
        _connStatus.Position = new Vector2(20, 620);
        _connStatus.Text = "连接中...";
        _connStatus.TextColor = Color.Gray;

        // 注册链接
        _registerLink.Position = new Vector2(738, 318);
        _registerLink.Text = "注册账号";
        _registerLink.TextColor = Color.LightBlue;

        // 修改密码链接
        _changePwdLink.Position = new Vector2(738, 403);
        _changePwdLink.Text = "修改密码";
        _changePwdLink.TextColor = Color.LightBlue;

        // 订阅网络事件
        Models.OnLoginResult += OnLoginResult;

        _netManager.Start();
    }

    private void LoadResources()
    {
        // 加载背景图片 (UILI2.png)
        _bgTexture = UIResourceManager.LoadResource("UILI2.png");

        // 加载头像图片 (捕获.PNG)
        _avatarTexture = UIResourceManager.LoadResource("捕获.PNG");

        // 加载按钮纹理
        _btnNormalTexture = UIResourceManager.ButtonNormal;
        _btnPressedTexture = UIResourceManager.ButtonPressed;
    }

    public override void UnloadContent()
    {
        // 取消订阅
        Models.OnLoginResult -= OnLoginResult;
        _netManager.OnConnectionStateChanged -= OnConnectionStateChanged;

        // 如果登录未成功，断开连接
        if (!_loginSuccess)
        {
            _netManager?.Disconnect();
        }
    }

    public override void Update(GameTime gameTime)
    {
        FadeIn(0.03);

        // 更新网络消息处理
        _netManager?.Update();

        // 更新占位标签可见性
        _usernameLabel.Visible = string.IsNullOrEmpty(_usernameBox.Text);
        _passwordLabel.Visible = string.IsNullOrEmpty(_passwordBox.Text);
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var color = Color.White * Opacity;
        var layout = GetWindowLayout();

        // 绘制背景
        if (_bgTexture != null)
        {
            spriteBatch.Draw(_bgTexture,
                layout.Bounds,
                color);
        }
        else
        {
            // 无图片时绘制半透明背景
            spriteBatch.Draw(UIResourceManager.WhitePixel,
                layout.Bounds,
                new Color(60, 60, 80, 200) * Opacity);
        }

        // 绘制头像 (位置359,84, 尺寸141x122)
        if (_avatarTexture != null)
        {
            spriteBatch.Draw(_avatarTexture,
                layout.ToScreenRectangle(359, 84, 141, 122),
                color);
        }

        // 绘制输入框背景(白色)
        spriteBatch.Draw(UIResourceManager.WhitePixel,
            layout.ToScreenRectangle(UsernameBoxRect),
            Color.White * Opacity);
        spriteBatch.Draw(UIResourceManager.WhitePixel,
            layout.ToScreenRectangle(PasswordBoxRect),
            Color.White * Opacity);

        // 绘制输入框文本
        var font = FontManager.Default;
        if (font != null)
        {
            float inputTextScale = 1.4f * layout.Scale;

            // 用户名文本
            string usernameDisplay = _usernameBox.Text;
            if (string.IsNullOrEmpty(usernameDisplay) && !_usernameBox.IsFocused)
            {
                // 显示占位提示
                FontManager.DrawString(spriteBatch, font, _usernameLabel.Text,
                    layout.ToScreen(new Vector2(174, 294)),
                    Color.LightGray * Opacity, inputTextScale);
            }
            else
            {
                FontManager.DrawString(spriteBatch, font, usernameDisplay,
                    layout.ToScreen(new Vector2(174, 294)),
                    Color.Black * Opacity, inputTextScale);
            }

            // 密码文本(显示为星号)
            string passwordDisplay = _passwordBox.IsPassword && !string.IsNullOrEmpty(_passwordBox.Text)
                ? new string('*', _passwordBox.Text.Length)
                : _passwordBox.Text;
            if (string.IsNullOrEmpty(_passwordBox.Text) && !_passwordBox.IsFocused)
            {
                FontManager.DrawString(spriteBatch, font, _passwordLabel.Text,
                    layout.ToScreen(new Vector2(174, 388)),
                    Color.LightGray * Opacity, inputTextScale);
            }
            else
            {
                FontManager.DrawString(spriteBatch, font, passwordDisplay,
                    layout.ToScreen(new Vector2(174, 388)),
                    Color.Black * Opacity, inputTextScale);
            }
        }

        // 绘制按钮
        DrawButton(spriteBatch, _loginBtn, layout, _loginBtnPressed);
        DrawButton(spriteBatch, _offlineBtn, layout, _offlineBtnPressed);

        // 绘制连接状态
        if (font != null)
        {
            FontManager.DrawString(spriteBatch, font, _connStatus.Text,
                layout.ToScreen(new Vector2(20, 620)),
                _connStatus.TextColor * Opacity, layout.Scale);
        }

        // 绘制链接
        if (font != null)
        {
            FontManager.DrawString(spriteBatch, font, _registerLink.Text,
                layout.ToScreen(new Vector2(738, 318)),
                _registerLink.TextColor * Opacity, layout.Scale);
            FontManager.DrawString(spriteBatch, font, _changePwdLink.Text,
                layout.ToScreen(new Vector2(738, 403)),
                _changePwdLink.TextColor * Opacity, layout.Scale);
        }
    }

    private void DrawButton(SpriteBatch sb, UIButton btn, WindowLayout layout, bool pressed)
    {
        var texture = pressed
            ? btn.PressedTexture ?? btn.NormalTexture
            : btn.NormalTexture;
        var rect = layout.ToScreenRectangle(btn.Position, btn.Size);

        if (texture != null)
        {
            sb.Draw(texture, rect, Color.White * Opacity);
        }
        else
        {
            sb.Draw(UIResourceManager.WhitePixel, rect,
                (pressed ? btn.PressedBackgroundColor : btn.BackgroundColor) * Opacity);
        }

        // 绘制按钮文字
        var font = FontManager.Default;
        if (font != null && !string.IsNullOrEmpty(btn.Text))
        {
            float textScale = layout.Scale;
            var textSize = FontManager.MeasureString(btn.Text, font, textScale);
            var textPos = new Vector2(
                rect.X + (rect.Width - textSize.X) / 2f,
                rect.Y + (rect.Height - textSize.Y) / 2f);
            FontManager.DrawString(sb, font, btn.Text, textPos, btn.TextColor * Opacity, textScale);
        }
    }

    public override void HandleInput(InputManager input)
    {
        var layout = GetWindowLayout();

        // 转换为相对于窗口的坐标
        var localMousePos = layout.ToLocal(input.MousePosition);

        // 更新按钮状态
        bool loginHovered = _loginBtn.Bounds.Contains(localMousePos);
        bool offlineHovered = _offlineBtn.Bounds.Contains(localMousePos);

        if (input.LeftMouseClicked)
        {
            _loginBtnPressed = loginHovered;
            _offlineBtnPressed = offlineHovered;
        }

        // 点击处理
        if (input.LeftMouseReleased)
        {
            bool loginClicked = _loginBtnPressed && loginHovered;
            bool offlineClicked = _offlineBtnPressed && offlineHovered;
            _loginBtnPressed = false;
            _offlineBtnPressed = false;

            if (loginClicked) OnLogin();
            else if (offlineClicked) OnOffline();
            else if (UsernameBoxRect.Contains(localMousePos))
            {
                _usernameBox.IsFocused = true;
                _passwordBox.IsFocused = false;
            }
            else if (PasswordBoxRect.Contains(localMousePos))
            {
                _usernameBox.IsFocused = false;
                _passwordBox.IsFocused = true;
            }
            else if (RegisterLinkRect.Contains(localMousePos))
            {
                // 打开注册
                ScreenManager.Push(new RegisterScreen(Game, ScreenManager, _netManager!));
            }
            else if (ChangePwdLinkRect.Contains(localMousePos))
            {
                // TODO: 打开修改密码屏幕
            }
        }
        else if (!input.LeftMouseHeld)
        {
            _loginBtnPressed = false;
            _offlineBtnPressed = false;
        }

        // 键盘输入处理
        if (_usernameBox.IsFocused || _passwordBox.IsFocused)
        {
            HandleTextInput(input);
        }

        // ESC返回
        if (input.KeyPressed(Keys.Escape))
        {
            OnOffline();
        }
    }

    private WindowLayout GetWindowLayout()
    {
        return WindowLayout.Create(DisplayManager.DesignWidth, DisplayManager.DesignHeight, WindowWidth, WindowHeight);
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

        public Rectangle ToScreenRectangle(float x, float y, float width, float height)
        {
            return ToScreenRectangle(new Vector2(x, y), new Vector2(width, height));
        }

        public Rectangle ToScreenRectangle(Rectangle localRectangle)
        {
            return ToScreenRectangle(
                new Vector2(localRectangle.X, localRectangle.Y),
                new Vector2(localRectangle.Width, localRectangle.Height));
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

    private void HandleTextInput(InputManager input)
    {
        UITextBox activeBox = _usernameBox.IsFocused ? _usernameBox : _passwordBox;

        // 处退格
        if (input.KeyPressed(Keys.Back) && activeBox.Text.Length > 0)
        {
            activeBox.Text = activeBox.Text.Substring(0, activeBox.Text.Length - 1);
        }

        // Enter提交
        if (input.KeyPressed(Keys.Enter))
        {
            if (_usernameBox.IsFocused)
            {
                _usernameBox.IsFocused = false;
                _passwordBox.IsFocused = true;
            }
            else
            {
                OnLogin();
            }
        }

        // Tab切换
        if (input.KeyPressed(Keys.Tab))
        {
            _usernameBox.IsFocused = !_usernameBox.IsFocused;
            _passwordBox.IsFocused = !_passwordBox.IsFocused;
        }

        // 字符输入
        foreach (Keys key in Enum.GetValues(typeof(Keys)))
        {
            if (input.KeyPressed(key))
            {
                char c = KeyToChar(key, input.KeyHeld(Keys.LeftShift) || input.KeyHeld(Keys.RightShift));
                if (c != '\0')
                {
                    activeBox.Text += c;
                }
            }
        }
    }

    private static char KeyToChar(Keys key, bool shift)
    {
        if (key >= Keys.A && key <= Keys.Z)
        {
            char c = (char)('a' + (key - Keys.A));
            return shift ? char.ToUpper(c) : c;
        }
        if (key >= Keys.D0 && key <= Keys.D9)
        {
            return shift ? (key - Keys.D0) switch
            {
                1 => '!', 2 => '@', 3 => '#', 4 => '$', 5 => '%',
                6 => '^', 7 => '&', 8 => '*', 9 => '(', 0 => ')',
                _ => '0'
            } : (char)('0' + (key - Keys.D0));
        }
        if (key == Keys.Space) return ' ';
        if (key == Keys.OemPeriod) return shift ? '>' : '.';
        if (key == Keys.OemComma) return shift ? '<' : ',';
        if (key == Keys.OemMinus) return shift ? '_' : '-';
        if (key == Keys.OemPlus) return shift ? '+' : '=';
        return '\0';
    }

    private void OnLogin()
    {
        if (!_isConnected)
        {
            _connStatus.Text = "服务器未连接!";
            _connStatus.TextColor = Color.Red;
            return;
        }

        string username = _usernameBox.Text;
        string password = _passwordBox.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            _connStatus.Text = "请输入用户名和密码";
            _connStatus.TextColor = Color.Red;
            return;
        }

        _pendingUsername = username;
        _pendingPassword = password;
        _triedPaddedHashFallback = false;

        SendLoginRequest(CreateLegacyPasswordHash(password));

        _connStatus.Text = "登录中...";
        _connStatus.TextColor = Color.Yellow;
    }

    private void SendLoginRequest(string passwordHash)
    {
        var loginDto = new AccountDto(_pendingUsername, passwordHash);
        var msg = new SocketMsg(OpCode.ACCOUNT, AccountCode.LOGIN, loginDto);
        _netManager?.Execute(0, msg);
    }

    private void OnOffline()
    {
        _netManager?.Disconnect();
        ScreenManager.Pop();
    }

    private void OnConnectionStateChanged(bool connected, string message)
    {
        _isConnected = connected;
        if (connected)
        {
            _connStatus.Text = "已连接";
            _connStatus.TextColor = Color.Green;
        }
        else
        {
            _connStatus.Text = message ?? "未连接";
            _connStatus.TextColor = Color.Red;
        }
    }

    private void OnLoginResult(int result)
    {
        if (result == 0)
        {
            _loginSuccess = true;
            ClearPendingLogin();
            _connStatus.Text = "登录成功!";
            _connStatus.TextColor = Color.Green;

            // 进入大厅
            ScreenManager.Replace(new LobbyScreen(Game, ScreenManager));
        }
        else
        {
            if (result == -3 && !_triedPaddedHashFallback && !string.IsNullOrEmpty(_pendingPassword))
            {
                _triedPaddedHashFallback = true;
                SendLoginRequest(CreatePaddedPasswordHash(_pendingPassword));
                _connStatus.Text = "登录中...";
                _connStatus.TextColor = Color.Yellow;
                return;
            }

            ClearPendingLogin();
            _connStatus.Text = result switch
            {
                -1 => "账号不存在",
                -2 => "账号已登录",
                -3 => "用户名或密码错误",
                _ => "登录失败"
            };
            _connStatus.TextColor = Color.Red;
        }
    }

    private void ClearPendingLogin()
    {
        _pendingUsername = "";
        _pendingPassword = "";
        _triedPaddedHashFallback = false;
    }

    private static string CreateLegacyPasswordHash(string input)
    {
        using var md5 = MD5.Create();
        byte[] bytes = md5.ComputeHash(Encoding.Default.GetBytes(input));
        var sb = new StringBuilder();
        foreach (var b in bytes) sb.Append(b.ToString("X"));
        return sb.ToString();
    }

    private static string CreatePaddedPasswordHash(string input)
    {
        using var md5 = MD5.Create();
        byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder();
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
