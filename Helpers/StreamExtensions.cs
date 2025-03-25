using System;
using System.IO;

namespace AncientLunar.Helpers
{
    internal static class StreamExtensions
    {
        internal static int GetAndValidateSize(Stream stream, int size, string streamParameterName)
        {
            long num = stream.Length - stream.Position;
            if (size < 0 || size > num)
            {
                throw new ArgumentOutOfRangeException("size");
            }
            if (size != 0)
            {
                return size;
            }
            if (num > 2147483647L)
            {
                throw new ArgumentException("Stream too large", streamParameterName);
            }
            return (int)num;
        }
    }
}
