using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreNet.Utils.LockHelper
{
    public class AsyncLocker : IDisposable
    {
        public long threadId { get; private set; }
        public long lockId { get; private set; }
        public string methodName { get; private set; }
        public int lineNo { get; private set; }
        public string lockName { get; private set; }

        private bool IsDisposed = false;

        private SemaphoreSlim ss = new SemaphoreSlim(1, 1);

        public AsyncLocker(long _lockId, string _lockName)
        {
            lockId = _lockId;
            lockName = _lockName;
        }

        internal void SetLockData(int _tId, string _mName, int _lineNo)
        {
            threadId = _tId;
            methodName = _mName;
            lineNo = _lineNo;
        }

        internal async Task BeginLock()
        {
            await ss.WaitAsync();
        }

        public void Dispose(bool _isDispose)
        {
            if (_isDispose)
            {
            }
            Dispose();
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            ss.Release();
        }

        public static async Task<AsyncLocker> Guard(AsyncLocker _lock = null, [CallerMemberName] string _mName = "", [CallerLineNumber] int _lineNo = 0)
        {
            int curId = Thread.CurrentThread.ManagedThreadId;
            try
            {
                _lock.SetLockData(curId, _mName, _lineNo);
                await _lock.BeginLock();
                return _lock;
            }
            catch (Exception _e)
            {
                //CoreLogger logger = new ConsoleLogger();
                //logger.Error($"error from {_mName}::{_lineNo} by using lock guard->{_e.ToString()}");
            }

            return null;
        }
    }
}
