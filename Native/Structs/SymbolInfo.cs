using System.Runtime.InteropServices;

namespace AncientLunar.Native.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 88)]
    internal readonly struct SymbolInfo
    {
        [field: FieldOffset(0x0)] public readonly int SizeOfStruct;
        [field: FieldOffset(0x38)] public readonly long Address;
        [field: FieldOffset(0x50)] public readonly int MaxNameLen;

        internal SymbolInfo(int sizeOfStruct, long address, int maxNameLen)
        {
            SizeOfStruct = sizeOfStruct;
            Address = address;
            MaxNameLen = maxNameLen;
        }
    }
}
