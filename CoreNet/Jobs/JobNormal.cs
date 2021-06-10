using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.Jobs
{
    public class JobNormal : Job
    {
        public JobNormal(DateTime sDt, DateTime eDt, long _deltaTicks, Action _act)
        {
            StartDate = sDt;
            EndDate = eDt;
            deltaTick = _deltaTicks;
            JobAct = _act;
        }

        public override bool Tick()
        {
            var curDt = DateTime.UtcNow;
            if (StartDate > curDt)
                return true;
            if (EndDate < curDt)
                return false;
            if (nextDate > curDt)
                return true;

            JobAct?.Invoke();
            nextDate = curDt.Add(new TimeSpan(deltaTick));
            return true;
        }

        public override async Task<bool> TickAsync()
        {
            var curDt = DateTime.UtcNow;
            if (JobTask == null)
                return false;
            if (StartDate > curDt)
                return true;
            if (EndDate < curDt)
                return false;
            if (nextDate > curDt)
                return true;

            await JobTask;
            nextDate = curDt.Add(new TimeSpan(deltaTick));
            return true;
        }
    }

}
