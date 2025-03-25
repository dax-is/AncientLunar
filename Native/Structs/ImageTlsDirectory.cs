using System.Runtime.InteropServices;

namespace AncientLunar.Native.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 24)]
    internal readonly struct ImageTlsDirectory32
    {
        [field: FieldOffset(0xC)] public readonly int AddressOfCallBacks;
    }

    [StructLayout(LayoutKind.Explicit, Size = 40)]
    internal readonly struct ImageTlsDirectory64
    {
        [field: FieldOffset(0x18)] public readonly long AddressOfCallBacks;
    }
}
