using AncientLunar.Native.Enums;
using AncientLunar.Native.Structs;
using System;
using System.Runtime.InteropServices;

namespace AncientLunar.Native.PInvoke
{
	internal static class Kernel32
	{
		[DllImport("kernel32.dll", EntryPoint = "K32EnumProcessModulesEx", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool EnumProcessModulesEx(IntPtr processHandle, byte[] bytes, int size, out int sizeNeeded, ModuleType moduleType);

		[DllImport("kernel32.dll", EntryPoint = "K32GetModuleFileNameExW", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetModuleFileNameEx(IntPtr processHandle, IntPtr address, byte[] bytes, int size);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWow64Process2(IntPtr processHandle, out ushort pProcessMachine, out ushort pNativeMachine);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWow64Process(IntPtr processHandle, [MarshalAs(UnmanagedType.Bool)] out bool isWow64Process);

        [DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool ReadProcessMemory(IntPtr processHandle, IntPtr address, byte[] bytes, IntPtr size, IntPtr bytesReadCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr VirtualAllocEx(IntPtr processHandle, IntPtr address, IntPtr size, AllocationType allocationType, ProtectionType protectionType);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr VirtualAlloc(IntPtr address, IntPtr size, AllocationType allocationType, ProtectionType protectionType);

        [DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool VirtualFreeEx(IntPtr processHandle, IntPtr address, IntPtr size, FreeType freeType);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool VirtualProtectEx(IntPtr processHandle, IntPtr address, IntPtr size, ProtectionType protectionType, out ProtectionType oldProtectionType);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern int WaitForSingleObject(IntPtr objectHandle, int milliseconds);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool WriteProcessMemory(IntPtr processHandle, IntPtr address, byte[] bytes, IntPtr size, IntPtr bytesWrittenCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool Module32First(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool Module32Next(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool Wow64RevertWow64FsRedirection(IntPtr ptr);

    }
}
