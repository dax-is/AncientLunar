using System.Runtime.InteropServices;

namespace AncientLunar.Native.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 20)]
    internal readonly struct ApiSetValueEntry
    {
        [field: FieldOffset(0x4)] public readonly int NameOffset;
        [field: FieldOffset(0x8)] public readonly int NameLength;
        [field: FieldOffset(0xC)] public readonly int ValueOffset;
        [field: FieldOffset(0x10)] public readonly int ValueCount;
    }
}