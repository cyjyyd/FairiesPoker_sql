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
/// 注册屏幕 - 替代Register.cs
/// 用户名/密码/确认密码 + 头像上传
/// </summary>
public class RegisterScreen : ScreenBase
{
    private readonly UITextBox _usernameBox = new();
    private readonly UITextBox _passwordBox = new();
    private readonly UITextBox _confirmBox = new();
    private readonly UIButton _registerBtn = new();
    private readonly UILabel _statusLabel = new();
    private readonly UILabel _usernameLabel = new();
    private readonly UILabel _passwordLabel = new();
    private readonly UILabel _confirmLabel = new();

    private NetManager _netManager;
    private Point _mousePos;

    public RegisterScreen(Game1 game, ScreenManager screenManager, NetManager netManager)
        : base(game, screenManager)
    {
        _netManager = netManager;
    }

    public override void Initialize()
    {
        base.Initialize();

        // 初始化UI
        _usernameLabel.Position = new Vector2(20, 20);
        _usernameLabel.Text = "用户名:";
        _usernameLabel.TextColor = Color.White;
        _usernameLabel.Size = new Vector2(70, 25);

        _usernameBox.Position = new Vector2(99, 20);
        _usernameBox.Size = new Vector2(223, 25);

        _passwordLabel.Position = new Vector2(20, 53);
        _passwordLabel.Text = "密码:";
        _passwordLabel.TextColor = Color.White;
        _passwordLabel.Size = new Vector2(70, 25);

        _passwordBox.Position = new Vector2(99, 53);
        _passwordBox.Size = new Vector2(223, 25);
        _passwordBox.Placeholder = "******";

        _confirmLabel.Position = new Vector2(20, 86);
        _confirmLabel.Text = "确认密码:";
        _confirmLabel.TextColor = Color.White;
        _confirmLabel.Size = new Vector2(70, 25);

        _confirmBox.Position = new Vector2(99, 86);
        _confirmBox.Size = new Vector2(223, 25);
        _confirmBox.Placeholder = "******";

        _registerBtn.Position = new Vector2(99, 220);
        _registerBtn.Size = new Vector2(223, 30);
        _registerBtn.Text = "注册";
        _registerBtn.TextColor = Color.White;
        _registerBtn.OnClick = OnRegister;

        _statusLabel.Position = new Vector2(96, 256);
        _statusLabel.Text = "";
        _statusLabel.TextColor = Color.Yellow;
        _statusLabel.Size = new Vector2(230, 30);

        // 订阅事件
        Models.OnRegisterResult += OnRegisterResult;
    }

    public override void UnloadContent()
    {
        Models.OnRegisterResult -= OnRegisterResult;
    }

    public override void Update(GameTime gameTime)
    {
        FadeIn(0.05);
        _netManager.Update();
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var color = Color.White * Opacity;

        // 背景
        spriteBatch.Draw(TextureManager.Get("_white") ?? CreateWhitePixel(),
            new Rectangle(0, 0, 342, 286), new Color(40, 45, 55, 240) * color);

        // UI
        _usernameLabel.Draw(spriteBatch);
        _passwordLabel.Draw(spriteBatch);
        _confirmLabel.Draw(spriteBatch);
        _usernameBox.Draw(spriteBatch);
        _passwordBox.Draw(spriteBatch);
        _confirmBox.Draw(spriteBatch);
        _registerBtn.Draw(spriteBatch);
        _statusLabel.Draw(spriteBatch);
    }

    public override void HandleInput(InputManager input)
    {
        _mousePos = new Point((int)input.MousePosition.X, (int)input.MousePosition.Y);

        _usernameBox.Update(input);
        _passwordBox.Update(input);
        _confirmBox.Update(input);
        _registerBtn.Update(input);

        // ESC 返回
        if (input.KeyPressed(Keys.Escape))
        {
            ScreenManager.Pop();
        }
    }

    private void OnRegister()
    {
        string username = _usernameBox.Text;
        string password = _passwordBox.Text;
        string confirm = _confirmBox.Text;

        // 验证
        if (username.Length < 4 || username.Length > 16)
        {
            _statusLabel.Text = "用户名长度需为4-16位";
            _statusLabel.TextColor = Color.Red;
            return;
        }
        if (password.Length < 6 || password.Length > 16)
        {
            _statusLabel.Text = "密码长度需为6-16位";
            _statusLabel.TextColor = Color.Red;
            return;
        }
        if (password != confirm)
        {
            _statusLabel.Text = "两次密码不一致";
            _statusLabel.TextColor = Color.Red;
            return;
        }

        // MD5加密
        string md5Pwd = MD5Hash(password);

        var dto = new AccountDto(username, md5Pwd);
        var msg = new SocketMsg(OpCode.ACCOUNT, AccountCode.REGIST_CREQ, dto);
        _netManager.Execute(0, msg);

        _statusLabel.Text = "注册中...";
        _statusLabel.TextColor = Color.Yellow;
    }

    private void OnRegisterResult(bool success)
    {
        if (success)
        {
            _statusLabel.Text = "注册成功!";
            _statusLabel.TextColor = Color.Green;
        }
        else
        {
            _statusLabel.Text = "注册失败";
            _statusLabel.TextColor = Color.Red;
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

    private static Texture2D? _whitePixel;
    private static Texture2D CreateWhitePixel()
    {
        if (_whitePixel != null) return _whitePixel;
        _whitePixel = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { Color.White });
        return _whitePixel;
    }
}
