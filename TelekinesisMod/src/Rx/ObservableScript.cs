using System;
using System.Windows.Forms;
using GTA;
using UniRx;

namespace TelekinesisMod
{
    public abstract class ObservableScript : Script
    {
        private ScriptScheduler scheduler = new ScriptScheduler();

        public UniRx.IObservable<Unit> AbortedAsObservable { get; }

        public UniRx.IObservable<Unit> TickAsObservable { get; }

        public UniRx.IObservable<KeyEventArgs> KeyDownAsObservable { get; }

        public UniRx.IObservable<KeyEventArgs> KeyUpAsObservable { get; }

        public IScheduler Scheduler => scheduler;

        public CoroutineCore Coroutine { get; } = new CoroutineCore();

        public CompositeDisposable CompositeDisposable { get; } = new CompositeDisposable();

        protected ObservableScript()
        {
            AbortedAsObservable =
                Observable.FromEventPattern<EventHandler, EventArgs>(h => h.Invoke, h => Aborted += h, h => Aborted -= h)
                .Select(_ => Unit.Default)
                .Publish()
                .RefCount();

            TickAsObservable =
                Observable.FromEventPattern<EventHandler, EventArgs>(h => h.Invoke, h => Tick += h, h => Tick -= h)
                .Select(_ => Unit.Default)
                .Publish()
                .RefCount();

            KeyDownAsObservable =
                Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(h => h.Invoke, h => KeyDown += h, h => KeyDown -= h)
                .Select(e => e.EventArgs)
                .Publish()
                .RefCount();

            KeyUpAsObservable =
                Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(h => h.Invoke, h => KeyDown += h, h => KeyDown -= h)
                .Select(e => e.EventArgs)
                .Publish()
                .RefCount();

            TickAsObservable
                .Subscribe(_ =>
                {
                    scheduler.Run();
                    Coroutine.Run();
                })
                .AddTo(CompositeDisposable);
        }

        ~ObservableScript()
        {
            CompositeDisposable.Dispose();
        }
    }
}