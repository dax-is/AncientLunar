using System;
using System.Runtime.InteropServices;

namespace AncientLunar.Helpers
{
    internal static class MemoryMarshal
    {
        public static unsafe T Read<T>(ArraySegment<byte> source)
            where T : struct
        {
            fixed (byte* ptr = source.Array)
            {
                return (T)Marshal.PtrToStructure((IntPtr)(ptr + source.Offset), typeof(T));
            }
        }

        public static unsafe void Write<T>(byte[] destination, in T value, int offset = 0) where T : struct
        {
            fixed (byte* ptr = &destination[offset])
            {
                Marshal.StructureToPtr(value, (IntPtr)ptr, true);
            }
        }

        public static unsafe void Write<T>(ArraySegment<byte> destination, in T value, int offset) where T : struct
        {
            Write(destination.Array, in value, destination.Offset + offset);
        }

        public static unsafe TTo[] AllocCast<TFrom, TTo>(TFrom[] source)
        where TFrom : unmanaged
        where TTo : unmanaged
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            int fromSize = sizeof(TFrom);
            int toSize = sizeof(TTo);

            if (fromSize == 0 || toSize == 0)
                throw new ArgumentException("Cannot cast types with zero size.");

            int totalBytes = source.Length * fromSize;
            int toLength = totalBytes / toSize;

            if (totalBytes % toSize != 0)
                throw new InvalidOperationException("Size mismatch: Data cannot be evenly divided.");

            TTo[] result = new TTo[toLength];

            fixed (TFrom* pSource = source)
            fixed (TTo* pResult = result)
            {
                byte* src = (byte*)pSource;
                byte* dst = (byte*)pResult;
                for (int i = 0; i < totalBytes; i++)
                {
                    dst[i] = src[i];
                }
            }

            return result;
        }
    }
}
