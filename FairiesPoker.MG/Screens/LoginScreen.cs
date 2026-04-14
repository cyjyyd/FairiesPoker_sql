using FairiesPoker.MG.Core;
using FairiesPoker.MG.Network;
using FairiesPoker.MG.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Protocol.Code;
using Protocol.Dto;
using System.Security.Cryptography;
using System.Text;

namespace FairiesPoker.MG.Screens;

/// <summary>
/// 登录屏幕 - 替代Login.cs
/// 用户名/密码输入 + 网络登录 + 服务器连接
/// </summary>
public class LoginScreen : ScreenBase
{
    private Texture2D? _bgTexture;

    private readonly UITextBox _usernameBox = new();
    private readonly UITextBox _passwordBox = new();
    private readonly UILabel _usernameLabel = new();
    private readonly UILabel _passwordLabel = new();
    private readonly UILabel _connStatus = new();
    private readonly UIButton _loginBtn = new();
    private readonly UIButton _offlineBtn = new();
    private readonly UILabel _registerLink = new();
    private readonly UILabel _changePwdLink = new();

    private NetManager? _netManager;
    private bool _isConnected;
    private bool _loginSuccess;
    private Point _mousePos;
    private bool _isDragging;
    private Vector2 _dragOffset;

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
        _netManager.Start();

        // 初始化UI控件 (位置对应原Login.cs)
        _usernameBox.Position = new Vector2(165, 288);
        _usernameBox.Size = new Vector2(541, 65);
        _usernameBox.Placeholder = "请输入用户名";

        _passwordBox.Position = new Vector2(165, 383);
        _passwordBox.Size = new Vector2(541, 65);
        _passwordBox.Placeholder = "请输入密码";

        _usernameLabel.Position = new Vector2(174, 294);
        _usernameLabel.Text = "用户名";
        _usernameLabel.TextColor = Color.LightGray;

        _passwordLabel.Position = new Vector2(174, 388);
        _passwordLabel.Text = "密码";
        _passwordLabel.TextColor = Color.LightGray;

        _loginBtn.Position = new Vector2(165, 556);
        _loginBtn.Size = new Vector2(246, 67);
        _loginBtn.Text = "登录";
        _loginBtn.TextColor = Color.White;
        _loginBtn.OnClick = OnLogin;

        _offlineBtn.Position = new Vector2(460, 556);
        _offlineBtn.Size = new Vector2(246, 67);
        _offlineBtn.Text = "离线模式";
        _offlineBtn.TextColor = Color.White;
        _offlineBtn.OnClick = OnOffline;

        _connStatus.Position = new Vector2(20, 620);
        _connStatus.Text = "连接中...";
        _connStatus.TextColor = Color.Yellow;
        _connStatus.Size = new Vector2(200, 30);

        _registerLink.Position = new Vector2(738, 318);
        _registerLink.Text = "注册账号";
        _registerLink.TextColor = Color.LightBlue;
        _registerLink.Size = new Vector2(100, 30);

        _changePwdLink.Position = new Vector2(738, 403);
        _changePwdLink.Text = "修改密码";
        _changePwdLink.TextColor = Color.LightBlue;
        _changePwdLink.Size = new Vector2(100, 30);

        // 订阅网络事件
        Models.OnLoginResult += OnLoginResult;
        _netManager.OnConnectionStateChanged += OnConnectionStateChanged;
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
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var color = Color.White * Opacity;

        // 背景
        if (_bgTexture != null)
        {
            spriteBatch.Draw(_bgTexture, new Rectangle(0, 0, 860, 656), color);
        }

        // UI控件
        _usernameBox.Draw(spriteBatch);
        _passwordBox.Draw(spriteBatch);
        _loginBtn.Draw(spriteBatch);
        _offlineBtn.Draw(spriteBatch);
        _connStatus.Draw(spriteBatch);
        _registerLink.Draw(spriteBatch);
        _changePwdLink.Draw(spriteBatch);

        // 输入框有内容时隐藏占位标签
        if (!string.IsNullOrEmpty(_usernameBox.Text))
            _usernameLabel.Visible = false;
        if (!string.IsNullOrEmpty(_passwordBox.Text))
            _passwordLabel.Visible = false;
    }

    public override void HandleInput(InputManager input)
    {
        _mousePos = new Point((int)input.MousePosition.X, (int)input.MousePosition.Y);

        // 窗口拖拽
        if (input.LeftMouseClicked && _mousePos.Y < 200)
        {
            _isDragging = true;
            _dragOffset = input.MousePosition;
        }
        if (input.LeftMouseReleased) _isDragging = false;

        // 更新UI控件输入
        _usernameBox.Update(input);
        _passwordBox.Update(input);
        _loginBtn.Update(input);
        _offlineBtn.Update(input);

        // 注册链接点击
        if (input.LeftMouseReleased && _registerLink.Bounds.Contains(_mousePos))
        {
            ScreenManager.Push(new RegisterScreen(Game, ScreenManager, _netManager!));
        }

        // 修改密码链接点击
        if (input.LeftMouseReleased && _changePwdLink.Bounds.Contains(_mousePos))
        {
            // TODO: 打开修改密码屏幕
        }
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

        // MD5加密
        string md5Pwd = MD5Hash(password);

        var loginDto = new AccountDto(username, md5Pwd);
        var msg = new SocketMsg(OpCode.ACCOUNT, AccountCode.LOGIN, loginDto);
        _netManager?.Execute(0, msg);

        _connStatus.Text = "登录中...";
        _connStatus.TextColor = Color.Yellow;
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
            _connStatus.Text = "登录成功!";
            _connStatus.TextColor = Color.Green;

            // 进入大厅
            ScreenManager.Replace(new LobbyScreen(Game, ScreenManager));
        }
        else
        {
            _connStatus.Text = result == -1 ? "用户名或密码错误" : "登录失败";
            _connStatus.TextColor = Color.Red;
        }
    }

    private static string MD5Hash(string input)
    {
        using var md5 = MD5.Create();
        byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder();
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
