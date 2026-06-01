using System;
using System.Globalization;
using System.Runtime.InteropServices;
using FairiesPoker;

namespace FairiesPoker.MG.Core;

/// <summary>
/// 配置管理器 - 替代原有的config.cs INI读取逻辑
/// </summary>
public static class ConfigManager
{
    public const int ThemeCount = 6;
    public const int DefaultWindowWidth = 1280;
    public const int DefaultWindowHeight = 720;

    public static readonly (int Width, int Height)[] ResolutionPresets =
    {
        (1280, 720),
        (1920, 1080),
        (2560, 1440)
    };

    // 音效开关
    public static bool SoundFX { get; set; } = true;
    public static bool BackMusic { get; set; } = true;
    public static float SoundFXVolume { get; set; } = 0.8f;
    public static float BackMusicVolume { get; set; } = 0.5f;

    // UI主题 (1-6)
    public static int UITheme { get; set; } = 5;

    // 窗口设置
    public static int WindowWidth { get; set; } = DefaultWindowWidth;
    public static int WindowHeight { get; set; } = DefaultWindowHeight;
    public static bool BorderlessWindow { get; set; } = false;
    public static bool FullScreen { get; set; } = false;

    // 网络设置
    public static string ServerIP { get; set; } = "127.0.0.1";
    public static int ServerPort { get; set; } = 40960;

    // 卡牌图像路径
    public static string DefaultCardImagePath => System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pokers", "5");
    public static string CardImagePath => ResolveDirectory(
        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pokers", UITheme.ToString()),
        DefaultCardImagePath);

    // 主题资源路径
    public static string ThemePath => System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UI_" + GetThemeSuffix(UITheme));
    public static string ThemeMusicPath => ResolveFile(
        System.IO.Path.Combine(ThemePath, "background.mp3"),
        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UI_PF", "background.mp3"));

    // 加载配置
    public static void Load()
    {
        try
        {
            // 复用原有config.cs的读取逻辑
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
            if (!System.IO.File.Exists(path)) return;

            BackMusic = ParseBool(ReadIniData("Settings", "BackMusic", "1", path), true);
            SoundFX = ParseBool(ReadIniData("Settings", "SoundFX", "1", path), true);
            BackMusicVolume = ParseFloat(ReadIniData("Settings", "BackMusicVolume", "0.5", path), 0.5f);
            SoundFXVolume = ParseFloat(ReadIniData("Settings", "SoundFXVolume", "0.8", path), 0.8f);
            UITheme = int.Parse(ReadIniData("Settings", "UI", "5", path));
            WindowWidth = int.Parse(ReadIniData("Settings", "Width", "1280", path));
            WindowHeight = int.Parse(ReadIniData("Settings", "Height", "720", path));
            BorderlessWindow = ReadIniData("Settings", "Borderless", "0", path) == "1";
            FullScreen = ReadIniData("Settings", "FullScreen", "0", path) == "1";
            ServerIP = ReadIniData("Network", "IP", "127.0.0.1", path);
            string portStr = ReadIniData("Network", "Port", "40960", path);
            if (int.TryParse(portStr, out int port)) ServerPort = port;
        }
        catch
        {
            // 使用默认值
        }

        NormalizeWindowSize();
    }

    // 保存配置
    public static void Save()
    {
        try
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
            WriteIniData("Settings", "BackMusic", BackMusic ? "1" : "0", path);
            WriteIniData("Settings", "SoundFX", SoundFX ? "1" : "0", path);
            WriteIniData("Settings", "BackMusicVolume", BackMusicVolume.ToString("0.00", CultureInfo.InvariantCulture), path);
            WriteIniData("Settings", "SoundFXVolume", SoundFXVolume.ToString("0.00", CultureInfo.InvariantCulture), path);
            WriteIniData("Settings", "UI", UITheme.ToString(), path);
            WriteIniData("Settings", "Width", WindowWidth.ToString(), path);
            WriteIniData("Settings", "Height", WindowHeight.ToString(), path);
            WriteIniData("Settings", "Borderless", BorderlessWindow ? "1" : "0", path);
            WriteIniData("Settings", "FullScreen", FullScreen ? "1" : "0", path);
            WriteIniData("Network", "IP", ServerIP, path);
            WriteIniData("Network", "Port", ServerPort.ToString(), path);
        }
        catch
        {
            // 忽略错误
        }
    }

    // INI读取 - 复用原有config.cs的P/Invoke逻辑
    [System.Runtime.InteropServices.DllImport("kernel32")]
    private static extern int GetPrivateProfileString(string section, string key, string def,
        System.Text.StringBuilder retVal, int size, string filePath);

    [System.Runtime.InteropServices.DllImport("kernel32")]
    private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

    private static string ReadIniData(string section, string key, string def, string path)
    {
        var ret = new System.Text.StringBuilder(1024);
        GetPrivateProfileString(section, key, def, ret, 1024, path);
        return ret.ToString();
    }

    private static void WriteIniData(string section, string key, string val, string path)
    {
        WritePrivateProfileString(section, key, val, path);
    }

    public static void NormalizeWindowSize()
    {
        if (UITheme < 1 || UITheme > ThemeCount) UITheme = 5;
        BackMusicVolume = Clamp01(BackMusicVolume);
        SoundFXVolume = Clamp01(SoundFXVolume);
        if (WindowWidth <= 0) WindowWidth = DefaultWindowWidth;
        if (WindowHeight <= 0) WindowHeight = DefaultWindowHeight;
        if (FullScreen) BorderlessWindow = false;
    }

    private static bool ParseBool(string value, bool defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value)) return defaultValue;
        if (value == "1") return true;
        if (value == "0") return false;
        return bool.TryParse(value, out bool parsed) ? parsed : defaultValue;
    }

    private static float ParseFloat(string value, float defaultValue)
    {
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed)
            ? Clamp01(parsed)
            : defaultValue;
    }

    private static float Clamp01(float value)
    {
        if (value < 0f) return 0f;
        if (value > 1f) return 1f;
        return value;
    }

    private static string ResolveDirectory(string preferred, string fallback)
    {
        return System.IO.Directory.Exists(preferred) ? preferred : fallback;
    }

    private static string ResolveFile(string preferred, string fallback)
    {
        return System.IO.File.Exists(preferred) ? preferred : fallback;
    }

    private static string GetThemeSuffix(int theme)
    {
        return theme switch
        {
            1 => "TB", 2 => "LT", 3 => "FR", 4 => "SW",
            5 => "PF", 6 => "LN",
            _ => "PF"
        };
    }
}
