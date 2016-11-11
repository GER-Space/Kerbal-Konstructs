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
        internal static void Debug(string message)
        {
            if (KerbalKonstructs.instance.DebugMode)
                UnityEngine.Debug.Log("KK: " + message);
        }

        internal static void Normal (string message)
        {
#if DEBUG
            UnityEngine.Debug.Log("KK: " + message);
#endif
        }

        internal static void Warning(string message)
        {
#if DEBUG
            UnityEngine.Debug.LogWarning("KK: " + message);
#endif
        }
        internal static void Error(string message)
        {
#if DEBUG
            UnityEngine.Debug.LogError("KK: " + message);
#endif
        }

        // unused Code: Marked for Deleteion
        private const int baseFrameOffset = 3;

        private static string GetStackFrameString(int skipFrames)
        {
            StackFrame frame = new StackFrame(skipFrames);
            MethodBase method = frame.GetMethod();

            return string.Concat(method.DeclaringType.Name, ".", method.Name);
        }

        public static void Write(string message)
        {
#if DEBUG
            Log.Write(message, baseFrameOffset);
#endif
        }

        private static void Write(string message, int skipFrames)
        {
#if DEBUG
            UnityEngine.Debug.Log(string.Format("[KK {0}]: {1}", Log.GetStackFrameString(skipFrames), message));
#endif
        }

        public static void Write(string format, params object[] args)
        {
#if DEBUG
            Log.Write(baseFrameOffset + 1, format, args);
#endif
        }

        private static void Write(int skipFrames, string format, params object[] args)
        {
#if DEBUG
            Log.Write(string.Format(format, args), skipFrames);
#endif
        }

        public static void WriteIf(bool test, string message)
        {
#if DEBUG
            Log.WriteIf(test, message, baseFrameOffset + 1);
#endif
        }

        private static void WriteIf(bool test, string message, int skipFrames)
        {
#if DEBUG
            if (test)
            {
                Log.Write(message, skipFrames);
            }
#endif
        }

        public static void WriteIf(bool test, string format, params object[] args)
        {
#if DEBUG
            Log.WriteIf(test, baseFrameOffset + 2, format, args);
#endif
        }

        private static void WriteIf(bool test, int skipFrames, string format, params object[] args)
        {
#if DEBUG
            if (test)
            {
                Log.Write(skipFrames, format, args);
            }
#endif
        }

        public static void WriteIfNull(object o, string message)
        {
#if DEBUG
            Log.WriteIfNull(o, message, baseFrameOffset + 2);
#endif
        }

        private static void WriteIfNull(object o, string message, int skipFrames)
        {
#if DEBUG
            Log.WriteIf(o == null, message, skipFrames);
#endif
        }

        public static void WriteIfNull(object o, string format, params object[] args)
        {
#if DEBUG
            Log.WriteIfNull(o, baseFrameOffset + 3, format, args);
#endif
        }

        private static void WriteIfNull(object o, int skipFrames, string format, params object[] args)
        {
#if DEBUG
            Log.WriteIf(o == null, skipFrames, format, args);
#endif
        }
    }
}
