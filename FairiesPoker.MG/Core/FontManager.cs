using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;

namespace FairiesPoker.MG.Core;

/// <summary>
/// SpriteFont字体管理器
/// </summary>
public static class FontManager
{
    private static readonly Dictionary<string, SpriteFont> _fonts = new();
    private static readonly Dictionary<string, Texture2D> _emojiTextures = new();
    private static readonly Dictionary<string, GlyphTexture> _fallbackGlyphTextures = new();
    private static readonly HashSet<char> _bakedCharacters = BuildInitialCharacterSet();
    private static GraphicsDevice? _graphicsDevice;
    private static float _currentFontSize;

    private const float BaseFontSize = 20f;
    private const float MaxFontSize = 32f;
    private const int DefaultAtlasSize = 2048;
    private const int LargeAtlasSize = 4096;
    private const int FallbackGlyphTextureSize = 64;
    private const float FallbackGlyphFontSize = 48f;

    private const string CommonChineseText =
        "斗地主单人模式多人模式联机模式设置退出返回登录注册用户名密码确认头像大厅房间创建加入快速匹配准备取消开始离开刷新在线玩家聊天发送系统消息" +
        "胜利失败结算继续地主农民叫地主抢地主不叫不抢出牌不出提示托管重新开始剩余底牌我的上家下家左边右边等待连接服务器端口断开错误成功失败" +
        "主题天波龙女芙蓉素问霹雳琳琅背景音乐音效音量窗口全屏无边框保存应用默认恢复返回主菜单请输入暂无加载中游戏结束分数积分倍数春天炸弹王炸顺子连对飞机三带一对" +
        "黑桃红桃梅花方块大小王一二三四五六七八九十零百千万上下左右中高低大小是否已未当前回合轮到玩家房主观战重连掉线自动通过拒绝审核列表测试";

    /// <summary>
    /// 当前默认字体相对于1280x720设计字体的绘制补偿。
    /// 高分辨率下字体会重新烘焙得更大，绘制时用该比例保持虚拟坐标尺寸不变。
    /// </summary>
    public static float RenderScale => _currentFontSize > 0f ? BaseFontSize / _currentFontSize : 1f;

    public static void Initialize(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        UpdateForDisplayScale(DisplayManager.Scale);
    }

    public static void Load(string key, SpriteFont font)
    {
        _fonts[key] = font;
    }

    public static SpriteFont? Get(string key)
    {
        return _fonts.GetValueOrDefault(key);
    }

    /// <summary>
    /// 默认字体
    /// </summary>
    public static SpriteFont Default => _fonts.TryGetValue("default", out var f) ? f : _fonts.Values.First();

    public static void UpdateForDisplayScale(float displayScale)
    {
        if (_graphicsDevice == null) return;

        float targetSize = Math.Clamp(BaseFontSize * Math.Max(1f, displayScale), BaseFontSize, MaxFontSize);
        if (Math.Abs(targetSize - _currentFontSize) < 0.5f && _fonts.ContainsKey("default"))
            return;

        if (TryBakeDefaultFont(targetSize, DefaultAtlasSize) ||
            TryBakeDefaultFont(targetSize, LargeAtlasSize) ||
            TryBakeDefaultFont(BaseFontSize, DefaultAtlasSize))
        {
            return;
        }
    }

    public static Vector2 MeasureString(string text, SpriteFont? font = null, float scale = 1f)
    {
        var resolvedFont = font ?? Default;
        if (NeedsInlineTextureRendering(text))
            return MeasureStringWithInlineTextures(text, resolvedFont, scale);

        try
        {
            return resolvedFont.MeasureString(text) * RenderScale * scale;
        }
        catch (ArgumentException)
        {
            return resolvedFont.MeasureString(SanitizeForSpriteFont(text)) * RenderScale * scale;
        }
    }

    public static void DrawString(SpriteBatch spriteBatch, string text, Vector2 position, Color color, float scale = 1f)
    {
        DrawString(spriteBatch, null, text, position, color, scale);
    }

    public static void DrawString(SpriteBatch spriteBatch, SpriteFont? font, string text, Vector2 position, Color color, float scale = 1f)
    {
        var resolvedFont = font ?? Default;
        if (NeedsInlineTextureRendering(text))
        {
            DrawStringWithInlineTextures(spriteBatch, resolvedFont, text, position, color, scale);
            return;
        }

        try
        {
            spriteBatch.DrawString(resolvedFont, text, position, color, 0f, Vector2.Zero, scale * RenderScale, SpriteEffects.None, 0f);
        }
        catch (ArgumentException)
        {
            spriteBatch.DrawString(resolvedFont, SanitizeForSpriteFont(text), position, color, 0f, Vector2.Zero, scale * RenderScale, SpriteEffects.None, 0f);
        }
    }

    public static void DrawString(SpriteBatch spriteBatch, SpriteFont? font, string text, Vector2 position, Color color,
        float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
    {
        var resolvedFont = font ?? Default;
        var adjustedOrigin = RenderScale > 0f ? origin / RenderScale : origin;
        try
        {
            spriteBatch.DrawString(resolvedFont, text, position, color, rotation, adjustedOrigin, scale * RenderScale, effects, layerDepth);
        }
        catch (ArgumentException)
        {
            spriteBatch.DrawString(resolvedFont, SanitizeForSpriteFont(text), position, color, rotation, adjustedOrigin, scale * RenderScale, effects, layerDepth);
        }
    }

    internal static bool ContainsEmoji(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        var enumerator = StringInfo.GetTextElementEnumerator(text);
        while (enumerator.MoveNext())
        {
            if (IsEmojiTextElement((string)enumerator.GetTextElement()))
                return true;
        }

        return false;
    }

    private static bool NeedsInlineTextureRendering(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        var enumerator = StringInfo.GetTextElementEnumerator(text);
        while (enumerator.MoveNext())
        {
            if (NeedsTextureForTextElement((string)enumerator.GetTextElement()))
                return true;
        }

        return false;
    }

    private static bool NeedsTextureForTextElement(string textElement)
    {
        return IsEmojiTextElement(textElement) || !IsBakedTextElement(textElement);
    }

    private static bool IsBakedTextElement(string textElement)
    {
        if (IsLineBreak(textElement))
            return true;

        if (string.IsNullOrEmpty(textElement) || textElement.Length != 1)
            return false;

        return IsKnownBakedCharacter(textElement[0]);
    }

    private static Vector2 MeasureStringWithInlineTextures(string text, SpriteFont font, float scale)
    {
        float lineHeight = GetScaledLineHeight(font, scale);
        float x = 0f;
        float y = 0f;
        float maxX = 0f;
        var textRun = new StringBuilder();

        void FlushTextRun()
        {
            if (textRun.Length == 0)
                return;

            x += MeasureTextRun(font, textRun.ToString()).X * RenderScale * scale;
            textRun.Clear();
        }

        var enumerator = StringInfo.GetTextElementEnumerator(text);
        while (enumerator.MoveNext())
        {
            string element = (string)enumerator.GetTextElement();
            if (IsLineBreak(element))
            {
                FlushTextRun();
                maxX = Math.Max(maxX, x);
                x = 0f;
                y += lineHeight;
                continue;
            }

            if (NeedsTextureForTextElement(element))
            {
                FlushTextRun();
                x += GetInlineTextureAdvance(font, scale);
                continue;
            }

            textRun.Append(element);
        }

        FlushTextRun();
        maxX = Math.Max(maxX, x);
        return new Vector2(maxX, y + lineHeight);
    }

    private static void DrawStringWithInlineTextures(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, float scale)
    {
        float lineHeight = GetScaledLineHeight(font, scale);
        float inlineTextureSize = GetInlineTextureAdvance(font, scale);
        var cursor = position;
        var textRun = new StringBuilder();

        void FlushTextRun()
        {
            if (textRun.Length == 0)
                return;

            string run = textRun.ToString();
            DrawTextRun(spriteBatch, font, run, cursor, color, scale);
            cursor.X += MeasureTextRun(font, run).X * RenderScale * scale;
            textRun.Clear();
        }

        var enumerator = StringInfo.GetTextElementEnumerator(text);
        while (enumerator.MoveNext())
        {
            string element = (string)enumerator.GetTextElement();
            if (IsLineBreak(element))
            {
                FlushTextRun();
                cursor.X = position.X;
                cursor.Y += lineHeight;
                continue;
            }

            if (NeedsTextureForTextElement(element))
            {
                FlushTextRun();
                var texture = IsEmojiTextElement(element)
                    ? GetEmojiTexture(element)
                    : GetFallbackGlyphTexture(element)?.Texture;
                if (texture != null)
                {
                    var rect = new Rectangle(
                        (int)Math.Round(cursor.X),
                        (int)Math.Round(cursor.Y + Math.Max(0f, (lineHeight - inlineTextureSize) / 2f)),
                        (int)Math.Ceiling(inlineTextureSize),
                        (int)Math.Ceiling(inlineTextureSize));
                    spriteBatch.Draw(texture, rect, color);
                    cursor.X += inlineTextureSize;
                }
                else
                {
                    textRun.Append('?');
                }
                continue;
            }

            textRun.Append(element);
        }

        FlushTextRun();
    }

    private static void DrawTextRun(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, float scale)
    {
        try
        {
            spriteBatch.DrawString(font, text, position, color, 0f, Vector2.Zero, scale * RenderScale, SpriteEffects.None, 0f);
        }
        catch (ArgumentException)
        {
            spriteBatch.DrawString(font, SanitizeForSpriteFont(text), position, color, 0f, Vector2.Zero, scale * RenderScale, SpriteEffects.None, 0f);
        }
    }

    private static Vector2 MeasureTextRun(SpriteFont font, string text)
    {
        try
        {
            return font.MeasureString(text);
        }
        catch (ArgumentException)
        {
            return font.MeasureString(SanitizeForSpriteFont(text));
        }
    }

    private static float GetScaledLineHeight(SpriteFont font, float scale)
    {
        float lineHeight = font.LineSpacing > 0 ? font.LineSpacing : font.MeasureString("M").Y;
        return Math.Max(1f, lineHeight * RenderScale * scale);
    }

    private static float GetInlineTextureAdvance(SpriteFont font, float scale)
    {
        return GetScaledLineHeight(font, scale);
    }

    private static bool IsLineBreak(string textElement)
    {
        return textElement == "\n" || textElement == "\r" || textElement == "\r\n";
    }

    private static bool IsEmojiTextElement(string textElement)
    {
        if (string.IsNullOrEmpty(textElement))
            return false;

        bool hasEmojiPresentation = textElement.Contains('\uFE0F') || textElement.Contains('\u200D');
        for (int i = 0; i < textElement.Length; i++)
        {
            int codePoint;
            char c = textElement[i];
            if (char.IsHighSurrogate(c))
            {
                if (i + 1 >= textElement.Length || !char.IsLowSurrogate(textElement[i + 1]))
                    continue;

                codePoint = char.ConvertToUtf32(c, textElement[++i]);
            }
            else if (char.IsLowSurrogate(c))
            {
                continue;
            }
            else
            {
                codePoint = c;
            }

            if (codePoint >= 0x1F000 && codePoint <= 0x1FAFF)
                return true;

            if (hasEmojiPresentation && codePoint >= 0x2600 && codePoint <= 0x27BF)
                return true;
        }

        return false;
    }

    private static Texture2D? GetEmojiTexture(string emoji)
    {
        if (_graphicsDevice == null)
            return null;

        if (_emojiTextures.TryGetValue(emoji, out var cached))
            return cached;

        try
        {
            const int size = 64;
            using var bitmap = new System.Drawing.Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using var graphics = System.Drawing.Graphics.FromImage(bitmap);
            graphics.Clear(System.Drawing.Color.Transparent);
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            using var font = new System.Drawing.Font("Segoe UI Emoji", 48f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            using var brush = new System.Drawing.SolidBrush(System.Drawing.Color.White);
            using var format = new System.Drawing.StringFormat
            {
                Alignment = System.Drawing.StringAlignment.Center,
                LineAlignment = System.Drawing.StringAlignment.Center
            };

            graphics.DrawString(emoji, font, brush, new System.Drawing.RectangleF(0f, -2f, size, size + 4f), format);

            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;

            var texture = Texture2D.FromStream(_graphicsDevice, ms);
            _emojiTextures[emoji] = texture;
            return texture;
        }
        catch
        {
            return null;
        }
    }

    private static GlyphTexture? GetFallbackGlyphTexture(string textElement)
    {
        if (_graphicsDevice == null || string.IsNullOrEmpty(textElement))
            return null;

        if (_fallbackGlyphTextures.TryGetValue(textElement, out var cached))
            return cached;

        try
        {
            using var bitmap = new System.Drawing.Bitmap(
                FallbackGlyphTextureSize,
                FallbackGlyphTextureSize,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using var graphics = System.Drawing.Graphics.FromImage(bitmap);
            graphics.Clear(System.Drawing.Color.Transparent);
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            using var font = new System.Drawing.Font(
                "Microsoft YaHei",
                FallbackGlyphFontSize,
                System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Pixel);
            using var brush = new System.Drawing.SolidBrush(System.Drawing.Color.White);
            using var format = new System.Drawing.StringFormat
            {
                Alignment = System.Drawing.StringAlignment.Center,
                LineAlignment = System.Drawing.StringAlignment.Center
            };

            graphics.DrawString(
                textElement,
                font,
                brush,
                new System.Drawing.RectangleF(0f, -2f, FallbackGlyphTextureSize, FallbackGlyphTextureSize + 4f),
                format);

            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;

            var texture = Texture2D.FromStream(_graphicsDevice, ms);
            var glyph = new GlyphTexture(texture);
            _fallbackGlyphTextures[textElement] = glyph;
            return glyph;
        }
        catch
        {
            return null;
        }
    }

    private static string SanitizeForSpriteFont(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var sb = new StringBuilder(text.Length);
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (char.IsHighSurrogate(c))
            {
                sb.Append('?');
                if (i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                    i++;
                continue;
            }

            if (char.IsLowSurrogate(c) || IsVariationSelector(c))
                continue;

            sb.Append(IsKnownBakedCharacter(c) ? c : '?');
        }

        return sb.ToString();
    }

    private static bool IsKnownBakedCharacter(char c)
    {
        return c == '\n' || c == '\r' || _bakedCharacters.Contains(c);
    }

    private static bool IsVariationSelector(char c)
    {
        return c >= 0xFE00 && c <= 0xFE0F;
    }

    private static HashSet<char> BuildInitialCharacterSet()
    {
        var chars = new HashSet<char>();
        AddCharacterRange(chars, 0x20, 0x7E);
        AddCharacterRange(chars, 0x2000, 0x206F);
        AddCharacterRange(chars, 0x3000, 0x303F);
        AddCharacterRange(chars, 0xFF00, 0xFFEF);

        foreach (char c in CommonChineseText)
        {
            if (!char.IsControl(c))
                chars.Add(c);
        }

        return chars;
    }

    private static void AddCharacterRange(HashSet<char> chars, int start, int end)
    {
        for (int codePoint = start; codePoint <= end; codePoint++)
        {
            chars.Add((char)codePoint);
        }
    }

    private static CharacterRange[] BuildCharacterRanges(IEnumerable<char> chars)
    {
        var ordered = chars
            .Where(c => !char.IsControl(c))
            .Distinct()
            .OrderBy(c => c)
            .ToArray();

        if (ordered.Length == 0)
            return Array.Empty<CharacterRange>();

        var ranges = new List<CharacterRange>();
        char rangeStart = ordered[0];
        char previous = ordered[0];

        for (int i = 1; i < ordered.Length; i++)
        {
            char current = ordered[i];
            if (current == previous + 1)
            {
                previous = current;
                continue;
            }

            ranges.Add(new CharacterRange(rangeStart, previous));
            rangeStart = current;
            previous = current;
        }

        ranges.Add(new CharacterRange(rangeStart, previous));
        return ranges.ToArray();
    }

    public static void Clear()
    {
        foreach (var font in _fonts.Values)
        {
            font.Texture.Dispose();
        }
        _fonts.Clear();

        foreach (var texture in _emojiTextures.Values)
        {
            texture.Dispose();
        }
        _emojiTextures.Clear();

        foreach (var glyph in _fallbackGlyphTextures.Values)
        {
            glyph.Texture.Dispose();
        }
        _fallbackGlyphTextures.Clear();

        _currentFontSize = 0f;
    }

    private sealed class GlyphTexture
    {
        public GlyphTexture(Texture2D texture)
        {
            Texture = texture;
        }

        public Texture2D Texture { get; }
    }

    private static bool TryBakeDefaultFont(float size, int atlasSize)
    {
        if (_graphicsDevice == null) return false;

        string[] fontFiles =
        {
            "C:\\WINDOWS\\Fonts\\msyh.ttc",
            "C:\\WINDOWS\\Fonts\\STKAITI.TTF",
            "C:\\WINDOWS\\Fonts\\simhei.ttf",
            "C:\\WINDOWS\\Fonts\\STXIHEI.TTF",
            "C:\\WINDOWS\\Fonts\\SIMYOU.TTF",
        };

        foreach (var ttfPath in fontFiles)
        {
            if (!File.Exists(ttfPath)) continue;

            try
            {
                var result = TtfFontBaker.Bake(
                    File.ReadAllBytes(ttfPath),
                    size,
                    atlasSize,
                    atlasSize,
                    BuildCharacterRanges(_bakedCharacters));

                var spriteFont = result.CreateSpriteFont(_graphicsDevice);
                if (spriteFont.MeasureString("测试").X <= 10) continue;

                if (_fonts.TryGetValue("default", out var oldFont))
                {
                    oldFont.Texture.Dispose();
                }

                _fonts["default"] = spriteFont;
                _currentFontSize = size;
                return true;
            }
            catch
            {
                // Try the next font or atlas size.
            }
        }

        return false;
    }
}
