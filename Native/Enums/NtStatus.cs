using System.Runtime.InteropServices;

namespace AncientLunar.Native.Enums
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NtStatus
    {
        public readonly uint Status;

        internal NtStatus(uint status) { Status = status; }
    }
}
