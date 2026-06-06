using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using IOPath = System.IO.Path;

namespace FairiesPoker.MG.Core;

/// <summary>
/// 配置管理器 - 替代原有的config.cs INI读取逻辑
/// </summary>
public static class ConfigManager
{
    private const string AppName = "FairiesPoker.MG";
    public const int ThemeCount = 6;
    public const int DefaultWindowWidth = 1280;
    public const int DefaultWindowHeight = 720;
    public const string DefaultServerIP = "www.fairybcd.top";
    public const int DefaultServerPort = 40960;

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
    public static string ServerIP { get; set; } = DefaultServerIP;
    public static int ServerPort { get; set; } = DefaultServerPort;

    public static string UserDataDirectory
    {
        get
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrWhiteSpace(appData))
                return AppContext.BaseDirectory;

            return IOPath.Combine(appData, AppName);
        }
    }

    public static string ConfigFilePath => ResolveConfigFilePath();

    // 卡牌图像路径
    public static string DefaultCardImagePath => IOPath.Combine(AppContext.BaseDirectory, "Pokers", "5");
    public static string CardImagePath => ResolveDirectory(
        IOPath.Combine(AppContext.BaseDirectory, "Pokers", UITheme.ToString()),
        DefaultCardImagePath);

    // 主题资源路径
    public static string ThemePath => IOPath.Combine(AppContext.BaseDirectory, "UI_" + GetThemeSuffix(UITheme));
    public static string ThemeMusicPath => ResolveFile(
        IOPath.Combine(ThemePath, "background.mp3"),
        IOPath.Combine(AppContext.BaseDirectory, "UI_PF", "background.mp3"));

    // 加载配置
    public static void Load()
    {
        try
        {
            string path = ConfigFilePath;
            if (!File.Exists(path)) return;

            var data = ReadIniFile(path);
            BackMusic = ParseBool(GetIniValue(data, "Settings", "BackMusic", "1"), true);
            SoundFX = ParseBool(GetIniValue(data, "Settings", "SoundFX", "1"), true);
            BackMusicVolume = ParseFloat(GetIniValue(data, "Settings", "BackMusicVolume", "0.5"), 0.5f);
            SoundFXVolume = ParseFloat(GetIniValue(data, "Settings", "SoundFXVolume", "0.8"), 0.8f);
            UITheme = ParseInt(GetIniValue(data, "Settings", "UI", "5"), 5);
            WindowWidth = ParseInt(GetIniValue(data, "Settings", "Width", GetIniValue(data, "Video", "ScreenWidth", DefaultWindowWidth.ToString())), DefaultWindowWidth);
            WindowHeight = ParseInt(GetIniValue(data, "Settings", "Height", GetIniValue(data, "Video", "ScreenHeight", DefaultWindowHeight.ToString())), DefaultWindowHeight);
            BorderlessWindow = ParseBool(GetIniValue(data, "Settings", "Borderless", "0"), false);
            FullScreen = ParseBool(GetIniValue(data, "Settings", "FullScreen", GetIniValue(data, "Video", "FullScreen", "0")), false);
            ServerIP = GetIniValue(data, "Network", "IP", GetIniValue(data, "Network", "IPAddress", DefaultServerIP));
            ServerPort = ParseInt(GetIniValue(data, "Network", "Port", DefaultServerPort.ToString()), DefaultServerPort);
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
            NormalizeWindowSize();
            string path = ConfigFilePath;
            string? directory = IOPath.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var data = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            SetIniValue(data, "Settings", "BackMusic", BackMusic ? "1" : "0");
            SetIniValue(data, "Settings", "SoundFX", SoundFX ? "1" : "0");
            SetIniValue(data, "Settings", "BackMusicVolume", BackMusicVolume.ToString("0.00", CultureInfo.InvariantCulture));
            SetIniValue(data, "Settings", "SoundFXVolume", SoundFXVolume.ToString("0.00", CultureInfo.InvariantCulture));
            SetIniValue(data, "Settings", "UI", UITheme.ToString(CultureInfo.InvariantCulture));
            SetIniValue(data, "Settings", "Width", WindowWidth.ToString(CultureInfo.InvariantCulture));
            SetIniValue(data, "Settings", "Height", WindowHeight.ToString(CultureInfo.InvariantCulture));
            SetIniValue(data, "Settings", "Borderless", BorderlessWindow ? "1" : "0");
            SetIniValue(data, "Settings", "FullScreen", FullScreen ? "1" : "0");
            SetIniValue(data, "Network", "IP", ServerIP);
            SetIniValue(data, "Network", "Port", ServerPort.ToString(CultureInfo.InvariantCulture));
            WriteIniFile(path, data);
        }
        catch
        {
            // 忽略错误
        }
    }

    public static string GetUserDataPath(string fileName)
    {
        Directory.CreateDirectory(UserDataDirectory);
        return IOPath.Combine(UserDataDirectory, fileName);
    }

    private static string ResolveConfigFilePath()
    {
        string localConfig = IOPath.Combine(AppContext.BaseDirectory, "config.ini");
        if (File.Exists(localConfig))
            return localConfig;

        Directory.CreateDirectory(UserDataDirectory);
        return IOPath.Combine(UserDataDirectory, "config.ini");
    }

    public static void NormalizeWindowSize()
    {
        if (UITheme < 1 || UITheme > ThemeCount) UITheme = 5;
        BackMusicVolume = Clamp01(BackMusicVolume);
        SoundFXVolume = Clamp01(SoundFXVolume);
        if (WindowWidth <= 0) WindowWidth = DefaultWindowWidth;
        if (WindowHeight <= 0) WindowHeight = DefaultWindowHeight;
        if (string.IsNullOrWhiteSpace(ServerIP)) ServerIP = DefaultServerIP;
        if (ServerPort <= 0 || ServerPort > 65535) ServerPort = DefaultServerPort;
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

    private static int ParseInt(string value, int defaultValue)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
            ? parsed
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
        return File.Exists(preferred) ? preferred : fallback;
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

    private static Dictionary<string, Dictionary<string, string>> ReadIniFile(string path)
    {
        var data = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        string currentSection = string.Empty;

        foreach (string rawLine in File.ReadAllLines(path, Encoding.UTF8))
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('#'))
                continue;

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                currentSection = line[1..^1].Trim();
                if (!data.ContainsKey(currentSection))
                    data[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                continue;
            }

            int separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
                continue;

            string key = line[..separatorIndex].Trim();
            string value = line[(separatorIndex + 1)..].Trim();
            SetIniValue(data, currentSection, key, value);
        }

        return data;
    }

    private static void WriteIniFile(string path, Dictionary<string, Dictionary<string, string>> data)
    {
        var lines = new List<string>();
        foreach (var section in data)
        {
            if (lines.Count > 0)
                lines.Add(string.Empty);

            lines.Add($"[{section.Key}]");
            foreach (var pair in section.Value)
            {
                lines.Add($"{pair.Key}={pair.Value}");
            }
        }

        File.WriteAllLines(path, lines, Encoding.UTF8);
    }

    private static string GetIniValue(Dictionary<string, Dictionary<string, string>> data, string section, string key, string defaultValue)
    {
        return data.TryGetValue(section, out var values) && values.TryGetValue(key, out string? value)
            ? value
            : defaultValue;
    }

    private static void SetIniValue(Dictionary<string, Dictionary<string, string>> data, string section, string key, string value)
    {
        if (!data.TryGetValue(section, out var values))
        {
            values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            data[section] = values;
        }

        values[key] = value;
    }
}
