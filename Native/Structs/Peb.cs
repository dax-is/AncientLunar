using System.Runtime.InteropServices;

namespace AncientLunar.Native.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 2000)]
    internal readonly struct Peb64
    {
        [field: FieldOffset(0x18)] public readonly ulong LoaderData;
        [field: FieldOffset(0x68)] public readonly long ApiSetMap;
    }

    [StructLayout(LayoutKind.Explicit, Size = 2000)]
    internal readonly struct Peb32
    {
        [field: FieldOffset(0x38)] public readonly int ApiSetMap;
    }
}
