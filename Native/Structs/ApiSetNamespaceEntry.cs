using System.Runtime.InteropServices;

namespace AncientLunar.Native.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 24)]
    internal readonly struct ApiSetNamespaceEntry
    {
        [field: FieldOffset(0x4)] public readonly int NameOffset;
        [field: FieldOffset(0x8)] public readonly int NameLength;
        [field: FieldOffset(0x10)] public readonly int ValueOffset;
        [field: FieldOffset(0x14)] public readonly int ValueCount;
    }
}