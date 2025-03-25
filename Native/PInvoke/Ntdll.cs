using AncientLunar.Native.Enums;
using AncientLunar.Native.Structs;
using System;
using System.Runtime.InteropServices;

namespace AncientLunar.Native.PInvoke
{
    internal static class Ntdll
    {
        [DllImport("ntdll.dll")]
        internal static extern NtStatus RtlCreateUserThread(IntPtr processHandle, IntPtr securityDescriptor, [MarshalAs(UnmanagedType.Bool)] bool createSuspended, int stackZeroBits, IntPtr stackReserved, IntPtr stackCommit, IntPtr startAddress, IntPtr parameter, out IntPtr threadHandle, IntPtr clientId);

        [DllImport("ntdll.dll")]
        internal static extern IntPtr RtlGetCurrentPeb();

        [DllImport("ntdll.dll")]
        internal static extern int RtlNtStatusToDosError(NtStatus status);

        [DllImport("ntdll.dll")]
        internal static extern NtStatus NtWow64QueryInformationProcess64(IntPtr processHandle, ProcessInfoClass processInformationClass, ref ProcessBasicInformation64 processInformation, int processInformationLength, IntPtr returnLength);

        [DllImport("ntdll.dll")]
        internal static extern NtStatus NtWow64AllocateVirtualMemory64(IntPtr processHandle, ref ulong baseAddress, ulong zeroBits, ref uint regionSize, AllocationType allocationType, ProtectionType protect);

        [DllImport("ntdll.dll")]
        internal static extern NtStatus NtWow64ReadVirtualMemory64(IntPtr processHandle, ulong baseAddress, byte[] buffer, ulong size, IntPtr numberOfBytesRead);

    }
}
