using System.Runtime.InteropServices;

namespace AncientLunar.Native.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 48)]
    internal struct ProcessBasicInformation64
    {
        [FieldOffset(0x8)] public readonly ulong PebBaseAddress;
    }
}
