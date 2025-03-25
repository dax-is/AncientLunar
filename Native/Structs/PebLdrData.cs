using System.Runtime.InteropServices;

namespace AncientLunar.Native.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct PebLdrData64
    {
        [field: FieldOffset(0x10)] public readonly ListEntry64 InLoadOrderModuleList;
    }
}
