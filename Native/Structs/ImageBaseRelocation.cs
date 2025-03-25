using System.Runtime.InteropServices;

namespace AncientLunar.Native.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    internal readonly struct ImageBaseRelocation
    {
        [field: FieldOffset(0x0)] public readonly int VirtualAddress;
        [field: FieldOffset(0x4)] public readonly int SizeOfBlock;
    }
}