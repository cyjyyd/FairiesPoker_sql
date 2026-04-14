using FairiesPoker.MG.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace FairiesPoker.MG.UI;

/// <summary>
/// UI文本输入框 - 光标闪烁、占位符
/// </summary>
public class UITextBox : UIControl
{
    public string Text { get; set; } = "";
    public string Placeholder { get; set; } = "";
    public Color TextColor { get; set; } = Color.White;
    public Color PlaceholderColor { get; set; } = Color.Gray;
    public SpriteFont? Font { get; set; }

    public bool IsFocused { get; private set; }
    public Action<string>? OnTextChanged;
    public Action? OnEnterPressed;

    private float _cursorBlinkTimer;
    private bool _cursorVisible;

    public override void Update(InputManager input)
    {
        if (!Visible || !Enabled) return;

        var mousePos = new Point((int)input.MousePosition.X, (int)input.MousePosition.Y);

        if (input.LeftMouseClicked)
        {
            IsFocused = ContainsPoint(mousePos);
        }

        if (IsFocused)
        {
            // 光标闪烁
            _cursorBlinkTimer += (float)0.016f;
            if (_cursorBlinkTimer > 0.5f)
            {
                _cursorBlinkTimer = 0;
                _cursorVisible = !_cursorVisible;
            }

            // 键盘输入
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (input.KeyPressed(key))
                {
                    if (key == Keys.Back && Text.Length > 0)
                    {
                        Text = Text.Substring(0, Text.Length - 1);
                        OnTextChanged?.Invoke(Text);
                    }
                    else if (key == Keys.Enter)
                    {
                        OnEnterPressed?.Invoke();
                    }
                    else if (key == Keys.A && input.KeyHeld(Keys.LeftControl))
                    {
                        // Ctrl+A: 全选(暂不实现)
                    }
                    else
                    {
                        char c = KeyToChar(key, input.KeyHeld(Keys.LeftShift) || input.KeyHeld(Keys.RightShift));
                        if (c != '\0')
                        {
                            Text += c;
                            OnTextChanged?.Invoke(Text);
                        }
                    }
                }
            }
        }
    }

    public override void Draw(SpriteBatch sb)
    {
        if (!Visible) return;

        var font = Font ?? FontManager.Default;
        if (font == null) return;

        string displayText = string.IsNullOrEmpty(Text) ? Placeholder : Text;
        Color color = string.IsNullOrEmpty(Text) ? PlaceholderColor : TextColor;

        sb.DrawString(font, displayText, Position + new Vector2(4, 2), color);

        // 绘制光标
        if (IsFocused && _cursorVisible)
        {
            var textWidth = font.MeasureString(Text).X;
            var cursorRect = new Rectangle((int)(Position.X + textWidth + 4), (int)Position.Y, 1, (int)Size.Y);
            sb.Draw(TextureManager.Get("_white") ?? CreateWhitePixel(), cursorRect, TextColor);
        }

        // 绘制边框
        var borderRect = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        sb.Draw(TextureManager.Get("_white") ?? CreateWhitePixel(), borderRect, Color.Gray * 0.5f);
    }

    private static Texture2D? _whitePixel;
    private static Texture2D CreateWhitePixel()
    {
        if (_whitePixel != null) return _whitePixel;
        _whitePixel = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { Color.White });
        TextureManager.Load("_white", ""); // dummy
        return _whitePixel;
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
}
