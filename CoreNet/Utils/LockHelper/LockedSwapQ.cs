using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.Utils.LockHelper
{
    public class LockedSwapQ<T>
    {
        Queue<T>[] doubleQueue = new Queue<T>[2]{
            new Queue<T>(), new Queue<T>()
        };

        private int popQueueIdx = 0;
        private int pushQueueIdx = 1;
        private int curPopQueueCnt = 0;
        private object cs = new object();

        public void Push(T _newEle)
        {
            doubleQueue[pushQueueIdx].Enqueue(_newEle);
        }

        public T pop()
        {
            if (curPopQueueCnt == 0)
                return default(T);
            var ele = doubleQueue[popQueueIdx].Peek();
            if (ele != null)
            {
                doubleQueue[popQueueIdx].Dequeue();
                curPopQueueCnt--;
                return ele;
            }
            return default(T);
        }

        public void Swap()
        {
            lock (cs)
            {
                int tmp = popQueueIdx;
                popQueueIdx = pushQueueIdx;
                pushQueueIdx = tmp;
                curPopQueueCnt = doubleQueue[popQueueIdx].Count;
            }
        }
    }

}
