using System;
using System.Diagnostics;

namespace XamlCSS.Utils
{
    public static class Investigate
    {
        private static int level = 0;
        public static void Measure(this string title, Action action)
        {
            var currentLevel = level;
            level++;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            action();

            stopwatch.Stop();
            Debug.WriteLine($"{new string(' ', currentLevel * 2)}{stopwatch.ElapsedMilliseconds}ms - " + title);

            level--;
        }
    }
}
