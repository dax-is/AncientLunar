using AncientLunar.Helpers;
using AncientLunar.Interop;
using AncientLunar.Native.Enums;
using AncientLunar.Native.PInvoke;
using AncientLunar.Native.Structs;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace AncientLunar.Extensions
{
    internal static class ProcessExtensions
    {
        public static string GetFileName(this Process process)
        {
            if (process.GetArchitecture() == Architecture.X86 || Process.GetCurrentProcess().GetArchitecture() == Architecture.X64)
                return process.MainModule.FileName;

            var pbi = new ProcessBasicInformation64();
            var status = Ntdll.NtWow64QueryInformationProcess64(process.Handle, ProcessInfoClass.ProcessBasicInformation, ref pbi, Marshal.SizeOf(typeof(ProcessBasicInformation64)), IntPtr.Zero);
            if (!status.IsSuccess())
                throw new Win32Exception(Ntdll.RtlNtStatusToDosError(status));

            var peb = process.ReadWow64Memory<Peb64>(pbi.PebBaseAddress);
            var ldr = process.ReadWow64Memory<PebLdrData64>(peb.LoaderData);

            var ldrEntry = process.ReadWow64Memory<LdrDataTableEntry64>(ldr.InLoadOrderModuleList.Flink);
            var moduleName = process.ReadUnicodeString(ldrEntry.FullDllName);

            return moduleName;
        }

        internal static IntPtr AllocateBuffer(this Process process, int size, ProtectionType protectionType)
        {
            var address = Kernel32.VirtualAllocEx(process.Handle, IntPtr.Zero, (IntPtr)size, AllocationType.Commit | AllocationType.Reserve, protectionType);

            if (address == IntPtr.Zero)
                throw new Win32Exception();

            return address;
        }

        internal static void FreeBuffer(this Process process, IntPtr address)
        {
            if (!Kernel32.VirtualFreeEx(process.Handle, address, IntPtr.Zero, FreeType.Release))
                throw new Win32Exception();
        }

        internal static bool IsEmulating64(this Process process)
        {
            if (!Kernel32.IsWow64Process(process.Handle, out var isWow64Process))
                throw new Win32Exception();

            return isWow64Process;
        }

        internal static Architecture GetArchitecture(this Process process)
        {
            if (!Kernel32.IsWow64Process2(process.Handle, out var processMachine, out var nativeMachine))
                throw new Win32Exception();

            if (processMachine == 0) // IMAGE_FILE_MACHINE_UNKNOWN
                return nativeMachine == 0x8664 ? Architecture.X64 : Architecture.X86;

            return Architecture.X86;
        }

        internal static ProtectionType ProtectBuffer(this Process process, IntPtr address, int size, ProtectionType protectionType)
        {
            if (!Kernel32.VirtualProtectEx(process.Handle, address, (IntPtr)size, protectionType, out var oldProtectionType))
                throw new Win32Exception();

            return oldProtectionType;
        }

        internal static ArraySegment<T> ReadSpan<T>(this Process process, IntPtr address, int elements) where T: unmanaged
        {
            var spanBytes = new byte[Marshal.SizeOf(typeof(T)) * elements];
            var oldProtectionType = process.ProtectBuffer(address, spanBytes.Length, ProtectionType.ExecuteReadWrite);

            try
            {
                if (!Kernel32.ReadProcessMemory(process.Handle, address, spanBytes, (IntPtr)spanBytes.Length, IntPtr.Zero))
                    throw new Win32Exception();
            } finally
            {
                process.ProtectBuffer(address, spanBytes.Length, oldProtectionType);
            }

            return new ArraySegment<T>(MemoryMarshal.AllocCast<byte, T>(spanBytes));
        }

        internal static T ReadStruct<T>(this Process process, IntPtr address) where T: unmanaged
        {
            return MemoryMarshal.Read<T>(process.ReadSpan<byte>(address, Marshal.SizeOf(typeof(T))));
        }

        internal static void WriteSpan<T>(this Process process, IntPtr address, ArraySegment<T> span) where T : unmanaged
        {
            var spanBytes = MemoryMarshal.AllocCast<T, byte>(span.ToArray());
            var oldProtectionType = process.ProtectBuffer(address, spanBytes.Length, ProtectionType.ExecuteReadWrite);

            try
            {
                if (!Kernel32.WriteProcessMemory(process.Handle, address, spanBytes, (IntPtr)spanBytes.Length, IntPtr.Zero))
                    throw new Win32Exception();
            }
            finally
            {
                process.ProtectBuffer(address, spanBytes.Length, oldProtectionType);
            }
        }

        internal static void WriteString(this Process process, IntPtr address, string @string)
        {
            process.WriteSpan(address, new ArraySegment<byte>(Encoding.Unicode.GetBytes(@string)));
        }

        internal static void WriteStruct<T>(this Process process, IntPtr address, T @struct) where T : unmanaged
        {
            process.WriteSpan(address, new ArraySegment<byte>(MemoryMarshal.AllocCast<T, byte>(new T[] { @struct })));
        }

        internal static T ReadWow64Memory<T>(this Process process, ulong address) where T : unmanaged
        {
            var size = Marshal.SizeOf(typeof(T));
            var buffer = new byte[size];
            var status = Ntdll.NtWow64ReadVirtualMemory64(process.Handle, address, buffer, (ulong)buffer.Length, IntPtr.Zero);
            if (!status.IsSuccess())
                throw new Win32Exception(Ntdll.RtlNtStatusToDosError(status));

            return ByteArrayToStructure<T>(buffer);
        }

        internal static string ReadUnicodeString(this Process process, UnicodeString64 unicodeString)
        {
            var buffer = new byte[unicodeString.MaximumLength];
            var status = Ntdll.NtWow64ReadVirtualMemory64(process.Handle, unicodeString.Buffer, buffer, (ulong)buffer.Length, IntPtr.Zero);
            if (!status.IsSuccess())
                throw new Win32Exception(Ntdll.RtlNtStatusToDosError(status));

            return Encoding.Unicode.GetString(buffer).TrimEnd('\0');
        }

        private static T ByteArrayToStructure<T>(byte[] bytes) where T : unmanaged
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var obj = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            
            handle.Free();
            return obj;
        }
    }
}
