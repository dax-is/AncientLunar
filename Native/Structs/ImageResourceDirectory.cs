using System.Runtime.InteropServices;

namespace AncientLunar.Native.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    internal readonly struct ImageResourceDirectory
    {
        [field: FieldOffset(0xC)] public readonly short NumberOfNameEntries;
        [field: FieldOffset(0xE)] public readonly short NumberOfIdEntries;
    }
}
