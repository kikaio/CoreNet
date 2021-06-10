using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreNet.Utils.LockHelper
{
    public class LockMgr
    {
        public static LockMgr Inst { get; private set; } = new LockMgr();
        int curLockId = 1;

        public int nextLockId
        {
            get
            {
                return Interlocked.Increment(ref curLockId);
            }
        }

        public bool CheckDeadLock(Locker _l)
        {
            if (_l == null)
                return false;
            if (_l.threadId == 0)
                return false;
            int curThreadId = Thread.CurrentThread.ManagedThreadId;
            if (_l.threadId == curThreadId)
                return false;
            return true;
        }

        int curAsyncLockId = 1;
        public int nextAsyncLockId
        {
            get
            {
                return Interlocked.Increment(ref curAsyncLockId);
            }
        }

        public bool CheckDeadLock(AsyncLocker _al)
        {
            if (_al == null)
                return false;
            if (_al.threadId == 0)
                return false;
            int curId = Thread.CurrentThread.ManagedThreadId;
            if (_al.threadId == curId)
                return false;
            return true;
        }
    }
}
