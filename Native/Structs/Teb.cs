using System.Runtime.InteropServices;

namespace AncientLunar.Native.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 232)]
    internal readonly struct Teb64
    {
        [field: FieldOffset(0x60)] public readonly ulong ProcessEnvironmentBlock;
    }
}
