using System;
using System.Runtime.InteropServices;
using FairiesPoker;

namespace FairiesPoker.MG.Core;

/// <summary>
/// 配置管理器 - 替代原有的config.cs INI读取逻辑
/// </summary>
public static class ConfigManager
{
    // 音效开关
    public static bool SoundFX { get; set; } = true;
    public static bool BackMusic { get; set; } = true;

    // UI主题 (1-7)
    public static int UITheme { get; set; } = 5;

    // 窗口设置
    public static int WindowWidth { get; set; } = 1280;
    public static int WindowHeight { get; set; } = 720;
    public static bool FullScreen { get; set; } = false;

    // 网络设置
    public static string ServerIP { get; set; } = "127.0.0.1";
    public static int ServerPort { get; set; } = 40960;

    // 卡牌图像路径
    public static string CardImagePath => System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pokers", UITheme.ToString());

    // 主题资源路径
    public static string ThemePath => System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UI_" + GetThemeSuffix(UITheme));

    // 加载配置
    public static void Load()
    {
        try
        {
            // 复用原有config.cs的读取逻辑
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
            if (!System.IO.File.Exists(path)) return;

            BackMusic = ReadIniData("Settings", "BackMusic", "1", path) == "1";
            SoundFX = ReadIniData("Settings", "SoundFX", "1", path) == "1";
            UITheme = int.Parse(ReadIniData("Settings", "UI", "5", path));
            WindowWidth = int.Parse(ReadIniData("Settings", "Width", "1280", path));
            WindowHeight = int.Parse(ReadIniData("Settings", "Height", "720", path));
            FullScreen = ReadIniData("Settings", "FullScreen", "0", path) == "1";
            ServerIP = ReadIniData("Network", "IP", "127.0.0.1", path);
            string portStr = ReadIniData("Network", "Port", "40960", path);
            if (int.TryParse(portStr, out int port)) ServerPort = port;
        }
        catch
        {
            // 使用默认值
        }
    }

    // 保存配置
    public static void Save()
    {
        try
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
            WriteIniData("Settings", "BackMusic", BackMusic ? "1" : "0", path);
            WriteIniData("Settings", "SoundFX", SoundFX ? "1" : "0", path);
            WriteIniData("Settings", "UI", UITheme.ToString(), path);
            WriteIniData("Settings", "Width", WindowWidth.ToString(), path);
            WriteIniData("Settings", "Height", WindowHeight.ToString(), path);
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

    private static string GetThemeSuffix(int theme)
    {
        return theme switch
        {
            1 => "TB", 2 => "LT", 3 => "FR", 4 => "SW",
            5 => "PF", 6 => "LN", 7 => "PG",
            _ => "PF"
        };
    }
}
