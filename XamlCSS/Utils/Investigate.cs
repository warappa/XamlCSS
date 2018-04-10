#define DEBUG
using System;
using System.Diagnostics;
using System.Text;

namespace XamlCSS.Utils
{
    public static class Investigate
    {
        static Investigate()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();

        }
        static Stopwatch stopwatch;

        static StringBuilder sb = new StringBuilder(50000);
        private static int level = 0;

        [DebuggerStepThrough]
        public static void Measure(this string title, Action action)
        {
#if !INVESTIGATE
            action();
            return;
#endif
            title.Measure(() => { action(); return true; });
        }

        [DebuggerStepThrough]
        public static T Measure<T>(this string title, Func<T> action)
        {
#if !INVESTIGATE
            return action();
#endif
            var currentLevel = level;
            level++;

            var startTime = stopwatch.ElapsedTicks;
            var result = action();

            var message = $"{new string(' ', Math.Max(0, currentLevel) * 4)}{new TimeSpan(stopwatch.ElapsedTicks - startTime).TotalMilliseconds}ms - " + title;
            if (currentLevel < 20)
            {
                lock (sb)
                {
                    //Debug.WriteLine(message);
                    sb.AppendLine(message);
                }
            }
            level--;

            return result;
        }

        public static void Print()
        {
            Debug.WriteLine(sb.ToString());
            sb.Clear();
        }
    }
}
