using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace FairiesPoker.MG.Core;

/// <summary>
/// UI资源管理器 - 管理主题按钮图片、背景图片等UI资源
/// 替代原项目的UI.cs资源加载逻辑
/// </summary>
public static class UIResourceManager
{
    private static GraphicsDevice? _graphicsDevice;
    private static int _currentTheme = 5;

    // 按钮纹理缓存
    private static Texture2D? _btnNormal;
    private static Texture2D? _btnPressed;

    // 预加载的资源
    private static readonly Dictionary<string, Texture2D> _cachedTextures = new();

    public static event Action<int>? ThemeChanged;

    /// <summary>当前主题编号 (1-6)</summary>
    public static int CurrentTheme => _currentTheme;

    /// <summary>主题文件夹名称</summary>
    public static string ThemeFolderName => GetThemeFolderName(_currentTheme);

    /// <summary>主题文件夹完整路径</summary>
    public static string ThemePath => System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ThemeFolderName);

    /// <summary>正常状态按钮纹理</summary>
    public static Texture2D? ButtonNormal => _btnNormal;

    /// <summary>按下状态按钮纹理</summary>
    public static Texture2D? ButtonPressed => _btnPressed;

    /// <summary>
    /// 初始化UI资源管理器
    /// </summary>
    public static void Initialize(GraphicsDevice graphicsDevice, int theme = 5)
    {
        _graphicsDevice = graphicsDevice;
        _currentTheme = theme;
    }

    /// <summary>
    /// 切换主题并重新加载资源
    /// </summary>
    public static void SetTheme(int theme)
    {
        if (theme < 1 || theme > ConfigManager.ThemeCount) theme = 5;
        if (_currentTheme == theme) return;

        _currentTheme = theme;

        // 清除旧缓存
        _btnNormal?.Dispose();
        _btnPressed?.Dispose();
        _btnNormal = null;
        _btnPressed = null;

        // 重新加载
        LoadThemeResources();
        ThemeChanged?.Invoke(_currentTheme);
    }

    /// <summary>
    /// 加载当前主题的资源
    /// </summary>
    public static void LoadThemeResources()
    {
        if (_graphicsDevice == null) return;

        string themePath = ThemePath;

        // 加载按钮图片
        string btn1Path = System.IO.Path.Combine(themePath, "btn1.png");
        string btn2Path = System.IO.Path.Combine(themePath, "btn2.png");

        if (File.Exists(btn1Path))
        {
            try
            {
                using var fs = File.OpenRead(btn1Path);
                _btnNormal = Texture2D.FromStream(_graphicsDevice, fs);
            }
            catch { _btnNormal = null; }
        }

        if (File.Exists(btn2Path))
        {
            try
            {
                using var fs = File.OpenRead(btn2Path);
                _btnPressed = Texture2D.FromStream(_graphicsDevice, fs);
            }
            catch { _btnPressed = null; }
        }
    }

    /// <summary>
    /// 加载Resources文件夹下的资源
    /// </summary>
    public static Texture2D? LoadResource(string resourceName)
    {
        string key = "res_" + resourceName;
        if (_cachedTextures.TryGetValue(key, out var cached))
            return cached;

        if (_graphicsDevice == null) return null;

        string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", resourceName);
        if (!File.Exists(path)) return null;

        try
        {
            using var fs = File.OpenRead(path);
            var texture = Texture2D.FromStream(_graphicsDevice, fs);
            _cachedTextures[key] = texture;
            return texture;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 加载Results文件夹下的结算图片
    /// </summary>
    public static Texture2D? LoadResultImage(string imageName)
    {
        string key = "result_" + _currentTheme + "_" + imageName;
        if (_cachedTextures.TryGetValue(key, out var cached))
            return cached;

        if (_graphicsDevice == null) return null;

        string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Results", _currentTheme.ToString(), imageName);
        if (!File.Exists(path) && _currentTheme != 5)
            path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Results", "5", imageName);
        if (!File.Exists(path)) return null;

        try
        {
            using var fs = File.OpenRead(path);
            var texture = Texture2D.FromStream(_graphicsDevice, fs);
            _cachedTextures[key] = texture;
            return texture;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取主题文件夹名称
    /// </summary>
    private static string GetThemeFolderName(int theme)
    {
        return theme switch
        {
            1 => "UI_TB",
            2 => "UI_LT",
            3 => "UI_FR",
            4 => "UI_SW",
            5 => "UI_PF",
            6 => "UI_LN",
            _ => "UI_PF"
        };
    }

    /// <summary>
    /// 清除所有缓存
    /// </summary>
    public static void Clear()
    {
        _btnNormal?.Dispose();
        _btnPressed?.Dispose();
        _btnNormal = null;
        _btnPressed = null;

        foreach (var tex in _cachedTextures.Values)
            tex.Dispose();
        _cachedTextures.Clear();
    }

    /// <summary>
    /// 创建白色像素纹理(用于绘制矩形)
    /// </summary>
    private static Texture2D? _whitePixel;
    public static Texture2D WhitePixel
    {
        get
        {
            if (_whitePixel != null) return _whitePixel;
            if (_graphicsDevice == null)
                throw new InvalidOperationException("GraphicsDevice not initialized");

            _whitePixel = new Texture2D(_graphicsDevice, 1, 1);
            _whitePixel.SetData(new[] { Microsoft.Xna.Framework.Color.White });
            return _whitePixel;
        }
    }
}
