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
    }
}
