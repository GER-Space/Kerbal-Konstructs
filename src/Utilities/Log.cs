using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Diagnostics;
using System.Reflection;

namespace KerbalKonstructs.Utilities
{
    internal class Log
    {
        internal static Dictionary<string, Stopwatch> alltimers = new Dictionary<string, Stopwatch>();
        internal static Stopwatch myWatch = null;

        /// <summary>
        /// log a normal message, if dbug is aktivated
        /// </summary>
        /// <param name="message"></param>
        internal static void Debug(string message)
        {
            if (KerbalKonstructs.instance.DebugMode)
                UnityEngine.Debug.Log("KK: " + message);
        }

        /// <summary>
        /// Logs a unity debug message to the User
        /// </summary>
        /// <param name="message"></param>
        internal static void UserInfo(string message)
        {
            UnityEngine.Debug.Log("KK: " + message);
        }

        /// <summary>
        /// Logs a unity Warning message to the User
        /// </summary>
        /// <param name="message"></param>
        internal static void UserWarning(string message)
        {
            UnityEngine.Debug.LogWarning("KK: " + message);
        }

        /// <summary>
        /// Logs a unity Error message to the User
        /// </summary>
        /// <param name="message"></param>
        internal static void UserError(string message)
        {
            UnityEngine.Debug.LogError("KK: " + message);
        }

        /// <summary>
        /// Logs a unity debug message
        /// </summary>
        /// <param name="message"></param>
        internal static void Normal (string message)
        {
#if DEBUG
            UnityEngine.Debug.Log("KK: " + message);
#endif
        }

        /// <summary>
        /// logs a warning message
        /// </summary>
        /// <param name="message"></param>
        internal static void Warning(string message)
        {
#if DEBUG
            UnityEngine.Debug.LogWarning("KK: " + message);
#endif
        }

        /// <summary>
        /// Logs a error message
        /// </summary>
        /// <param name="message"></param>
        internal static void Error(string message)
        {
#if DEBUG
            UnityEngine.Debug.LogError("KK: " + message);
#endif
        }

        /// <summary>
        /// prints the current call-trace to the debug log
        /// </summary>
        internal static void Trace()
        {
            StackTrace t = new StackTrace(); Log.Normal(t.ToString());
        }

        /// <summary>
        /// Starts a Stopwatch timer with the id = string
        /// </summary>
        /// <param name="id"></param>
        internal static void PerfStart(string id = "default")
        {
            myWatch = new Stopwatch();
            alltimers.Add(id, myWatch);
            myWatch.Start();
        }

        /// <summary>
        /// resets the Stopwatchtimer with the (string)id and prints the result.
        /// </summary>
        /// <param name="id"></param>
        internal static void PerfStop(string id = "default")
        {
            if (alltimers.TryGetValue(id, out myWatch))
            {
                myWatch.Stop();
                Log.Normal("Stopwatch: \"" + id + "\" elapsed time: " + myWatch.Elapsed);
                myWatch.Reset();
                alltimers.Remove(id);
            }
        }

        /// <summary>
        /// Pauses the timer with the id
        /// </summary>
        /// <param name="id"></param>
        internal static void PerfPause(string id = "default")
        {
            if (alltimers.TryGetValue(id, out myWatch))
                myWatch.Stop();
        }

        /// <summary>
        /// resumes the timer
        /// </summary>
        /// <param name="id"></param>
        internal static void PerfContinue(string id = "default")
        {
            if (alltimers.TryGetValue(id, out myWatch))
                myWatch.Start();
        }

    }
}
