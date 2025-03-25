using System;

namespace AncientLunar.Helpers
{
    internal static class ArraySegmentExtensions
    {
        /// <summary>
        /// Map a view of an array segment to a new array segment, without cloning the data
        /// </summary>
        public static ArraySegment<T> GetRange<T>(this ArraySegment<T> segment, int start = -1, int end = -1)
        {
            var actualStart = (start == -1) ? segment.Offset : segment.Offset + start;
            var actualEnd = (end == -1) ? segment.Offset + segment.Count : segment.Offset + end;

            if (actualStart < segment.Offset || actualStart >= segment.Offset + segment.Count ||
                actualEnd < segment.Offset || actualEnd > segment.Offset + segment.Count || actualStart > actualEnd)
            {
                throw new ArgumentOutOfRangeException("Invalid range specified.");
            }

            int newCount = actualEnd - actualStart;

            return new ArraySegment<T>(segment.Array, actualStart, newCount);
        }

        public static T[] ToArray<T>(this ArraySegment<T> segment)
        {
            var alloc = new T[segment.Count];
            Array.Copy(segment.Array, segment.Offset, alloc, 0, segment.Count);

            return alloc;
        }
    }
}
