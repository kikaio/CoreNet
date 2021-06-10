using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.Jobs
{
    public class JobOnce : Job
    {
        public JobOnce(DateTime _sd, Action _act)
        {
            StartDate = _sd;
            JobAct = _act;
        }

        //Do Job once after start datetime, and then return false;
        public override bool Tick()
        {
            DateTime nowUtc = DateTime.UtcNow;
            if (StartDate > nowUtc)
                return true;
            JobAct?.Invoke();
            return false;
        }

        public override async Task<bool> TickAsync()
        {
            DateTime nowUtc = DateTime.UtcNow;
            if (StartDate > nowUtc)
                return true;
            await Task.Factory.StartNew(async () =>
            {
                await Task.Delay(10);
                JobAct();
            }, TaskCreationOptions.AttachedToParent);
            return false;
        }
    }

}
