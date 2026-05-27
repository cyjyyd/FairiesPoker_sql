using FairiesPoker.MG.Core;
using FairiesPoker.MG.Network;
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
/// 注册屏幕 - 替代Register.cs
/// 用户名/密码/确认密码 + 头像预览
/// 对齐原项目UI：窗口342x286，输入框、标签、按钮位置
/// </summary>
public class RegisterScreen : ScreenBase
{
    // 资源
    private Texture2D? _btnNormalTexture;
    private Texture2D? _btnPressedTexture;
    private Texture2D? _defaultAvatarTexture; // Pla.jpg默认头像

    // UI控件数据
    private string _username = "";
    private string _password = "";
    private string _confirmPassword = "";
    private string _statusText = "";
    private Color _statusColor = Color.Gray;

    // 焦点状态
    private int _focusedField = 0; // 0=none, 1=username, 2=password, 3=confirm

    // 网络管理器
    private readonly NetManager _netManager;

    // 窗口尺寸 (原Register.cs: 342x286)
    private const int WindowWidth = 342;
    private const int WindowHeight = 286;

    // 光标闪烁
    private float _cursorBlinkTimer;
    private bool _cursorVisible;

    // 按钮悬停状态
    private bool _registerHovered;
    private bool _uploadHovered;

    public RegisterScreen(Game1 game, ScreenManager screenManager, NetManager netManager)
        : base(game, screenManager)
    {
        _netManager = netManager;
    }

    public override void Initialize()
    {
        base.Initialize();

        // 加载资源
        LoadResources();

        // 订阅事件
        Models.OnRegisterResult += OnRegisterResult;
    }

    private void LoadResources()
    {
        // 加载按钮纹理
        _btnNormalTexture = UIResourceManager.ButtonNormal;
        _btnPressedTexture = UIResourceManager.ButtonPressed;

        // 加载默认头像
        _defaultAvatarTexture = UIResourceManager.LoadResource("Pla.jpg");
    }

    public override void UnloadContent()
    {
        Models.OnRegisterResult -= OnRegisterResult;
    }

    public override void Update(GameTime gameTime)
    {
        FadeIn(0.05);
        _netManager.Update();

        // 光标闪烁
        _cursorBlinkTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_cursorBlinkTimer > 0.5f)
        {
            _cursorBlinkTimer = 0;
            _cursorVisible = !_cursorVisible;
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var color = Color.White * Opacity;

        // 计算居中位置
        int offsetX = (DisplayManager.DesignWidth - WindowWidth) / 2;
        int offsetY = (DisplayManager.DesignHeight - WindowHeight) / 2;

        // 绘制背景
        spriteBatch.Draw(UIResourceManager.WhitePixel,
            new Rectangle(offsetX, offsetY, WindowWidth, WindowHeight),
            new Color(240, 240, 240) * Opacity);

        var font = FontManager.Default;

        // 绘制标签 (对应原Register.Designer.cs的位置)
        // 用户名label(20,24), 密码label(20,57), 确认密码label(20,90), 头像label(20,123)
        if (font != null)
        {
            FontManager.DrawString(spriteBatch, font, "用户名：", new Vector2(offsetX + 20, offsetY + 24), Color.Black * Opacity);
            FontManager.DrawString(spriteBatch, font, "密码：", new Vector2(offsetX + 20, offsetY + 57), Color.Black * Opacity);
            FontManager.DrawString(spriteBatch, font, "确认密码：", new Vector2(offsetX + 20, offsetY + 90), Color.Black * Opacity);
            FontManager.DrawString(spriteBatch, font, "头像：", new Vector2(offsetX + 20, offsetY + 123), Color.Black * Opacity);
        }

        // 绘制输入框背景
        // txtUsername(99,20), txtPassword(99,53), txtConfirmPassword(99,86) - 尺寸223x25
        spriteBatch.Draw(UIResourceManager.WhitePixel,
            new Rectangle(offsetX + 99, offsetY + 20, 223, 25), Color.White * Opacity);
        spriteBatch.Draw(UIResourceManager.WhitePixel,
            new Rectangle(offsetX + 99, offsetY + 53, 223, 25), Color.White * Opacity);
        spriteBatch.Draw(UIResourceManager.WhitePixel,
            new Rectangle(offsetX + 99, offsetY + 86, 223, 25), Color.White * Opacity);

        // 绘制输入框边框(当聚焦时)
        if (_focusedField == 1)
            spriteBatch.Draw(UIResourceManager.WhitePixel,
                new Rectangle(offsetX + 99, offsetY + 20, 223, 2), Color.Blue * Opacity);
        if (_focusedField == 2)
            spriteBatch.Draw(UIResourceManager.WhitePixel,
                new Rectangle(offsetX + 99, offsetY + 53, 223, 2), Color.Blue * Opacity);
        if (_focusedField == 3)
            spriteBatch.Draw(UIResourceManager.WhitePixel,
                new Rectangle(offsetX + 99, offsetY + 86, 223, 2), Color.Blue * Opacity);

        // 绘制文本内容
        if (font != null)
        {
            // 用户名文本
            FontManager.DrawString(spriteBatch, font, _username, new Vector2(offsetX + 103, offsetY + 24), Color.Black * Opacity);
            if (_focusedField == 1 && _cursorVisible)
            {
                var textWidth = FontManager.MeasureString(_username, font).X;
                spriteBatch.Draw(UIResourceManager.WhitePixel,
                    new Rectangle((int)(offsetX + 103 + textWidth), (int)(offsetY + 22), 1, 21), Color.Black * Opacity);
            }

            // 密码文本(显示为●)
            string pwdDisplay = new string('●', _password.Length);
            FontManager.DrawString(spriteBatch, font, pwdDisplay, new Vector2(offsetX + 103, offsetY + 57), Color.Black * Opacity);
            if (_focusedField == 2 && _cursorVisible)
            {
                var textWidth = FontManager.MeasureString(pwdDisplay, font).X;
                spriteBatch.Draw(UIResourceManager.WhitePixel,
                    new Rectangle((int)(offsetX + 103 + textWidth), (int)(offsetY + 55), 1, 21), Color.Black * Opacity);
            }

            // 确认密码文本(显示为●)
            string confirmDisplay = new string('●', _confirmPassword.Length);
            FontManager.DrawString(spriteBatch, font, confirmDisplay, new Vector2(offsetX + 103, offsetY + 90), Color.Black * Opacity);
            if (_focusedField == 3 && _cursorVisible)
            {
                var textWidth = FontManager.MeasureString(confirmDisplay, font).X;
                spriteBatch.Draw(UIResourceManager.WhitePixel,
                    new Rectangle((int)(offsetX + 103 + textWidth), (int)(offsetY + 88), 1, 21), Color.Black * Opacity);
            }
        }

        // 绘制头像预览 (pictureBox1: 99,123, 尺寸80x80)
        if (_defaultAvatarTexture != null)
        {
            spriteBatch.Draw(_defaultAvatarTexture,
                new Rectangle(offsetX + 99, offsetY + 123, 80, 80), Color.White * Opacity);
        }
        else
        {
            spriteBatch.Draw(UIResourceManager.WhitePixel,
                new Rectangle(offsetX + 99, offsetY + 123, 80, 80), Color.LightGray * Opacity);
        }

        // 绘制上传头像按钮 (btnUploadAvatar: 187,178, 尺寸135x25)
        DrawButton(spriteBatch, offsetX + 187, offsetY + 178, 135, 25, "上传自定义头像", false, _uploadHovered);

        // 绘制注册按钮 (btnRegister: 99,220, 尺寸223x30)
        DrawButton(spriteBatch, offsetX + 99, offsetY + 220, 223, 30, "注册", true, _registerHovered);

        // 绘制状态文本 (lblStatus: 96,256)
        if (font != null && !string.IsNullOrEmpty(_statusText))
        {
            FontManager.DrawString(spriteBatch, font, _statusText, new Vector2(offsetX + 96, offsetY + 256), _statusColor * Opacity);
        }
    }

    private void DrawButton(SpriteBatch sb, int x, int y, int width, int height, string text, bool isPrimary, bool hovered)
    {
        var texture = isPrimary ? (hovered ? _btnPressedTexture : _btnNormalTexture) : null;

        if (texture != null)
        {
            sb.Draw(texture, new Rectangle(x, y, width, height), Color.White * Opacity);
        }
        else
        {
            // 绘制纯色按钮
            Color bgColor = isPrimary ? (hovered ? new Color(80, 80, 100) : new Color(100, 100, 120)) : (hovered ? new Color(180, 180, 180) : new Color(200, 200, 200));
            sb.Draw(UIResourceManager.WhitePixel, new Rectangle(x, y, width, height), bgColor * Opacity);
        }

        var font = FontManager.Default;
        if (font != null)
        {
            var textSize = FontManager.MeasureString(text, font);
            var textPos = new Vector2(x + (width - textSize.X) / 2, y + (height - textSize.Y) / 2);
            FontManager.DrawString(sb, font, text, textPos, isPrimary ? Color.White * Opacity : Color.Black * Opacity);
        }
    }

    public override void HandleInput(InputManager input)
    {
        // 计算偏移量
        int offsetX = (DisplayManager.DesignWidth - WindowWidth) / 2;
        int offsetY = (DisplayManager.DesignHeight - WindowHeight) / 2;

        // 转换为相对于窗口的坐标
        var localMousePos = new Point((int)input.MousePosition.X - offsetX, (int)input.MousePosition.Y - offsetY);

        // 更新悬停状态
        _registerHovered = new Rectangle(99, 220, 223, 30).Contains(localMousePos);
        _uploadHovered = new Rectangle(187, 178, 135, 25).Contains(localMousePos);

        // 点击处理
        if (input.LeftMouseReleased)
        {
            // 输入框点击
            if (new Rectangle(99, 20, 223, 25).Contains(localMousePos))
                _focusedField = 1;
            else if (new Rectangle(99, 53, 223, 25).Contains(localMousePos))
                _focusedField = 2;
            else if (new Rectangle(99, 86, 223, 25).Contains(localMousePos))
                _focusedField = 3;
            else if (new Rectangle(187, 178, 135, 25).Contains(localMousePos))
            {
                // TODO: 上传头像功能
            }
            else if (new Rectangle(99, 220, 223, 30).Contains(localMousePos))
            {
                OnRegister();
            }
            else
            {
                _focusedField = 0;
            }
        }

        // 键盘输入处理
        if (_focusedField > 0)
        {
            HandleTextInput(input);
        }

        // ESC返回
        if (input.KeyPressed(Keys.Escape))
        {
            ScreenManager.Pop();
        }
    }

    private void HandleTextInput(InputManager input)
    {
        string activeText = _focusedField == 1 ? _username :
                           _focusedField == 2 ? _password : _confirmPassword;

        // 退格
        if (input.KeyPressed(Keys.Back) && activeText.Length > 0)
        {
            activeText = activeText.Substring(0, activeText.Length - 1);
        }

        // Tab切换
        if (input.KeyPressed(Keys.Tab))
        {
            _focusedField = (_focusedField % 3) + 1;
        }

        // Enter
        if (input.KeyPressed(Keys.Enter))
        {
            if (_focusedField < 3)
                _focusedField++;
            else
                OnRegister();
        }

        // 字符输入
        foreach (Keys key in Enum.GetValues(typeof(Keys)))
        {
            if (input.KeyPressed(key))
            {
                char c = KeyToChar(key, input.KeyHeld(Keys.LeftShift) || input.KeyHeld(Keys.RightShift));
                if (c != '\0' && activeText.Length < 16)
                {
                    activeText += c;
                }
            }
        }

        // 更新对应字段
        if (_focusedField == 1) _username = activeText;
        else if (_focusedField == 2) _password = activeText;
        else _confirmPassword = activeText;
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
        return '\0';
    }

    private void OnRegister()
    {
        // 验证用户名
        if (_username.Length < 4 || _username.Length > 16)
        {
            _statusText = "用户名长度需为4-16位";
            _statusColor = Color.Red;
            _focusedField = 1;
            return;
        }

        // 验证密码
        if (_password.Length < 6 || _password.Length > 16)
        {
            _statusText = "密码长度需为6-16位";
            _statusColor = Color.Red;
            _focusedField = 2;
            return;
        }

        // 确认密码
        if (_password != _confirmPassword)
        {
            _statusText = "两次密码不一致";
            _statusColor = Color.Red;
            _focusedField = 3;
            return;
        }

        // MD5加密
        string md5Pwd = MD5Hash(_password);

        var dto = new AccountDto(_username, md5Pwd);
        var msg = new SocketMsg(OpCode.ACCOUNT, AccountCode.REGIST_CREQ, dto);
        _netManager.Execute(0, msg);

        _statusText = "注册中...";
        _statusColor = Color.Gray;
    }

    private void OnRegisterResult(bool success)
    {
        if (success)
        {
            _statusText = "注册成功!";
            _statusColor = Color.Green;

            // 延迟关闭
            System.Threading.Tasks.Task.Delay(500).ContinueWith(_ =>
            {
                ScreenManager.Pop();
            });
        }
        else
        {
            _statusText = "注册失败，用户名可能已存在";
            _statusColor = Color.Red;
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
