using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using NAudio.Wave;

namespace FairiesPoker.MG.Core;

/// <summary>
/// 音频管理器 - 替代原有的AudioPlayer.cs + SoundPlayer
/// </summary>
public enum SoundCue
{
    Click,
    Deal,
    Win,
    Lose
}

public class AudioManager : IDisposable
{
    private WaveOutEvent? _bgmOut;
    private AudioFileReader? _bgmReader;
    private bool _bgmLoop;
    private float _bgmVolume = 0.5f;
    private float _sfxVolume = 0.8f;

    // 音效缓存
    private readonly Dictionary<string, SoundEffect> _soundEffects = new();

    // 设置
    public bool BackMusicEnabled { get; set; } = true;
    public bool SoundFXEnabled { get; set; } = true;
    public float BgmVolume
    {
        get => _bgmVolume;
        set
        {
            _bgmVolume = Clamp01(value);
            if (_bgmReader != null)
                _bgmReader.Volume = _bgmVolume;
        }
    }
    public float SfxVolume
    {
        get => _sfxVolume;
        set => _sfxVolume = Clamp01(value);
    }

    public void ApplySettings(bool backMusicEnabled, bool soundFXEnabled, float bgmVolume, float sfxVolume)
    {
        BackMusicEnabled = backMusicEnabled;
        SoundFXEnabled = soundFXEnabled;
        BgmVolume = bgmVolume;
        SfxVolume = sfxVolume;

        if (!BackMusicEnabled)
            StopBgm();
    }

    public void PlayThemeBgm(bool loop = true)
    {
        PlayBgm(ConfigManager.ThemeMusicPath, loop);
    }

    /// <summary>
    /// 播放背景音乐(MP3,支持循环)
    /// </summary>
    public void PlayBgm(string filePath, bool loop = true)
    {
        StopBgm();

        if (!BackMusicEnabled || !File.Exists(filePath)) return;

        try
        {
            _bgmReader = new AudioFileReader(filePath);
            _bgmReader.Volume = BgmVolume;
            _bgmLoop = loop;

            _bgmOut = new WaveOutEvent();
            _bgmOut.PlaybackStopped += (s, e) =>
            {
                if (_bgmLoop && e.Exception == null)
                {
                    _bgmReader?.Seek(0, System.IO.SeekOrigin.Begin);
                    _bgmOut?.Play();
                }
            };
            _bgmOut.Init(_bgmReader);
            _bgmOut.Play();
        }
        catch
        {
            // 忽略音频错误
            StopBgm();
        }
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopBgm()
    {
        try
        {
            _bgmLoop = false;
            _bgmOut?.Stop();
            _bgmOut?.Dispose();
            _bgmReader?.Dispose();
        }
        catch { }
        _bgmOut = null;
        _bgmReader = null;
    }

    /// <summary>
    /// 播放音效(WAV)
    /// </summary>
    public void PlaySfx(SoundCue cue)
    {
        PlaySfx(GetSoundPath(cue));
    }

    public void PlaySfx(string filePath)
    {
        if (!SoundFXEnabled || !File.Exists(filePath)) return;

        try
        {
            if (!_soundEffects.TryGetValue(filePath, out var effect))
            {
                using var fs = System.IO.File.OpenRead(filePath);
                effect = SoundEffect.FromStream(fs);
                _soundEffects[filePath] = effect;
            }
            effect.Play(SfxVolume, 0, 0);
        }
        catch
        {
            // 忽略音频错误
        }
    }

    private static string GetSoundPath(SoundCue cue)
    {
        string fileName = cue switch
        {
            SoundCue.Click => "click.wav",
            SoundCue.Deal => "give.wav",
            SoundCue.Win => "5553.wav",
            SoundCue.Lose => "5538.wav",
            _ => "click.wav"
        };

        return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", fileName);
    }

    private static float Clamp01(float value)
    {
        if (value < 0f) return 0f;
        if (value > 1f) return 1f;
        return value;
    }

    public void Dispose()
    {
        StopBgm();
        foreach (var sfx in _soundEffects.Values)
            sfx.Dispose();
        _soundEffects.Clear();
    }
}
