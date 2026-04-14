using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace FairiesPoker.MG.Core;

/// <summary>
/// SpriteFont字体管理器
/// </summary>
public static class FontManager
{
    private static readonly Dictionary<string, SpriteFont> _fonts = new();

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
}
