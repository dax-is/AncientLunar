using System.Runtime.InteropServices;

namespace AncientLunar.Native.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 28)]
    internal readonly struct ApiSetNamespace
    {
        [FieldOffset(0xC)]
        public readonly int Count;

        [FieldOffset(0x10)]
        public readonly int EntryOffset;

        [FieldOffset(0x14)]
        public readonly int HashOffset;

        [FieldOffset(0x18)]
        public readonly int HashFactor;
    }
}
