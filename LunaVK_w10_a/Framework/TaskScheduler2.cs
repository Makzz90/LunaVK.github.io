using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using LunaVK.Core.Framework;

namespace LunaVK.Framework
{
    //VeryLowProfileImageLoader
    public static class TaskScheduler2
    {
        private static readonly Task _task;
        private static readonly object _syncBlock = new object();
        private static readonly Queue<Func<Task>> _pendingRequests = new Queue<Func<Task>>();
        public static bool AllowBoostLoading;
        private static int InProcess;

        private static int MaxSimultaneousLoads
        {
            get { return TaskScheduler2.AllowBoostLoading ? 20 : 5; }
        }

        static TaskScheduler2()
        {
            _task = new Task(WorkerThreadProc);
            _task.Start();
        }

        public static void Add(Func<Task> action)
        {
            _pendingRequests.Enqueue(action);
        }

        public static void Clear()
        {
            InProcess = 0;
            _pendingRequests.Clear();
        }

        private static void WorkerThreadProc()
        {
            while (true)
            {
                lock (_syncBlock)
                {
                    if (_pendingRequests.Count == 0 || InProcess == MaxSimultaneousLoads)//если нет заданий или их и так много
                    {
                        Monitor.Wait(_syncBlock, 500);//то ждём, когда они появятся
                        continue;
                    }

                    /*
                    CancellationTokenSource cts = new CancellationTokenSource();

                    Func<Task> temp = _pendingRequests.Dequeue();
                    Do(temp, cts);
                    try
                    {
                        _task.Wait(cts.Token);
                    }
                    catch (OperationCanceledException e)
                    {
                    }*/
                    if(InProcess<MaxSimultaneousLoads)
                    {
                        Func<Task> temp = _pendingRequests.Dequeue();
                        Do(temp);
                    }
                }
            }
        }
        /*
        private static void Do(Func<Task> task, CancellationTokenSource cts)
        {
            Execute.ExecuteOnUIThread(async () =>
            {
                await task.Invoke();
                cts.Cancel();
            });
        }
        */
        private static void Do(Func<Task> task)
        {
            InProcess++;
            Execute.ExecuteOnUIThread(async () =>
            {
                if (InProcess == 0)
                    return;//мы сбросили задания - не надо вызывать функцию

                await task.Invoke();
                InProcess--;
            });
        }
    }
}
