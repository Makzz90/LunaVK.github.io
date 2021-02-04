using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LunaVK.Core.Utils
{
    public class DelayedExecutorWithQueue
    {
        private Queue<DelayedExecutorWithQueue.ExecutionInfo> m_executionInfoQueue = new Queue<DelayedExecutorWithQueue.ExecutionInfo>();
        private object m_lockObj = new object();
        private DelayedExecutorWithQueue.ExecutionInfo _currentExecution;
        private Timer m_timer;
        private int m_delay;
        private Func<DelayedExecutorWithQueue.ExecutionInfo, bool> _allowExecute;

        public bool IsActive { get; private set; }

        public DelayedExecutorWithQueue(int delay, Func<DelayedExecutorWithQueue.ExecutionInfo, bool> allowExecution)
        {
            this.m_delay = delay;
            this.m_timer = new Timer(new TimerCallback(this.TimerCallback),null,0, this.m_delay);
            this._allowExecute = allowExecution;
        }

        public void AddToDelayedExecutionQueue(Action action/*, string name*/)
        {
            lock (this.m_lockObj)
                this.m_executionInfoQueue.Enqueue(new DelayedExecutorWithQueue.ExecutionInfo()
                {
                    Action = action,
                    TimestampAdded = DateTime.Now,
                    //Name = name
                });
            this.ChangeTimer(true);
        }

        private void TimerCallback(object state)
        {
            lock (this.m_lockObj)
            {
                if (this._currentExecution != null && (DateTime.Now - this._currentExecution.TimestampCompleted).TotalMilliseconds <= this.m_delay)
                    return;
                if (this.m_executionInfoQueue.Count == 0)
                {
                    this.ChangeTimer(false);
                }
                else
                {
                    if (!this._allowExecute(this.m_executionInfoQueue.Peek()))
                        return;
                    this._currentExecution = this.m_executionInfoQueue.Dequeue();
                    this._currentExecution.TimestampStarted = DateTime.Now;
                    try
                    {
                        this._currentExecution.Action();
                        this._currentExecution.TimestampCompleted = DateTime.Now;
                    }
                    catch (Exception ex)
                    {
                        //Logger.Instance.Error("DelayedExecutorWithQueue failed to execute" + ex);
                    }
                }
            }
        }

        private void ChangeTimer(bool activate)
        {
            if (activate && !this.IsActive)
            {
                lock (this.m_timer)
                {
                    this.IsActive = true;
                    this.m_timer.Change(this.m_delay, this.m_delay);
                }
            }
            else
            {
                if (activate || !this.IsActive)
                    return;
                lock (this.m_timer)
                {
                    this.IsActive = false;
                    this.m_timer.Change(-1, 0);
                }
            }
        }

        public class ExecutionInfo
        {
            public Action Action { get; set; }
            
            public DateTime TimestampAdded { get; set; }

            public DateTime TimestampStarted { get; set; }

            public DateTime TimestampCompleted { get; set; }

            public ExecutionInfo()
            {
                this.TimestampCompleted = DateTime.MaxValue;
            }
        }
    }
}
