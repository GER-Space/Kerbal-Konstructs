using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace KerbalKonstructs.Core
{
    class AsmUtils
    {

        public static unsafe Boolean TryDetourFromTo(MethodBase source, MethodBase destination)
        {
            // error out on null arguments
            if (source == null)
            {
                Log.Error("Source MethodInfo is null");
                return false;
            }

            if (destination == null)
            {
                Log.Error("Destination MethodInfo is null");
                return false;
            }

            if (IntPtr.Size == sizeof(Int64))
            {
                // 64-bit systems use 64-bit absolute address and jumps
                // 12 byte destructive

                // Get function pointers
                Int64 sourceBase = source.MethodHandle.GetFunctionPointer().ToInt64();
                Int64 destinationBase = destination.MethodHandle.GetFunctionPointer().ToInt64();

                // Native source address
                Byte* pointerRawSource = (Byte*)sourceBase;

                // Pointer to insert jump address into native code
                Int64* pointerRawAddress = (Int64*)(pointerRawSource + 0x02);

                // Insert 64-bit absolute jump into native code (address in rax)
                // mov rax, immediate64
                // jmp [rax]
                *(pointerRawSource + 0x00) = 0x48;
                *(pointerRawSource + 0x01) = 0xB8;
                *pointerRawAddress = destinationBase; // ( Pointer_Raw_Source + 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 )
                *(pointerRawSource + 0x0A) = 0xFF;
                *(pointerRawSource + 0x0B) = 0xE0;

            }
            else
            {
                // 32-bit systems use 32-bit relative offset and jump
                // 5 byte destructive

                // Get function pointers
                Int32 sourceBase = source.MethodHandle.GetFunctionPointer().ToInt32();
                Int32 destinationBase = destination.MethodHandle.GetFunctionPointer().ToInt32();

                // Native source address
                Byte* pointerRawSource = (Byte*)sourceBase;

                // Pointer to insert jump address into native code
                Int32* pointerRawAddress = (Int32*)(pointerRawSource + 1);

                // Jump offset (less instruction size)
                Int32 offset = (destinationBase - sourceBase) - 5;

                // Insert 32-bit relative jump into native code
                *pointerRawSource = 0xE9;
                *pointerRawAddress = offset;
            }

            // done!
            return true;
        }


    }
}
