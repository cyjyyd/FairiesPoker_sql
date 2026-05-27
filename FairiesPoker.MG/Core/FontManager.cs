using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private static GraphicsDevice? _graphicsDevice;
    private static float _currentFontSize;

    private const float BaseFontSize = 20f;
    private const int DefaultAtlasSize = 4096;
    private const int LargeAtlasSize = 8192;

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

        float targetSize = Math.Clamp(BaseFontSize * Math.Max(1f, displayScale), BaseFontSize, 42f);
        if (Math.Abs(targetSize - _currentFontSize) < 0.5f && _fonts.ContainsKey("default"))
            return;

        if (TryBakeDefaultFont(targetSize, LargeAtlasSize) ||
            TryBakeDefaultFont(targetSize, DefaultAtlasSize) ||
            TryBakeDefaultFont(BaseFontSize, DefaultAtlasSize))
        {
            return;
        }
    }

    public static Vector2 MeasureString(string text, SpriteFont? font = null, float scale = 1f)
    {
        var resolvedFont = font ?? Default;
        return resolvedFont.MeasureString(text) * RenderScale * scale;
    }

    public static void DrawString(SpriteBatch spriteBatch, string text, Vector2 position, Color color, float scale = 1f)
    {
        DrawString(spriteBatch, null, text, position, color, scale);
    }

    public static void DrawString(SpriteBatch spriteBatch, SpriteFont? font, string text, Vector2 position, Color color, float scale = 1f)
    {
        var resolvedFont = font ?? Default;
        spriteBatch.DrawString(resolvedFont, text, position, color, 0f, Vector2.Zero, scale * RenderScale, SpriteEffects.None, 0f);
    }

    public static void DrawString(SpriteBatch spriteBatch, SpriteFont? font, string text, Vector2 position, Color color,
        float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
    {
        var resolvedFont = font ?? Default;
        var adjustedOrigin = RenderScale > 0f ? origin / RenderScale : origin;
        spriteBatch.DrawString(resolvedFont, text, position, color, rotation, adjustedOrigin, scale * RenderScale, effects, layerDepth);
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
                    new[]
                    {
                        new CharacterRange((char)0x20, (char)0x7E),
                        new CharacterRange((char)0x2000, (char)0x206F),
                        new CharacterRange((char)0x3000, (char)0x303F),
                        new CharacterRange((char)0x3400, (char)0x4DB5),
                        new CharacterRange((char)0x4E00, (char)0x9FEF),
                        new CharacterRange((char)0xFF00, (char)0xFFEF),
                    });

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
