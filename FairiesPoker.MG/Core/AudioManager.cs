using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using NAudio.Wave;

namespace FairiesPoker.MG.Core;

/// <summary>
/// 音频管理器 - 替代原有的AudioPlayer.cs + SoundPlayer
/// </summary>
public class AudioManager : IDisposable
{
    private WaveOutEvent? _bgmOut;
    private AudioFileReader? _bgmReader;
    private bool _bgmLoop;

    // 音效缓存
    private readonly Dictionary<string, SoundEffect> _soundEffects = new();

    // 设置
    public bool BackMusicEnabled { get; set; } = true;
    public bool SoundFXEnabled { get; set; } = true;
    public float BgmVolume { get; set; } = 0.5f;
    public float SfxVolume { get; set; } = 0.8f;

    /// <summary>
    /// 播放背景音乐(MP3,支持循环)
    /// </summary>
    public void PlayBgm(string filePath, bool loop = true)
    {
        StopBgm();

        if (!BackMusicEnabled || !System.IO.File.Exists(filePath)) return;

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
        }
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopBgm()
    {
        try
        {
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
    public void PlaySfx(string filePath)
    {
        if (!SoundFXEnabled || !System.IO.File.Exists(filePath)) return;

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

    public void Dispose()
    {
        StopBgm();
        foreach (var sfx in _soundEffects.Values)
            sfx.Dispose();
        _soundEffects.Clear();
    }
}
