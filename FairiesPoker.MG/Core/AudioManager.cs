using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

    private readonly object _sfxLock = new();
    private readonly List<SfxPlayback> _activeSfx = new();

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
        set
        {
            _sfxVolume = Clamp01(value);
            lock (_sfxLock)
            {
                foreach (var sfx in _activeSfx)
                    sfx.Reader.Volume = _sfxVolume;
            }
        }
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
        PlaySfx(GetSoundPath(cue), GetCueGain(cue));
    }

    public void PlaySfx(string filePath)
    {
        PlaySfx(filePath, 1f);
    }

    private void PlaySfx(string filePath, float gain)
    {
        if (!SoundFXEnabled || !File.Exists(filePath)) return;

        SfxPlayback? playback = null;
        bool addedToActiveList = false;
        try
        {
            var reader = new AudioFileReader(filePath)
            {
                Volume = ClampVolume(SfxVolume * gain)
            };
            var output = new WaveOutEvent();
            playback = new SfxPlayback(output, reader);

            output.Init(reader);
            output.PlaybackStopped += (_, _) => DisposeSfxPlayback(playback);

            lock (_sfxLock)
            {
                _activeSfx.Add(playback);
                addedToActiveList = true;
            }

            output.Play();
        }
        catch (Exception ex)
        {
            if (playback != null)
            {
                if (addedToActiveList)
                    DisposeSfxPlayback(playback);
                else
                    playback.Dispose();
            }
            Debug.WriteLine($"Failed to play sound effect '{filePath}': {ex.Message}");
        }
    }

    private void DisposeSfxPlayback(SfxPlayback playback)
    {
        lock (_sfxLock)
        {
            _activeSfx.Remove(playback);
        }

        playback.Dispose();
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

    private static float GetCueGain(SoundCue cue)
    {
        return cue switch
        {
            SoundCue.Deal => 1.6f,
            SoundCue.Click => 1.2f,
            _ => 1f
        };
    }

    private static float ClampVolume(float value)
    {
        if (value < 0f) return 0f;
        if (value > 2f) return 2f;
        return value;
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
        lock (_sfxLock)
        {
            foreach (var sfx in _activeSfx.ToArray())
                sfx.Dispose();
            _activeSfx.Clear();
        }
    }

    private sealed class SfxPlayback : IDisposable
    {
        public SfxPlayback(WaveOutEvent output, AudioFileReader reader)
        {
            Output = output;
            Reader = reader;
        }

        public WaveOutEvent Output { get; }
        public AudioFileReader Reader { get; }

        public void Dispose()
        {
            try
            {
                Output.Stop();
                Output.Dispose();
                Reader.Dispose();
            }
            catch
            {
            }
        }
    }
}
