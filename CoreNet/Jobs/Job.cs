using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.Jobs
{
    public abstract class Job
    {
        //0 : 단 1번 실행. 0 > val : sec지날때마다, 0 < val : 무한반복.
        public DateTime EndDate { get; protected set; } = DateTime.MinValue;
        public DateTime StartDate { get; protected set; } = DateTime.MinValue;
        public DateTime nextDate { get; protected set; }
        protected long deltaTick;
        protected Action JobAct = null;
        protected Task JobTask = null;

        public Job()
        {
        }

        public abstract bool Tick();
        //딱 한번만 실행.
        public virtual async Task<bool> TickAsync()
        {
            throw new NotImplementedException("");
        }
    }
}
