using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;


namespace KerbalKonstructs.Core
{
    class AsmUtilsOld
    {

        public class Detour
        {
            #region Constants and Fields

            // The pointer to the new function.
            private readonly IntPtr newFunctionPointer;
            // The pointer to the original function.
            private readonly IntPtr oldFunctionPointer;


            // asm code for the detour
            private readonly byte[] newBytes;
            // The original asm code of the function
            private readonly byte[] originalBytes;

            // The method of the original function
            private readonly MethodBase origFuction;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="Detour"/> class.
            /// </summary>
            /// <param name="oldFunction">
            ///   The target.
            /// </param>
            /// <param name="newFunction">
            ///   The improved function.
            /// </param>
            public unsafe Detour(MethodBase oldFunction, MethodBase newFunction)
            {
                // error out on null arguments
                if (oldFunction == null)
                {
                    Log.Error("Source MethodInfo is null");
                    return;
                }

                if (newFunction == null)
                {
                    Log.Error("Destination MethodInfo is null");
                    return;
                }

                this.origFuction = oldFunction;

                this.oldFunctionPointer = oldFunction.MethodHandle.GetFunctionPointer();
                this.newFunctionPointer = newFunction.MethodHandle.GetFunctionPointer();


                if (IntPtr.Size == sizeof(Int64))
                {

                    // 64-bit systems use 64-bit absolute address and jumps
                    // 12 byte destructive

                    // Get function pointers

                    byte[] destinationByes = BitConverter.GetBytes(this.newFunctionPointer.ToInt64());

                    this.originalBytes = new byte[12];
                    this.newBytes = new byte[12];
                    //Marshal.Copy(this.oldFunctionPointer, this.originalBytes, 0, 12);
                    this.originalBytes = Memory.Read(oldFunctionPointer, 12);

                    newBytes[0] = 0x48;
                    newBytes[1] = 0xB8;
                    newBytes[2] = destinationByes[0];
                    newBytes[3] = destinationByes[1];
                    newBytes[4] = destinationByes[2];
                    newBytes[5] = destinationByes[3];
                    newBytes[6] = destinationByes[4];
                    newBytes[7] = destinationByes[5];
                    newBytes[8] = destinationByes[6];
                    newBytes[9] = destinationByes[7];
                    newBytes[10] = 0xFF;
                    newBytes[11] = 0xE0;

                }
                else
                {
                    Log.Normal("32Bit");
                    this.originalBytes = new byte[6];
                    this.originalBytes = Memory.Read(oldFunctionPointer, 6);

                    byte[] destinationByes = BitConverter.GetBytes(this.newFunctionPointer.ToInt32());
                    this.newBytes = new byte[]
                        {
                   0x68, destinationByes[0], destinationByes[1], destinationByes[2], destinationByes[3], 0xC3
                        };

                }
            }

            #endregion

            #region Public Properties

            /// <summary>
            /// Gets a value indicating whether the hook is installed.
            /// </summary>
            public bool IsInstalled
            {
                get; private set;
            }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// Calls the original function, and returns a return value.
            /// </summary>
            /// <param name="args">
            ///   The arguments to pass. If it is a 'void' argument list,
            ///   you must pass 'null'.
            /// </param>
            /// <returns>
            /// An object containing the original functions return value.
            /// </returns>
            public object CallOriginal(object instance, params object[] args)
            {
                this.Uninstall();
                var ret = this.origFuction.Invoke(instance, args);
                this.Install();
                return ret;
            }

            /// <summary>
            /// Installs the hook.
            /// </summary>
            /// <returns>
            /// Whether the operation was successful.
            /// </returns>
            public bool Install()
            {
                try
                {
                    //Log.Normal("Installing Detour");
                    Memory.Write(oldFunctionPointer, this.newBytes);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Removes the hook.
            /// </summary>
            /// <returns>
            /// Whether the operation was successful.
            /// </returns>
            public bool Uninstall()
            {
                try
                {
                    //Log.Normal("Uninstalling Detour");
                    Memory.Write(oldFunctionPointer, this.originalBytes);
                    this.IsInstalled = false;
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            #endregion
        }

        public static unsafe class Memory
        {
            public static byte[] Read(IntPtr functionPtr, int length)
            {
                Byte* pointerRawDest;
                if (IntPtr.Size == sizeof(Int64))
                {
                    Int64 destinationBase = functionPtr.ToInt64();
                    pointerRawDest = (Byte*)destinationBase;
                }
                else
                {
                    Int32 destinationBase = functionPtr.ToInt32();
                    pointerRawDest = (Byte*)destinationBase;
                }

                var dest = new byte[length];

                Int32 counter = 0;
                int idx = 0;
                while (idx < length)
                {
                    dest[idx] = *(pointerRawDest + counter);

                    counter++;
                    idx++;
                }
                return dest;
            }

            public static void Write(IntPtr functionPtr, byte[] allBites)
            {
                Byte* pointerRawDest;
                if (IntPtr.Size == sizeof(Int64))
                {
                    Int64 destinationBase = functionPtr.ToInt64();
                    pointerRawDest = (Byte*)destinationBase;
                }
                else
                {
                    Int32 destinationBase = functionPtr.ToInt32();
                    pointerRawDest = (Byte*)destinationBase;
                }


                Int32 counter = 0;
                foreach (byte value in allBites)
                {
                    *(pointerRawDest + counter) = value;
                    counter++;
                }
            }
        }

    }

}
