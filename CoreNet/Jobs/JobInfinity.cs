using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.Jobs
{
    public class JobInfinity : Job
    {
        public JobInfinity(Action _jobAct = null, DateTime _sDate = default(DateTime), long _deltaTicks = 0)
        {
            if (_sDate == default(DateTime))
                StartDate = DateTime.UtcNow;
            JobAct = _jobAct;
            deltaTick = _deltaTicks;
        }
        public override bool Tick()
        {
            //일정시간 이후에 작업 수행 후 시작 시점 다시 지정한 간격만큼 이동.
            if (StartDate > DateTime.UtcNow)
                return true;
            if (JobAct == null)
                return false;
            JobAct?.Invoke();
            StartDate = StartDate.AddTicks(deltaTick);
            return true;
        }

        public override async Task<bool> TickAsync()
        {
            if (StartDate > DateTime.UtcNow)
                return true;
            if (JobAct == null)
                return false;

            await Task.Factory.StartNew(JobAct, TaskCreationOptions.AttachedToParent);
            StartDate = StartDate.AddTicks(deltaTick);
            return true;
        }
    }
}
