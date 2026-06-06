using System;
using FairiesPoker.MG.Core;
using FairiesPoker.MG.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FairiesPoker.MG.Screens;

public class CardPreviewScreen : ScreenBase
{
    private const int WindowWidth = 650;
    private const int WindowHeight = 550;

    private static readonly Rectangle CardRect = new(24, 60, 277, 413);
    private static readonly Rectangle CloseButtonRect = new(494, 486, 120, 38);
    private static readonly string[] RankLabels = { "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A", "2" };
    private static readonly (string Label, string Huase, int Size)[] SuitOptions =
    {
        ("方块", "fangkuai", 0),
        ("梅花", "meihua", 0),
        ("红桃", "hongtao", 0),
        ("黑桃", "heitao", 0),
        ("小王", "", 16),
        ("大王", "", 17)
    };

    private int _suitIndex;
    private int _rankIndex;
    private int _pressedSuitIndex = -1;
    private int _pressedRankIndex = -1;
    private bool _closePressed;

    public CardPreviewScreen(Game1 game, ScreenManager screenManager)
        : base(game, screenManager)
    {
        Opacity = 0f;
    }

    public override void Update(GameTime gameTime)
    {
        FadeIn(0.05);
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var layout = WindowLayout.Create(DisplayManager.DesignWidth, DisplayManager.DesignHeight, WindowWidth, WindowHeight);

        spriteBatch.Draw(UIResourceManager.WhitePixel, new Rectangle(0, 0, DisplayManager.DesignWidth, DisplayManager.DesignHeight),
            new Color(0, 0, 0, 150) * Opacity);
        DrawPanel(spriteBatch, layout.Bounds);

        DrawTitle(spriteBatch, layout);
        DrawCurrentCard(spriteBatch, layout);
        DrawSuitButtons(spriteBatch, layout);
        DrawRankButtons(spriteBatch, layout);
        DrawButton(spriteBatch, layout.ToScreenRectangle(CloseButtonRect), "退出", _closePressed, false);
    }

    public override void HandleInput(InputManager input)
    {
        if (input.KeyPressed(Keys.Escape))
        {
            ScreenManager.Pop();
            return;
        }

        var layout = WindowLayout.Create(DisplayManager.DesignWidth, DisplayManager.DesignHeight, WindowWidth, WindowHeight);
        var localMousePos = layout.ToLocal(input.MousePosition);

        int hoveredSuit = GetHoveredSuitIndex(localMousePos);
        int hoveredRank = GetHoveredRankIndex(localMousePos);
        bool closeHovered = CloseButtonRect.Contains(localMousePos);

        if (input.LeftMouseClicked)
        {
            _pressedSuitIndex = hoveredSuit;
            _pressedRankIndex = hoveredRank;
            _closePressed = closeHovered;
        }

        if (input.LeftMouseReleased)
        {
            if (_pressedSuitIndex >= 0 && _pressedSuitIndex == hoveredSuit)
            {
                _suitIndex = hoveredSuit;
                if (IsJokerSelected())
                    _rankIndex = 0;
                Game.AudioManager?.PlaySfx(SoundCue.Click);
            }
            else if (!IsJokerSelected() && _pressedRankIndex >= 0 && _pressedRankIndex == hoveredRank)
            {
                _rankIndex = hoveredRank;
                Game.AudioManager?.PlaySfx(SoundCue.Click);
            }
            else if (_closePressed && closeHovered)
            {
                Game.AudioManager?.PlaySfx(SoundCue.Click);
                ScreenManager.Pop();
            }

            _pressedSuitIndex = -1;
            _pressedRankIndex = -1;
            _closePressed = false;
        }
    }

    private void DrawTitle(SpriteBatch spriteBatch, WindowLayout layout)
    {
        var font = FontManager.Default;
        if (font == null) return;

        FontManager.DrawString(spriteBatch, font, "图片欣赏", layout.ToScreen(new Vector2(24, 20)),
            Color.Gold * Opacity, 1.2f * layout.Scale);

        string label = IsJokerSelected()
            ? SuitOptions[_suitIndex].Label
            : $"{SuitOptions[_suitIndex].Label} {RankLabels[_rankIndex]}";
        FontManager.DrawString(spriteBatch, font, $"牌面预览：{label}", layout.ToScreen(new Vector2(330, 64)),
            Color.WhiteSmoke * Opacity, layout.Scale);
    }

    private void DrawCurrentCard(SpriteBatch spriteBatch, WindowLayout layout)
    {
        var selected = SuitOptions[_suitIndex];
        int size = selected.Size > 0 ? selected.Size : _rankIndex + 3;
        var texture = CardRenderer.GetCardTexture(selected.Huase, size);
        spriteBatch.Draw(texture, layout.ToScreenRectangle(CardRect), Color.White * Opacity);
    }

    private void DrawSuitButtons(SpriteBatch spriteBatch, WindowLayout layout)
    {
        for (int i = 0; i < SuitOptions.Length; i++)
        {
            var rect = layout.ToScreenRectangle(GetSuitRect(i));
            bool selected = i == _suitIndex;
            bool pressed = i == _pressedSuitIndex;
            DrawButton(spriteBatch, rect, SuitOptions[i].Label, pressed, selected);
        }
    }

    private void DrawRankButtons(SpriteBatch spriteBatch, WindowLayout layout)
    {
        for (int i = 0; i < RankLabels.Length; i++)
        {
            var rect = layout.ToScreenRectangle(GetRankRect(i));
            bool selected = !IsJokerSelected() && i == _rankIndex;
            bool pressed = i == _pressedRankIndex;
            DrawButton(spriteBatch, rect, RankLabels[i], pressed, selected && !IsJokerSelected(), IsJokerSelected());
        }
    }

    private int GetHoveredSuitIndex(Point point)
    {
        for (int i = 0; i < SuitOptions.Length; i++)
        {
            if (GetSuitRect(i).Contains(point))
                return i;
        }

        return -1;
    }

    private int GetHoveredRankIndex(Point point)
    {
        for (int i = 0; i < RankLabels.Length; i++)
        {
            if (GetRankRect(i).Contains(point))
                return i;
        }

        return -1;
    }

    private static Rectangle GetSuitRect(int index)
    {
        int column = index % 2;
        int row = index / 2;
        return new Rectangle(330 + column * 98, 112 + row * 42, 86, 34);
    }

    private static Rectangle GetRankRect(int index)
    {
        int column = index % 4;
        int row = index / 4;
        return new Rectangle(330 + column * 70, 270 + row * 42, 58, 34);
    }

    private bool IsJokerSelected()
    {
        return SuitOptions[_suitIndex].Size > 0;
    }

    private void DrawPanel(SpriteBatch spriteBatch, Rectangle rect)
    {
        spriteBatch.Draw(UIResourceManager.WhitePixel, rect, new Color(24, 28, 38, 238) * Opacity);
        DrawBorder(spriteBatch, rect, new Color(210, 170, 85, 220) * Opacity);
    }

    private void DrawButton(SpriteBatch spriteBatch, Rectangle rect, string text, bool pressed, bool selected, bool disabled = false)
    {
        Color bg = disabled
            ? new Color(40, 40, 46, 145)
            : selected
                ? new Color(115, 86, 34, 230)
                : pressed ? new Color(60, 55, 70, 230) : new Color(48, 48, 62, 220);

        spriteBatch.Draw(UIResourceManager.WhitePixel, rect, bg * Opacity);
        DrawBorder(spriteBatch, rect, (selected ? Color.Gold : new Color(160, 130, 70, 220)) * Opacity);

        var font = FontManager.Default;
        if (font == null) return;

        Color textColor = disabled ? Color.Gray : Color.Gold;
        var textSize = FontManager.MeasureString(text, font);
        var textPos = new Vector2(rect.X + (rect.Width - textSize.X) / 2f, rect.Y + (rect.Height - textSize.Y) / 2f);
        FontManager.DrawString(spriteBatch, font, text, textPos, textColor * Opacity);
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
