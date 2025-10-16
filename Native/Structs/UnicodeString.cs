using System.Runtime.InteropServices;

namespace AncientLunar.Native.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct UnicodeString64
    {
        public ushort Length;
        public ushort MaximumLength;
        public ulong Buffer;
    }
}
