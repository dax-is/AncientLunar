using AncientLunar.Helpers;
using AncientLunar.Interop;
using AncientLunar.Native.Enums;
using AncientLunar.Native.PInvoke;
using AncientLunar.Native.Structs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using AncientLunar.Native;
using AncientLunar.Remote.Structs;

namespace AncientLunar.Extensions
{
    public static class ProcessExtensions
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

        public static IEnumerable<ModuleInfo> EnumModules(this Process process, bool all = false)
        {
            if (Process.GetCurrentProcess().GetArchitecture() == process.GetArchitecture())
            {
                foreach (ProcessModule mod in process.Modules)
                    yield return new ModuleInfo((ulong)mod.BaseAddress, mod.FileName);

                yield break;
            }

            if (Process.GetCurrentProcess().GetArchitecture() == Architecture.X64)
            {
                if (all)
                {
                    foreach (ProcessModule mod in process.Modules)
                        yield return new ModuleInfo((ulong)mod.BaseAddress, mod.FileName);

                    yield break;
                }

                foreach (var mod in process.EnumModulesX86())
                    yield return mod;

                yield break;
            }

            if (Process.GetCurrentProcess().IsEmulating64() && !process.IsEmulating64())
            {
                foreach (var mod in process.EnumModulesWow64())
                    yield return mod;

                yield break;
            }

            foreach (ProcessModule mod in process.Modules)
                yield return new ModuleInfo((ulong)mod.BaseAddress, mod.FileName);
        }

        public static IEnumerable<ModuleInfo> EnumModulesWow64(this Process process)
        {
            var pbi = new ProcessBasicInformation64();
            var status = Ntdll.NtWow64QueryInformationProcess64(process.Handle, ProcessInfoClass.ProcessBasicInformation, ref pbi, Marshal.SizeOf(typeof(ProcessBasicInformation64)), IntPtr.Zero);
            if (!status.IsSuccess())
                throw new Win32Exception(Ntdll.RtlNtStatusToDosError(status));

            foreach (var mod in Wow64.EnumModules(process, pbi.PebBaseAddress))
                yield return mod;
        }

        public static IEnumerable<ModuleInfo> EnumModulesX86(this Process process)
        {
            var moduleAddressListBytes = new byte[8];

            if (!Kernel32.EnumProcessModulesEx(process.Handle, moduleAddressListBytes, moduleAddressListBytes.Length, out var sizeNeeded, ModuleType.X86))
                throw new Win32Exception();

            if (sizeNeeded > moduleAddressListBytes.Length)
            {
                // Reallocate the module address buffer
                moduleAddressListBytes = new byte[sizeNeeded];

                if (!Kernel32.EnumProcessModulesEx(process.Handle, moduleAddressListBytes, moduleAddressListBytes.Length, out sizeNeeded, ModuleType.X86))
                    throw new Win32Exception();
            }

            // Buffer for storing module path
            var moduleFilePathBytes = new byte[Encoding.Unicode.GetMaxByteCount(Constants.MaxPath)];

            foreach (var address in MemoryMarshal.AllocCast<byte, IntPtr>(moduleAddressListBytes))
            {
                if (!Kernel32.GetModuleFileNameEx(process.Handle, address, moduleFilePathBytes, Encoding.Unicode.GetCharCount(moduleFilePathBytes)))
                    throw new Win32Exception();

                var moduleFilePath = Encoding.Unicode.GetString(moduleFilePathBytes).TrimEnd('\0');

                yield return new ModuleInfo((ulong)address, moduleFilePath);

                Array.Clear(moduleFilePathBytes, 0, moduleFilePathBytes.Length);
            }
        }

        public static IntPtr AllocateBuffer(this Process process, int size, ProtectionType protectionType)
        {
            var address = Kernel32.VirtualAllocEx(process.Handle, IntPtr.Zero, (IntPtr)size, AllocationType.Commit | AllocationType.Reserve, protectionType);

            if (address == IntPtr.Zero)
                throw new Win32Exception();

            return address;
        }

        public static void FreeBuffer(this Process process, IntPtr address)
        {
            if (!Kernel32.VirtualFreeEx(process.Handle, address, IntPtr.Zero, FreeType.Release))
                throw new Win32Exception();
        }

        public static bool IsEmulating64(this Process process)
        {
            if (!Kernel32.IsWow64Process(process.Handle, out var isWow64Process))
                throw new Win32Exception();

            return isWow64Process;
        }

        public static Architecture GetArchitecture(this Process process)
        {
            if (!Kernel32.IsWow64Process2(process.Handle, out var processMachine, out var nativeMachine))
                throw new Win32Exception();

            if (processMachine == 0) // IMAGE_FILE_MACHINE_UNKNOWN
                return nativeMachine == 0x8664 ? Architecture.X64 : Architecture.X86;

            return Architecture.X86;
        }

        public static ProtectionType ProtectBuffer(this Process process, IntPtr address, int size, ProtectionType protectionType)
        {
            if (!Kernel32.VirtualProtectEx(process.Handle, address, (IntPtr)size, protectionType, out var oldProtectionType))
                throw new Win32Exception();

            return oldProtectionType;
        }

        public static ArraySegment<T> ReadSpan<T>(this Process process, IntPtr address, int elements) where T: unmanaged
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

        public static T ReadStruct<T>(this Process process, IntPtr address) where T: unmanaged
        {
            return MemoryMarshal.Read<T>(process.ReadSpan<byte>(address, Marshal.SizeOf(typeof(T))));
        }

        public static void WriteSpan<T>(this Process process, IntPtr address, ArraySegment<T> span) where T : unmanaged
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

        public static void WriteString(this Process process, IntPtr address, string @string)
        {
            process.WriteSpan(address, new ArraySegment<byte>(Encoding.Unicode.GetBytes(@string)));
        }

        public static void WriteStruct<T>(this Process process, IntPtr address, T @struct) where T : unmanaged
        {
            process.WriteSpan(address, new ArraySegment<byte>(MemoryMarshal.AllocCast<T, byte>(new T[] { @struct })));
        }

        public static T ReadWow64Memory<T>(this Process process, ulong address) where T : unmanaged
        {
            var size = Marshal.SizeOf(typeof(T));
            var buffer = new byte[size];
            var status = Ntdll.NtWow64ReadVirtualMemory64(process.Handle, address, buffer, (ulong)buffer.Length, IntPtr.Zero);
            if (!status.IsSuccess())
                throw new Win32Exception(Ntdll.RtlNtStatusToDosError(status));

            return ByteArrayToStructure<T>(buffer);
        }

        public static string ReadUnicodeString(this Process process, UnicodeString64 unicodeString)
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
