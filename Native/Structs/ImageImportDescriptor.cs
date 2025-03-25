using System.Runtime.InteropServices;

namespace AncientLunar.Native.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 20)]
    internal readonly struct ImageImportDescriptor
    {
        [field: FieldOffset(0x0)] public readonly int OriginalFirstThunk;
        [field: FieldOffset(0xC)] public readonly int Name;
        [field: FieldOffset(0x10)] public readonly int FirstThunk;
    }
}
