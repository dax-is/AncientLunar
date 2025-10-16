using AncientLunar.Extensions;
using AncientLunar.FileResolution;
using AncientLunar.Helpers;
using AncientLunar.Interop;
using AncientLunar.Native.Enums;
using AncientLunar.Native.PInvoke;
using AncientLunar.PortableExecutable;
using AncientLunar.PortableExecutable.Structs;
using AncientLunar.Remote.Structs;
using AncientLunar.Shellcode;
using AncientLunar.Shellcode.Structs;
using AncientLunar.SymbolResolution;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace AncientLunar.Remote
{
    public class ProcessContext
    {
        internal Architecture Architecture { get; }
        internal Process Process { get; }

        private readonly ApiSetMap _apiSetMap;
        private readonly Dictionary<string, Module> _moduleCache;
        private readonly SymbolLookup _symbolLookup;

        public ProcessContext(Process process)
        {
            _apiSetMap = new ApiSetMap();
            _moduleCache = new Dictionary<string, Module>(StringComparer.OrdinalIgnoreCase);
            _symbolLookup = new SymbolLookup(process.GetArchitecture());

            Architecture = process.GetArchitecture();
            Process = process;
        }

        public void CallRoutine(ulong routineAddress, CallingConvention callingConvention, params object[] arguments)
        {
            ArraySegment<byte> shellcodeBytes;

            if (Architecture == Architecture.X86)
            {
                var descriptor = new CallDescriptor<int>(routineAddress, callingConvention, Array.ConvertAll(arguments, argument => DumbConvert.ToInt32(argument)), null);
                shellcodeBytes = Assembler.AssembleCall32(descriptor);
            } else
            {
                var descriptor = new CallDescriptor<long>(routineAddress, callingConvention, Array.ConvertAll(arguments, argument => DumbConvert.ToInt64(argument)), null);
                shellcodeBytes = Assembler.AssembleCall64(descriptor);
            }

            ExecuteShellcode(shellcodeBytes);
        }

        public unsafe T CallRoutine<T>(ulong routineAddress, CallingConvention callingConvention, params object[] arguments)
            where T : unmanaged
        {
            var returnSize = typeof(T) == typeof(IntPtr) ? Architecture == Architecture.X86 ? sizeof(int) : sizeof(long) : sizeof(T);
            var returnAddress = Process.AllocateBuffer(returnSize, ProtectionType.ReadWrite);

            try
            {
                ArraySegment<byte> shellcodeBytes;

                if (Architecture == Architecture.X86)
                {
                    var descriptor = new CallDescriptor<int>(routineAddress, callingConvention, Array.ConvertAll(arguments, argument => DumbConvert.ToInt32(argument)), returnAddress);
                    shellcodeBytes = Assembler.AssembleCall32(descriptor);
                }
                else
                {
                    var descriptor = new CallDescriptor<long>(routineAddress, callingConvention, Array.ConvertAll(arguments, argument => DumbConvert.ToInt64(argument)), returnAddress);
                    shellcodeBytes = Assembler.AssembleCall64(descriptor);
                }

                ExecuteShellcode(shellcodeBytes);

                if (typeof(T) != typeof(IntPtr))
                    return Process.ReadStruct<T>(returnAddress);

                var pointer = Architecture == Architecture.X86 ? (IntPtr)Process.ReadStruct<int>(returnAddress) : (IntPtr)Process.ReadStruct<long>(returnAddress);
                var ptr = &pointer;

                return *(T*)ptr;
            } finally
            {
                Executor.IgnoreExceptions(() => Process.FreeBuffer(returnAddress));
            }
        }

        public ulong CallRoutinePointerWidth(ulong routineAddress, CallingConvention callingConvention, params object[] arguments)
        {
            var returnSize = Architecture == Architecture.X86 ? sizeof(int) : sizeof(long);
            var returnAddress = Process.AllocateBuffer(returnSize, ProtectionType.ReadWrite);

            try
            {
                ArraySegment<byte> shellcodeBytes;

                if (Architecture == Architecture.X86)
                {
                    var descriptor = new CallDescriptor<int>(routineAddress, callingConvention, Array.ConvertAll(arguments, argument => DumbConvert.ToInt32(argument)), returnAddress);
                    shellcodeBytes = Assembler.AssembleCall32(descriptor);
                }
                else
                {
                    var descriptor = new CallDescriptor<long>(routineAddress, callingConvention, Array.ConvertAll(arguments, argument => DumbConvert.ToInt64(argument)), returnAddress);
                    shellcodeBytes = Assembler.AssembleCall64(descriptor);
                }

                ExecuteShellcode(shellcodeBytes);

                var value = Architecture == Architecture.X86 ? (ulong)Process.ReadStruct<int>(returnAddress) : (ulong)Process.ReadStruct<long>(returnAddress);

                return value;
            }
            finally
            {
                Executor.IgnoreExceptions(() => Process.FreeBuffer(returnAddress));
            }
        }

        internal void ClearModuleCache()
        {
            _moduleCache.Clear();
        }

        public ulong GetFunctionAddress(string moduleName, string functionName)
        {
            var module = GetModule(moduleName, null);
            var function = module.PEImage.ExportDirectory.GetExportedFunction(functionName);

            if (function is null)
                throw new ApplicationException($"Failed to find the function {functionName} in the module {moduleName.ToLower()}");

            return function.Value.ForwarderString == null ? (module.Address + (ulong)function.Value.RelativeAddress) : ResolveForwardedFunction(function.Value.ForwarderString, null);
        }

        public ulong GetFunctionAddress(string moduleName, int functionOrdinal)
        {
            var module = GetModule(moduleName, null);
            var function = module.PEImage.ExportDirectory.GetExportedFunction(functionOrdinal);

            if (function is null)
                throw new ApplicationException($"Failed to find the function #{functionOrdinal} in the module {moduleName.ToLower()}");

            return function.Value.ForwarderString == null ? (module.Address + (ulong)function.Value.RelativeAddress) : ResolveForwardedFunction(function.Value.ForwarderString, null);
        }

        internal ulong GetModuleAddress(string moduleName)
        {
            return GetModule(moduleName, null).Address;
        }

        internal ulong GetNtdllSymbolAddress(string symbolName)
        {
            return GetModule("ntdll.dll", null).Address + (ulong)_symbolLookup.GetOffset(symbolName);
        }

        internal void RecordModuleLoad(ulong moduleAddress, string moduleFilePath)
        {
            var key = Path.GetFileName(moduleFilePath);

            if (_moduleCache.ContainsKey(key))
                return;

            _moduleCache.Add(Path.GetFileName(moduleFilePath), new Module(moduleAddress, new PEImage(new ArraySegment<byte>(File.ReadAllBytes(moduleFilePath)))));
        }

        internal string ResolveModuleName(string moduleName, string parentName)
        {
            if (moduleName.StartsWith("api-ms") || moduleName.StartsWith("ext-ms"))
                return _apiSetMap.ResolveApiSet(moduleName, parentName) ?? moduleName;

            return moduleName;
        }

        private void ExecuteShellcode(ArraySegment<byte> shellcodeBytes)
        {
            // Execute the shellcode in the process
            var shellcodeAddress = Process.AllocateBuffer(shellcodeBytes.Count, ProtectionType.ExecuteRead);
            var threadHandle = IntPtr.Zero;

            try
            {
                Process.WriteSpan(shellcodeAddress, shellcodeBytes);

                if (Architecture == Architecture.X64 && Process.GetCurrentProcess().GetArchitecture() == Architecture.X86)
                {
                    var status = Wow64.CreateRemoteThread(Process.Handle, (ulong)shellcodeAddress, out threadHandle);
                    
                    if (!status.IsSuccess())
                        throw new Win32Exception(Ntdll.RtlNtStatusToDosError(status));
                }
                else
                {
                    var status = Ntdll.RtlCreateUserThread(Process.Handle, IntPtr.Zero, false, 0, IntPtr.Zero, IntPtr.Zero, shellcodeAddress, IntPtr.Zero, out threadHandle, IntPtr.Zero);

                    if (!status.IsSuccess())
                        throw new Win32Exception(Ntdll.RtlNtStatusToDosError(status));
                }

                if (Kernel32.WaitForSingleObject(threadHandle, int.MaxValue) == -1)
                    throw new Win32Exception();
            } finally
            {
                if (threadHandle != IntPtr.Zero)
                    Kernel32.CloseHandle(threadHandle);

                Executor.IgnoreExceptions(() => Process.FreeBuffer(shellcodeAddress));
            }
        }

        internal Module GetModule(string moduleName, string parentName)
        {
            moduleName = ResolveModuleName(moduleName, parentName);

            if (_moduleCache.TryGetValue(moduleName, out var module))
                return module;

            // Query the process for its module address list
            foreach (var mod in Process.EnumModules())
            {
                var moduleFilePath = mod.FileName;

                if (Architecture == Architecture.X86)
                    moduleFilePath = moduleFilePath.ToLowerInvariant().Replace("system32", "SysWOW64");
                else if (Architecture == Architecture.X64 && Process.GetCurrentProcess().GetArchitecture() == Architecture.X86)
                    moduleFilePath = moduleFilePath.ToLowerInvariant().Replace("system32", "sysnative");

                if (moduleName.Equals(Path.GetFileName(moduleFilePath), StringComparison.OrdinalIgnoreCase))
                {
                    if (!_moduleCache.ContainsKey(moduleName))
                        _moduleCache.Add(moduleName, new Module(mod.Address, new PEImage(new ArraySegment<byte>(File.ReadAllBytes(moduleFilePath)))));

                    return _moduleCache[moduleName];
                }
            }

            throw new ApplicationException($"Failed to find the module {moduleName.ToLower()} in the process");
        }

        private ulong ResolveForwardedFunction(string forwarderString, string parentName)
        {
            while (true)
            {
                var forwardedData = forwarderString.Split('.');
                var module = GetModule($"{forwardedData[0]}.dll", parentName);

                ExportedFunction? forwardedFunction;

                if (forwardedData[1].StartsWith("#"))
                {
                    var functionOrdinal = int.Parse(forwardedData[1].Replace("#", string.Empty));
                    forwardedFunction = module.PEImage.ExportDirectory.GetExportedFunction(functionOrdinal);
                }
                else
                    forwardedFunction = module.PEImage.ExportDirectory.GetExportedFunction(forwardedData[1]);

                if (!forwardedFunction.HasValue)
                    throw new ApplicationException($"Failed to find the function {forwardedData[1]} in the module {forwardedData[0].ToLower()}.dll");

                if (forwardedFunction.Value.ForwarderString is null)
                    return module.Address + (ulong)forwardedFunction.Value.RelativeAddress;

                forwarderString = forwardedFunction.Value.ForwarderString;
                parentName = ResolveModuleName($"{forwardedData[0]}.dll", parentName);
            }
        }
    }
}
