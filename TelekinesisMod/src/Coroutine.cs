using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UniRx;

namespace TelekinesisMod
{
    public static class Coroutine
    {
        public static IEnumerable<object> StartAsSubroutine(IEnumerable<object> subroutine)
        {
            return subroutine.SelectMany(e => e as IEnumerable<object> ?? new[] { e });
        }

        public static IEnumerable<object> Wait(TimeSpan dueTime)
        {
            var millisec = dueTime.TotalMilliseconds;
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.ElapsedMilliseconds < millisec)
                yield return null;

            stopwatch.Stop();
            yield break;
        }

        public static IEnumerable<object> Wait(int frame)
        {
            for (int i = 0; i < frame; i++)
                yield return null;
        }
    }
}