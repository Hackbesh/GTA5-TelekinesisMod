using System;
using System.Collections.Generic;
using UniRx;

namespace TelekinesisMod
{
    public class ScriptScheduler : IScheduler
    {
        private readonly object lockObject = new object();

        private Queue<ScheduledItem> queue = new Queue<ScheduledItem>();

        public DateTimeOffset Now => Scheduler.Now;

        public IDisposable Schedule(Action action)
        {
            var item = new ScheduledItem(action);

            lock (lockObject)
            {
                queue.Enqueue(item);
            }

            return item.Cancellation;
        }

        public IDisposable Schedule(TimeSpan dueTime, Action action)
            => Scheduler.ThreadPool.Schedule(dueTime, action);

        public void Run()
        {
            lock (lockObject)
            {
                while (queue.Count > 0)
                {
                    queue.Dequeue().Invoke();
                }
            }
        }
    }

    internal class ScheduledItem
    {
        private Action action;

        private BooleanDisposable disposable = new BooleanDisposable();

        internal IDisposable Cancellation => disposable;

        internal ScheduledItem(Action action)
        {
            this.action = action;
        }

        internal void Invoke()
        {
            if (!disposable.IsDisposed)
            {
                action?.Invoke();
                disposable.Dispose();
            }
        }
    }
}