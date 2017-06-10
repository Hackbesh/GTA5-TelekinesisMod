using System;
using UniRx;

namespace TelekinesisMod
{
    public static class Extensions
    {
        public static T AddTo<T>(this T source, ObservableScript script) where T : IDisposable
        {
            script?.CompositeDisposable.Add(source);
            return source;
        }

        public static UniRx.IObservable<TSource> ResetAfter<TSource>(this UniRx.IObservable<TSource> source, TSource defaultValue, TimeSpan dueTime, IScheduler scheduler)
        {
            return new ResetAfterObservable<TSource>(source, defaultValue, dueTime, scheduler);
        }

        public static UniRx.IObservable<TSource> ResetAfter<TSource>(this UniRx.IObservable<TSource> source, TSource defaultValue, TimeSpan dueTime)
        {
            return source.ResetAfter(defaultValue, dueTime, Scheduler.DefaultSchedulers.TimeBasedOperations);
        }

        public static UniRx.IObservable<TSource> ResetAfter<TSource>(this UniRx.IObservable<TSource> source, TimeSpan dueTime)
        {
            return source.ResetAfter(default(TSource), dueTime, Scheduler.DefaultSchedulers.TimeBasedOperations);
        }
    }
}