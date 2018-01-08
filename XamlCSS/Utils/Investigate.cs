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

        static StringBuilder sb = new StringBuilder();
        private static int level = 0;
        public static void Measure(this string title, Action action)
        {
            title.Measure(() => { action(); return true; });
        }

        public static T Measure<T>(this string title, Func<T> action)
        {
            var currentLevel = level;
            level++;

            var startTime = stopwatch.ElapsedTicks;
            var result = action();

            var message = $"{new string(' ', currentLevel * 2)}{new TimeSpan(stopwatch.ElapsedTicks - startTime).TotalMilliseconds}ms - " + title;
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
