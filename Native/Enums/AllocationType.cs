using System;

namespace AncientLunar.Native.Enums
{
    [Flags]
    internal enum AllocationType
    {
        Commit = 0x1000,
        Reserve = 0x2000,
        TopDown = 0x100000
    }
}
