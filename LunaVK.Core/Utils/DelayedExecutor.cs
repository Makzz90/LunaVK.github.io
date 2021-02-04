using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LunaVK.Core.Utils
{
    public class DelayedExecutor
    {
        private object m_lockObj = new object();
        private DelayedExecutor.ExecutionInfo m_executionInfo;
        private Timer m_timer;
        private int m_delay;

        public bool IsActive { get; private set; }

        public void Cancel()
        {
            this.ChangeTimer(false);
        }

        public DelayedExecutor(int delay)
        {
            this.m_delay = delay;
            this.m_timer = new Timer(new TimerCallback(this.TimerCallback),null,0, delay);
        }

        public void AddToDelayedExecution(Action action)
        {
            lock (this.m_lockObj)
                this.m_executionInfo = new DelayedExecutor.ExecutionInfo()
                {
                    Action = action,
                    Timestamp = DateTime.Now
                };
            this.ChangeTimer(true);
        }

        private void TimerCallback(object state)
        {
            Action action = null;
            lock (this.m_lockObj)
            {
                if (this.m_executionInfo != null)
                {
                    if (DateTime.Now - this.m_executionInfo.Timestamp >= TimeSpan.FromMilliseconds((double)this.m_delay))
                    {
                        action = this.m_executionInfo.Action;
                        this.m_executionInfo = null;
                        this.ChangeTimer(false);
                    }
                }
            }
            if (action == null)
                return;
            try
            {
                action();
            }
            catch (Exception ex)
            {
                //Logger.Instance.Error("Exeption during delayed execution", ex);
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

        private class ExecutionInfo
        {
            public Action Action { get; set; }

            public DateTime Timestamp { get; set; }
        }
    }
}
