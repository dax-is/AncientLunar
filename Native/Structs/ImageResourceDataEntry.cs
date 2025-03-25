using System.Runtime.InteropServices;

namespace AncientLunar.Native.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    internal readonly struct ImageResourceDataEntry
    {
        [field: FieldOffset(0x0)] public readonly int OffsetToData;
        [field: FieldOffset(0x4)] public readonly int Size;
    }
}
