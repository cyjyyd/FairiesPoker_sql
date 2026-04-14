using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace FairiesPoker.MG.Core;

/// <summary>
/// 纹理管理器 - 加载和缓存Texture2D(卡牌/按钮/背景等)
/// </summary>
public static class TextureManager
{
    private static GraphicsDevice? _graphicsDevice;
    private static readonly Dictionary<string, Texture2D> _textures = new();

    public static void Initialize(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
    }

    /// <summary>
    /// 从文件加载纹理(自动缓存)
    /// </summary>
    public static Texture2D Load(string key, string filePath)
    {
        if (_textures.TryGetValue(key, out var existing)) return existing;

        if (_graphicsDevice == null || !System.IO.File.Exists(filePath))
            return Texture2D.FromFile(_graphicsDevice!, filePath);

        try
        {
            using var fs = System.IO.File.OpenRead(filePath);
            var texture = Texture2D.FromStream(_graphicsDevice, fs);
            _textures[key] = texture;
            return texture;
        }
        catch
        {
            // 返回一个1x1白色占位纹理
            return GetPlaceholder();
        }
    }

    /// <summary>
    /// 加载已创建的纹理(用于程序生成的纹理)
    /// </summary>
    public static void LoadInternal(string key, Texture2D texture)
    {
        _textures[key] = texture;
    }

    /// <summary>
    /// 获取已缓存的纹理
    /// </summary>
    public static Texture2D? Get(string key)
    {
        return _textures.GetValueOrDefault(key);
    }

    /// <summary>
    /// 清除缓存
    /// </summary>
    public static void Clear()
    {
        foreach (var t in _textures.Values)
            t.Dispose();
        _textures.Clear();
    }

    /// <summary>
    /// 按主题前缀清除卡牌纹理缓存
    /// </summary>
    public static void ClearCardTextures(string themePrefix)
    {
        var keysToRemove = _textures.Keys.Where(k => k.StartsWith(themePrefix)).ToList();
        foreach (var key in keysToRemove)
        {
            _textures[key].Dispose();
            _textures.Remove(key);
        }
    }

    private static Texture2D? _placeholder;
    private static Texture2D GetPlaceholder()
    {
        if (_placeholder != null) return _placeholder;
        if (_graphicsDevice == null) return Texture2D.FromFile(_graphicsDevice!, "");
        _placeholder = new Texture2D(_graphicsDevice, 1, 1);
        _placeholder.SetData(new[] { Microsoft.Xna.Framework.Color.White });
        return _placeholder;
    }
}
