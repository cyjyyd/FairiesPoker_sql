using System;
using System.IO;
using NAudio.Wave;

namespace FairiesPoker
{
    /// <summary>
    /// 音频播放器辅助类，使用 NAudio 实现 MP3 播放
    /// </summary>
    public class AudioPlayer : IDisposable
    {
        private IWavePlayer waveOut;
        private AudioFileReader audioFileReader;
        private string filePath;
        private bool isLoop;
        private bool isDisposed;

        public AudioPlayer()
        {
        }

        /// <summary>
        /// 播放音频文件
        /// </summary>
        /// <param name="path">音频文件路径</param>
        /// <param name="loop">是否循环播放</param>
        public void Play(string path, bool loop = false)
        {
            Stop();

            if (!File.Exists(path))
                return;

            filePath = path;
            isLoop = loop;

            try
            {
                waveOut = new WaveOutEvent();
                audioFileReader = new AudioFileReader(path);
                waveOut.Init(audioFileReader);
                waveOut.PlaybackStopped += OnPlaybackStopped;
                waveOut.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"音频播放错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void Stop()
        {
            if (waveOut != null)
            {
                waveOut.PlaybackStopped -= OnPlaybackStopped;
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }

            if (audioFileReader != null)
            {
                audioFileReader.Dispose();
                audioFileReader = null;
            }
        }

        /// <summary>
        /// 播放停止事件处理
        /// </summary>
        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (isLoop && !isDisposed)
            {
                // 循环播放：重置位置并重新播放
                if (audioFileReader != null && waveOut != null)
                {
                    audioFileReader.Position = 0;
                    waveOut.Play();
                }
            }
        }

        public void Dispose()
        {
            isDisposed = true;
            Stop();
        }
    }
}