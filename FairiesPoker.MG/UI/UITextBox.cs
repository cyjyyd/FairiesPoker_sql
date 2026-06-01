using FairiesPoker.MG.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Globalization;

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
    public Color BackgroundColor { get; set; } = Color.Transparent;
    public Color BorderColor { get; set; } = Color.Gray * 0.5f;
    public SpriteFont? Font { get; set; }
    public bool IsPassword { get; set; } = false;
    public bool PreferIme { get; set; }

    public bool IsFocused { get; set; }
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
            bool wasFocused = IsFocused;
            IsFocused = ContainsPoint(mousePos);
            if (!wasFocused && IsFocused && PreferIme)
                input.OpenIme();
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
                        Text = RemoveLastTextElement(Text);
                        OnTextChanged?.Invoke(Text);
                    }
                    else if (key == Keys.Enter && !input.HasTextInputCharacters)
                    {
                        OnEnterPressed?.Invoke();
                    }
                    else if (key == Keys.A && (input.KeyHeld(Keys.LeftControl) || input.KeyHeld(Keys.RightControl)))
                    {
                        // Ctrl+A: 全选(暂不实现)
                    }
                    else if (input.ShouldUseKeyboardTextFallback)
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

            if (input.HasTextInputCharacters)
            {
                foreach (char c in input.TextInputCharacters)
                {
                    Text += c;
                }
                OnTextChanged?.Invoke(Text);
            }
        }
    }

    public override void Draw(SpriteBatch sb)
    {
        if (!Visible) return;

        var font = Font ?? FontManager.Default;
        if (font == null) return;

        var textValue = IsPassword ? new string('*', Text.Length) : Text;
        string displayText = string.IsNullOrEmpty(Text) ? Placeholder : textValue;
        Color color = string.IsNullOrEmpty(Text) ? PlaceholderColor : TextColor;
        var whitePixel = TextureManager.Get("_white") ?? CreateWhitePixel();

        if (BackgroundColor != Color.Transparent && Size.X > 0 && Size.Y > 0)
        {
            sb.Draw(whitePixel, Bounds, BackgroundColor);
        }

        FontManager.DrawString(sb, font, displayText, Position + new Vector2(4, 2), color);

        // 绘制光标
        if (IsFocused && _cursorVisible)
        {
            var textWidth = FontManager.MeasureString(textValue, font).X;
            var cursorRect = new Rectangle((int)(Position.X + textWidth + 4), (int)Position.Y, 1, (int)Size.Y);
            sb.Draw(whitePixel, cursorRect, TextColor);
        }

        // 绘制边框
        if (Size.X > 0 && Size.Y > 0 && BorderColor != Color.Transparent)
        {
            var x = (int)Position.X;
            var y = (int)Position.Y;
            var w = (int)Size.X;
            var h = (int)Size.Y;
            sb.Draw(whitePixel, new Rectangle(x, y, w, 1), BorderColor);
            sb.Draw(whitePixel, new Rectangle(x, y, 1, h), BorderColor);
            sb.Draw(whitePixel, new Rectangle(x, y + h - 1, w, 1), BorderColor);
            sb.Draw(whitePixel, new Rectangle(x + w - 1, y, 1, h), BorderColor);
        }
    }

    private static Texture2D? _whitePixel;
    private static Texture2D CreateWhitePixel()
    {
        if (_whitePixel != null) return _whitePixel;
        _whitePixel = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { Color.White });
        TextureManager.LoadInternal("_white", _whitePixel);
        return _whitePixel;
    }

    internal static string RemoveLastTextElement(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var enumerator = StringInfo.GetTextElementEnumerator(text);
        int lastIndex = 0;
        while (enumerator.MoveNext())
        {
            lastIndex = enumerator.ElementIndex;
        }

        return text.Substring(0, lastIndex);
    }

    internal static char KeyToChar(Keys key, bool shift)
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
        if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
        {
            return (char)('0' + (key - Keys.NumPad0));
        }

        return key switch
        {
            Keys.Space => ' ',
            Keys.Decimal => '.',
            Keys.Add => '+',
            Keys.Subtract => '-',
            Keys.Multiply => '*',
            Keys.Divide => '/',
            Keys.OemPeriod => '.',
            Keys.OemComma => ',',
            Keys.OemMinus => shift ? '_' : '-',
            Keys.OemPlus => shift ? '+' : '=',
            Keys.OemQuestion => shift ? '?' : '/',
            Keys.OemSemicolon => shift ? ':' : ';',
            Keys.OemQuotes => shift ? '"' : '\'',
            Keys.OemOpenBrackets => shift ? '{' : '[',
            Keys.OemCloseBrackets => shift ? '}' : ']',
            Keys.OemPipe => shift ? '|' : '\\',
            Keys.OemTilde => shift ? '~' : '`',
            _ => '\0'
        };
    }
}
