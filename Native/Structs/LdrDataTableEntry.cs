using System.Runtime.InteropServices;

namespace AncientLunar.Native.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 192)]
    internal readonly struct LdrDataTableEntry32
    {
        [field: FieldOffset(0x18)] public readonly int DllBase;

        internal LdrDataTableEntry32(int dllBase)
        {
            DllBase = dllBase;
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 312)]
    internal readonly struct LdrDataTableEntry64
    {
        [field: FieldOffset(0x0)] public readonly ListEntry64 InLoadOrderLinks;
        [field: FieldOffset(0x30)] public readonly long DllBase;
        [field: FieldOffset(0x48)] public readonly UnicodeString64 FullDllName;

        internal LdrDataTableEntry64(long dllBase)
        {
            InLoadOrderLinks = default;
            DllBase = dllBase;
            FullDllName = default;
        }
    }
}
