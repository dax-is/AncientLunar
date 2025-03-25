using System.Runtime.InteropServices;

namespace AncientLunar.Native.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 40)]
    internal readonly struct ImageExportDirectory
    {
        [field: FieldOffset(0x10)] public readonly int Base;
        [field: FieldOffset(0x14)] public readonly int NumberOfFunctions;
        [field: FieldOffset(0x18)] public readonly int NumberOfNames;
        [field: FieldOffset(0x1C)] public readonly int AddressOfFunctions;
        [field: FieldOffset(0x20)] public readonly int AddressOfNames;
        [field: FieldOffset(0x24)] public readonly int AddressOfNameOrdinals;
    }
}