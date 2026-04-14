using System;
using System.Threading;

namespace FairiesPoker
{
    /// <summary>
    /// 游戏线程管理器 - 使用现代同步机制替代废弃的 Thread.Abort/Suspend/Resume
    /// </summary>
    public class GameThreadManager : IDisposable
    {
        private CancellationTokenSource _cts;
        private ManualResetEventSlim _pauseEvent;
        private Thread _thread;
        private bool _disposed;

        public CancellationToken CancellationToken => _cts?.Token ?? CancellationToken.None;
        public bool IsCancellationRequested => _cts?.IsCancellationRequested ?? false;

        public GameThreadManager()
        {
            _pauseEvent = new ManualResetEventSlim(true);
        }

        /// <summary>
        /// 启动线程
        /// </summary>
        public void Start(ThreadStart threadStart)
        {
            _cts = new CancellationTokenSource();
            _pauseEvent.Set();
            _thread = new Thread(threadStart);
            _thread.Start();
        }

        /// <summary>
        /// 等待线程结束
        /// </summary>
        public void Join()
        {
            _thread?.Join();
        }

        /// <summary>
        /// 暂停线程（等待用户输入）
        /// </summary>
        public void Pause()
        {
            _pauseEvent.Reset();
        }

        /// <summary>
        /// 恢复线程
        /// </summary>
        public void Resume()
        {
            _pauseEvent.Set();
        }

        /// <summary>
        /// 等待恢复信号（在线程内调用）
        /// </summary>
        public void WaitOne()
        {
            _pauseEvent.Wait(_cts.Token);
        }

        /// <summary>
        /// 取消线程
        /// </summary>
        public void Cancel()
        {
            _pauseEvent.Set(); // 先恢复，让线程可以检查取消状态
            _cts?.Cancel();
        }

        /// <summary>
        /// 检查是否应该停止（在线程内调用）
        /// </summary>
        public bool ShouldStop()
        {
            return _cts?.IsCancellationRequested ?? false;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _cts?.Dispose();
                _pauseEvent?.Dispose();
                _disposed = true;
            }
        }
    }
}