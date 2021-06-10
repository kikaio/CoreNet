using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreNet.Utils.LockHelper
{
    public class Locker : IDisposable
    {
        public long threadId { get; private set; }
        public long lockId { get; private set; }
        public string methodName { get; private set; }
        public int lineNo { get; private set; }
        public string lockName { get; private set; }

        private object lockObj = new object();

        public Locker(long _lockId, string _name)
        {
            lockName = _name;
            lockId = _lockId;
        }

        internal void SetLockData(int _tId, string _mName, int _lineNo)
        {
            threadId = _tId;
            methodName = _mName;
            lineNo = _lineNo;
        }

        internal void BeginLock()
        {
            Monitor.Enter(lockObj);
        }

        void EndLock()
        {
            Monitor.Exit(lockObj);
            methodName = "";
            lineNo = 0;
            threadId = 0;
        }

        public void Dispose()
        {
            EndLock();
        }

        public static Locker Guard(Locker _lock, [CallerMemberName]string _mName = "", [CallerLineNumber]int _lineNo = 0)
        {
            int curThreadId = Thread.CurrentThread.ManagedThreadId;
            if (LockMgr.Inst.CheckDeadLock(_lock))
            {
                //ConsoleLogger logger = new ConsoleLogger();
                //logger.Error($"locker[{_lock.lockId}] is deadlock in {_mName}:{_lineNo}");
                //logger.Error($"thread[{_lock.threadId}] VS thread[{curThreadId }]");
                return null;
            }
            try
            {
                _lock.BeginLock();
                _lock.SetLockData(curThreadId, _mName, _lineNo);
                return _lock;
            }
            catch (Exception _e)
            {
                //ConsoleLogger logger = new ConsoleLogger();
                //logger.Error($"error from {_mName}::{_lineNo} by using lock guard->{_e}");
            }

            return null;
        }
    }
}
