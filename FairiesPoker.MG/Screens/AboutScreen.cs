using System;
using System.Reflection;
using FairiesPoker.MG.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FairiesPoker.MG.Screens;

public class AboutScreen : ScreenBase
{
    private const int WindowWidth = 773;
    private const int WindowHeight = 506;
    private const string CopyrightText = "Copyright (C) 2026 FairiesPoker. All rights reserved.";
    private const string CopyrightWarningText =
        "警告：本计算机程序受著作权法和国际条约保护。如未经授权而擅自复制或传播本程序（或其中任何部分），将受到严厉的民事及刑事制裁，并将在法律许可范围内受到最大程度的起诉";

    private static readonly Rectangle LogoRect = new(33, 33, 308, 440);
    private static readonly Rectangle CloseButtonRect = new(690, 18, 70, 31);

    private Texture2D _logoTexture;
    private bool _logoPressed;
    private bool _closePressed;

    public AboutScreen(Game1 game, ScreenManager screenManager)
        : base(game, screenManager)
    {
        Opacity = 0f;
    }

    public override void LoadContent()
    {
        _logoTexture = UIResourceManager.LoadResource("prf.jpg")
            ?? UIResourceManager.LoadResource("Pla.jpg");
    }

    public override void Update(GameTime gameTime)
    {
        FadeIn(0.05);
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var layout = WindowLayout.Create(DisplayManager.DesignWidth, DisplayManager.DesignHeight, WindowWidth, WindowHeight);

        spriteBatch.Draw(UIResourceManager.WhitePixel, new Rectangle(0, 0, DisplayManager.DesignWidth, DisplayManager.DesignHeight),
            new Color(0, 0, 0, 120) * Opacity);
        DrawPanel(spriteBatch, layout.Bounds);

        var logoScreenRect = layout.ToScreenRectangle(LogoRect);
        if (_logoTexture != null)
            spriteBatch.Draw(_logoTexture, logoScreenRect, Color.White * Opacity);
        else
            spriteBatch.Draw(UIResourceManager.WhitePixel, logoScreenRect, new Color(35, 45, 60, 220) * Opacity);

        DrawAboutText(spriteBatch, layout);
        DrawButton(spriteBatch, layout.ToScreenRectangle(CloseButtonRect), "关闭", _closePressed);
    }

    public override void HandleInput(InputManager input)
    {
        var layout = WindowLayout.Create(DisplayManager.DesignWidth, DisplayManager.DesignHeight, WindowWidth, WindowHeight);
        var localMousePos = layout.ToLocal(input.MousePosition);
        bool logoHovered = LogoRect.Contains(localMousePos);
        bool closeHovered = CloseButtonRect.Contains(localMousePos);

        if (input.KeyPressed(Keys.Escape) || input.KeyPressed(Keys.Enter))
        {
            ScreenManager.Pop();
            return;
        }

        if (input.LeftMouseClicked)
        {
            _logoPressed = logoHovered;
            _closePressed = closeHovered;
        }

        if (input.LeftMouseReleased)
        {
            bool openCards = _logoPressed && logoHovered;
            bool close = _closePressed && closeHovered;

            _logoPressed = false;
            _closePressed = false;

            if (openCards)
            {
                Game.AudioManager?.PlaySfx(SoundCue.Click);
                ScreenManager.Push(new CardPreviewScreen(Game, ScreenManager));
                return;
            }

            if (close)
            {
                Game.AudioManager?.PlaySfx(SoundCue.Click);
                ScreenManager.Pop();
            }
        }
    }

    private void DrawAboutText(SpriteBatch spriteBatch, WindowLayout layout)
    {
        var font = FontManager.Default;
        if (font == null) return;

        string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";

        DrawLine(spriteBatch, layout, "产品名称  FairiesPoker", new Vector2(358, 44));
        DrawLine(spriteBatch, layout, $"版本 {version}", new Vector2(358, 89));
        DrawLine(spriteBatch, layout, "版权  Copyright (C) 2026 FairiesPoker.", new Vector2(358, 134), 0.85f);
        DrawLine(spriteBatch, layout, "All rights reserved.", new Vector2(358, 158), 0.85f);
        DrawLine(spriteBatch, layout, "公司名称  FairiesPoker", new Vector2(358, 194));
        DrawWrappedText(spriteBatch, layout, CopyrightWarningText, new Vector2(358, 246), 342, 0.88f);
    }

    private void DrawLine(SpriteBatch spriteBatch, WindowLayout layout, string text, Vector2 localPosition, float scale = 1f)
    {
        FontManager.DrawString(spriteBatch, FontManager.Default, text, layout.ToScreen(localPosition),
            Color.WhiteSmoke * Opacity, scale * layout.Scale);
    }

    private void DrawWrappedText(SpriteBatch spriteBatch, WindowLayout layout, string text, Vector2 localPosition, int maxWidth, float scale)
    {
        float y = localPosition.Y;
        foreach (string line in WrapText(text, maxWidth, scale))
        {
            FontManager.DrawString(spriteBatch, FontManager.Default, line, layout.ToScreen(new Vector2(localPosition.X, y)),
                Color.WhiteSmoke * Opacity, scale * layout.Scale);
            y += 28;
        }
    }

    private static string[] WrapText(string text, int maxWidth, float scale)
    {
        var font = FontManager.Default;
        if (font == null || string.IsNullOrEmpty(text))
            return Array.Empty<string>();

        var lines = new System.Collections.Generic.List<string>();
        string current = "";
        foreach (char c in text)
        {
            if (c == '\r')
                continue;
            if (c == '\n')
            {
                lines.Add(current);
                current = "";
                continue;
            }

            string candidate = current + c;
            if (current.Length > 0 && FontManager.MeasureString(candidate, font, scale).X > maxWidth)
            {
                lines.Add(current);
                current = c.ToString();
            }
            else
            {
                current = candidate;
            }
        }

        if (current.Length > 0)
            lines.Add(current);

        return lines.ToArray();
    }

    private void DrawPanel(SpriteBatch spriteBatch, Rectangle rect)
    {
        spriteBatch.Draw(UIResourceManager.WhitePixel, rect, new Color(24, 28, 38, 238) * Opacity);
        DrawBorder(spriteBatch, rect, new Color(210, 170, 85, 220) * Opacity);
    }

    private void DrawButton(SpriteBatch spriteBatch, Rectangle rect, string text, bool pressed)
    {
        var texture = pressed ? UIResourceManager.ButtonPressed : UIResourceManager.ButtonNormal;
        if (texture != null)
            spriteBatch.Draw(texture, rect, Color.White * Opacity);
        else
            spriteBatch.Draw(UIResourceManager.WhitePixel, rect, (pressed ? new Color(60, 55, 70, 230) : new Color(48, 48, 62, 220)) * Opacity);

        DrawBorder(spriteBatch, rect, new Color(160, 130, 70, 220) * Opacity);

        var font = FontManager.Default;
        if (font == null) return;

        var textSize = FontManager.MeasureString(text, font);
        var textPos = new Vector2(rect.X + (rect.Width - textSize.X) / 2f, rect.Y + (rect.Height - textSize.Y) / 2f);
        FontManager.DrawString(spriteBatch, font, text, textPos, Color.Gold * Opacity);
    }

    private static void DrawBorder(SpriteBatch spriteBatch, Rectangle rect, Color color)
    {
        var pixel = UIResourceManager.WhitePixel;
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), color);
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), color);
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), color);
        spriteBatch.Draw(pixel, new Rectangle(rect.Right - 1, rect.Y, 1, rect.Height), color);
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
            float scale = Math.Min(1f, Math.Min(viewportWidth / (float)windowWidth, viewportHeight / (float)windowHeight));
            int width = (int)Math.Round(windowWidth * scale);
            int height = (int)Math.Round(windowHeight * scale);
            int x = (viewportWidth - width) / 2;
            int y = (viewportHeight - height) / 2;
            return new WindowLayout(new Rectangle(x, y, width, height), scale);
        }

        public Vector2 ToScreen(Vector2 localPosition)
        {
            return new Vector2(Bounds.X + localPosition.X * Scale, Bounds.Y + localPosition.Y * Scale);
        }

        public Point ToLocal(Vector2 screenPosition)
        {
            return new Point((int)((screenPosition.X - Bounds.X) / Scale), (int)((screenPosition.Y - Bounds.Y) / Scale));
        }

        public Rectangle ToScreenRectangle(Rectangle localRect)
        {
            return new Rectangle(
                (int)Math.Round(Bounds.X + localRect.X * Scale),
                (int)Math.Round(Bounds.Y + localRect.Y * Scale),
                (int)Math.Round(localRect.Width * Scale),
                (int)Math.Round(localRect.Height * Scale));
        }
    }
}
