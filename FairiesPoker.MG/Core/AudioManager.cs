using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using IOPath = System.IO.Path;

namespace FairiesPoker.MG.Core;

/// <summary>
/// Cross-platform audio manager backed by MonoGame audio APIs.
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
    private readonly Dictionary<string, SoundEffect> _sfxCache = new(StringComparer.OrdinalIgnoreCase);
    private Song? _bgmSong;
#if ANDROID
    private global::Android.Media.MediaPlayer? _androidBgmPlayer;
#endif
    private float _bgmVolume = 0.5f;
    private float _sfxVolume = 0.8f;

    public bool BackMusicEnabled { get; set; } = true;
    public bool SoundFXEnabled { get; set; } = true;

    public float BgmVolume
    {
        get => _bgmVolume;
        set
        {
            _bgmVolume = Clamp01(value);
            TrySetMediaPlayerVolume(_bgmVolume);
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

    public void PlayBgm(string filePath, bool loop = true)
    {
        StopBgm();

        filePath = ConfigManager.ResolveResourcePath(filePath);
        if (!BackMusicEnabled || !File.Exists(filePath))
            return;

#if ANDROID
        PlayAndroidBgm(filePath, loop);
#else
        try
        {
            _bgmSong = Song.FromUri(IOPath.GetFileNameWithoutExtension(filePath), CreateFileUri(filePath));
            TrySetMediaPlayerRepeating(loop);
            TrySetMediaPlayerVolume(BgmVolume);
            MediaPlayer.Play(_bgmSong);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to play background music '{filePath}': {ex.Message}");
            StopBgm();
        }
#endif
    }

    public void StopBgm()
    {
#if ANDROID
        StopAndroidBgm();
#else
        try
        {
            MediaPlayer.Stop();
        }
        catch
        {
        }

        _bgmSong?.Dispose();
        _bgmSong = null;
#endif
    }

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
        if (!SoundFXEnabled || !File.Exists(filePath))
            return;

        try
        {
            SoundEffect effect = GetOrLoadSfx(filePath);
            effect.Play(Clamp01(SfxVolume * gain), 0f, 0f);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to play sound effect '{filePath}': {ex.Message}");
        }
    }

    private SoundEffect GetOrLoadSfx(string filePath)
    {
        string fullPath = IOPath.GetFullPath(filePath);
        if (_sfxCache.TryGetValue(fullPath, out var cached))
            return cached;

        using var stream = File.OpenRead(fullPath);
        var effect = SoundEffect.FromStream(stream);
        _sfxCache[fullPath] = effect;
        return effect;
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

        return ConfigManager.ResolveResourcePath(IOPath.Combine("Resources", fileName));
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

    private static Uri CreateFileUri(string filePath)
    {
        string fullPath = IOPath.GetFullPath(filePath).Replace('\\', '/');
        if (!fullPath.StartsWith("/", StringComparison.Ordinal))
            fullPath = "/" + fullPath;

        return new Uri("file://" + fullPath);
    }

    private static void TrySetMediaPlayerRepeating(bool loop)
    {
#if ANDROID
        _ = loop;
#else
        try
        {
            MediaPlayer.IsRepeating = loop;
        }
        catch
        {
        }
#endif
    }

    private void TrySetMediaPlayerVolume(float volume)
    {
#if ANDROID
        TrySetAndroidBgmVolume(volume);
#else
        try
        {
            MediaPlayer.Volume = volume;
        }
        catch
        {
        }
#endif
    }

#if ANDROID
    private void PlayAndroidBgm(string filePath, bool loop)
    {
        try
        {
            StopAndroidBgm();
            var player = new global::Android.Media.MediaPlayer();
            player.SetDataSource(filePath);
            player.Looping = loop;
            player.SetVolume(BgmVolume, BgmVolume);
            player.Prepare();
            player.Start();
            _androidBgmPlayer = player;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to play Android background music '{filePath}': {ex.Message}");
            StopAndroidBgm();
        }
    }

    private void StopAndroidBgm()
    {
        var player = _androidBgmPlayer;
        _androidBgmPlayer = null;
        if (player == null)
            return;

        try
        {
            if (player.IsPlaying)
                player.Stop();
        }
        catch
        {
        }

        try
        {
            player.Release();
        }
        catch
        {
        }

        player.Dispose();
    }

    private void TrySetAndroidBgmVolume(float volume)
    {
        try
        {
            _androidBgmPlayer?.SetVolume(volume, volume);
        }
        catch
        {
        }
    }
#endif

    private static float Clamp01(float value)
    {
        if (value < 0f) return 0f;
        if (value > 1f) return 1f;
        return value;
    }

    public void Dispose()
    {
        StopBgm();
        foreach (var effect in _sfxCache.Values)
        {
            effect.Dispose();
        }
        _sfxCache.Clear();
    }
}
