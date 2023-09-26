using System;
using System.Diagnostics;

namespace Optimization
{
    public static class SimpleBenchmark
    {
        public static void Execute(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action.Invoke();
            stopwatch.Stop();
            UnityEngine.Debug.Log($"{action.Method.Name} | {stopwatch.ElapsedMilliseconds} ms");
        }

        public static void Execute<T1>(Action<T1> action, T1 argument)
        {
            var stopwatch = Stopwatch.StartNew();
            action.Invoke(argument);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"{action.Method.Name} | {stopwatch.ElapsedMilliseconds} ms");
        }

        public static TResult Execute<TResult>(Func<TResult> function)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = function.Invoke();
            stopwatch.Stop();
            UnityEngine.Debug.Log($"{function.Method.Name} | {stopwatch.ElapsedMilliseconds} ms");
            return result;
        }

        public static TResult Execute<T1, TResult>(Func<T1, TResult> function, T1 argument)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = function.Invoke(argument);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"{function.Method.Name} | {stopwatch.ElapsedMilliseconds} ms");
            return result;
        }

        public static T3 Execute<T1, T2, T3>(Func<T1, T2, T3> function, T1 argument1, T2 argument2)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = function.Invoke(argument1, argument2);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"{function.Method.Name} | {stopwatch.ElapsedMilliseconds} ms");
            return result;
        }
    }
}