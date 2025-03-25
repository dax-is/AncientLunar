using System.Runtime.InteropServices;

namespace AncientLunar.Native.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ListEntry64
    {
        public readonly ulong Flink;
        public readonly ulong Blink;
    }
}
