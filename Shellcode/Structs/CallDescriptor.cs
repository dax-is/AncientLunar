using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AncientLunar.Shellcode.Structs
{
    internal struct CallDescriptor<T>
    {
        public ulong Address;
        public CallingConvention CallingConvention;
        public IList<T> Arguments;
        public IntPtr? ReturnAddress;

        internal CallDescriptor(ulong address, CallingConvention callingConvention, IList<T> arguments, IntPtr? returnAddress)
        {
            Address = address;
            CallingConvention = callingConvention;
            Arguments = arguments;
            ReturnAddress = returnAddress;
        }
    }
}
