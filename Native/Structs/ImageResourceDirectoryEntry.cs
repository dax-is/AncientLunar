using System.Runtime.InteropServices;

namespace AncientLunar.Native.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    internal readonly struct ImageResourceDirectoryEntry
    {
        [field: FieldOffset(0x0)] public readonly int Id;
        [field: FieldOffset(0x4)] public readonly int OffsetToData;
    }
}
