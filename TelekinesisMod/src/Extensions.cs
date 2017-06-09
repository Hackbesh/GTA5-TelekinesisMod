using System;

namespace TelekinesisMod
{
    public static class Extensions
    {
        public static T AddTo<T>(this T source, ObservableScript script) where T : IDisposable
        {
            script?.CompositeDisposable.Add(source);
            return source;
        }
    }
}