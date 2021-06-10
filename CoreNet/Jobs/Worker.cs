using CoreNet.Utils.LockHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreNet.Jobs
{
    using JobList = List<Job>;
    public class Worker
    {
        JobList jobList = new JobList();
        JobList delJobList = new JobList();
        Thread thread;
        Locker locker;
        AsyncLocker asyncLocker;
        public string workerName { get; private set; } = "";

        protected bool IsAsyncWorker = false;


        //CoreLogger logger = new ConsoleLogger();
        bool isWorkFin = false;
        public int ThreadId
        {
            get
            {
                return thread == null ? 0 : thread.ManagedThreadId;
            }
        }

        public static Dictionary<int, Worker> WorkerDict { get; private set; } = new Dictionary<int, Worker>();

        public Worker(string _wName = "defaultName", bool isAsyncWorker = false, Locker _locker = null, AsyncLocker _al = null)
        {
            workerName = _wName;
            IsAsyncWorker = isAsyncWorker;
            if (isAsyncWorker == false)
            {
                if (_locker == null)
                    _locker = new Locker(LockMgr.Inst.nextLockId, $"{workerName }'s lock");
                locker = _locker;
            }
            else
            {
                if (_al == null)
                    _al = new AsyncLocker(LockMgr.Inst.nextAsyncLockId, $"{workerName }'s lock");
                asyncLocker = _al;
            }
        }

        public void WorkStart()
        {
            //logger.WriteDebug($"{workerName} is start called");
            if (IsAsyncWorker == false)
            {
                thread = new Thread(Run);
                thread.Start();
            }
            else
            {
                thread = new Thread(async () => { await RunAsync(); });
                thread.Start();
            }
        }

        public void WorkFinish()
        {
            isWorkFin = true;
            try
            {
                thread.Join();
            }
            catch (Exception e)
            {
                //logger.Error(e.ToString());
            }
            //logger.WriteDebug($"{workerName} worker is finished");
        }

        protected void Run()
        {
            Action act = () =>
            {
                foreach (var job in jobList)
                {
                    if (job.Tick() == false)
                        delJobList.Add(job);
                }

                foreach (var delJob in delJobList)
                {
                    jobList.Remove(delJob);
                }
                delJobList.Clear();
            };


            //token으로 처리하는 방식으로 바꿀 것.
            while (isWorkFin == false)
            {
                bool ret = ActWithLockCheck(act);
                if (ret == false)
                    break;
                Thread.Sleep(1);
            }
            //logger.WriteDebug($"Worker[{ThreadId}] is finished");
        }

        protected async Task RunAsync()
        {
            try
            {
                using (var locker = await AsyncLocker.Guard(asyncLocker))
                {
                    if (locker != null)
                    {
                        while (isWorkFin == false)
                        {
                            foreach (var job in jobList)
                            {
                                await Task.Factory.StartNew(async () => {
                                    var ret = await job.TickAsync();
                                    if (ret == false)
                                        delJobList.Add(job);
                                }, TaskCreationOptions.AttachedToParent);
                            }
                            foreach (var del in delJobList)
                            {
                                jobList.Remove(del);
                            }
                            delJobList.Clear();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //logger.Error(e.ToString());
            }
        }
        public void PushJob(Job _newJob)
        {
            if (IsAsyncWorker == false)
            {
                ActWithLockCheck(() =>
                {
                    jobList.Add(_newJob);
                });
            }
            else
            {
                Task.Factory.StartNew(() => { PushJobAsync(_newJob); });
            }
        }

        private async void PushJobAsync(Job _newJob)
        {
            using (var locker = await AsyncLocker.Guard(asyncLocker))
            {
                if (locker != null)
                    jobList.Add(_newJob);
            }
        }

        private bool ActWithLockCheck(Action _act)
        {
            using (var checkedLock = Locker.Guard(locker))
            {
                if (checkedLock == null)
                    return false;
                _act?.Invoke();
            }
            return true;
        }
    }
}
