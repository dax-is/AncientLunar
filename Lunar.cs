using AncientLunar.Extensions;
using AncientLunar.FileResolution;
using AncientLunar.Helpers;
using AncientLunar.Interop;
using AncientLunar.Native.Enums;
using AncientLunar.Native.PInvoke;
using AncientLunar.Native.Structs;
using AncientLunar.PortableExecutable;
using AncientLunar.PortableExecutable.Native;
using AncientLunar.Remote;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using ActivationContext = AncientLunar.FileResolution.ActivationContext;

namespace AncientLunar
{
    /// <summary>
    /// Provides the functionality to map a DLL from disk or memory into a process
    /// </summary>
    public class Lunar
    {
        /// <summary>
        /// The base address of the DLL in the process
        /// </summary>
        public IntPtr DllBaseAddress { get; private set; }

        private readonly ArraySegment<byte> _dllBytes;
        private readonly FileResolver _fileResolver;
        private readonly MappingFlags _mappingFlags;
        private readonly PEImage _peImage;
        private readonly ProcessContext _processContext;

        private IntPtr _ldrEntryAddress;

        /// <summary>
        /// Initializes an instance of the <see cref="Lunar"/> class with the functionality to map a DLL from memory into a process
        /// </summary>
        public Lunar(Process process, byte[] dllBytes, MappingFlags mappingFlags = MappingFlags.None)
        {
            if (process.HasExited)
                throw new ArgumentException("The provided process is not currently running", nameof(process));

            if (dllBytes.Length == 0)
                throw new ArgumentException("The provided DLL bytes were empty", nameof(dllBytes));

            _dllBytes = new ArraySegment<byte>((byte[])dllBytes.Clone());
            _fileResolver = new FileResolver(process, null);
            _mappingFlags = mappingFlags;
            _peImage = new PEImage(new ArraySegment<byte>(dllBytes));
            _processContext = new ProcessContext(process);
        }

        /// <summary>
        /// Initializes an instance of the <see cref="Lunar"/> class with the functionality to map a DLL from disk into a process
        /// </summary>
        public Lunar(Process process, string dllFilePath, MappingFlags mappingFlags = MappingFlags.None)
        {
            if (process.HasExited)
                throw new ArgumentException("The provided process is not currently running", nameof(process));

            if (!File.Exists(dllFilePath))
                throw new ArgumentException("The provided file path did not point to a valid file", nameof(dllFilePath));

            var dllBytes = File.ReadAllBytes(dllFilePath);

            _dllBytes = new ArraySegment<byte>(dllBytes);
            _fileResolver = new FileResolver(process, Path.GetDirectoryName(dllFilePath));
            _mappingFlags = mappingFlags;
            _peImage = new PEImage(_dllBytes);
            _processContext = new ProcessContext(process);
        }

        /// <summary>
        /// Maps the DLL into the process
        /// </summary>
        public void MapLibrary()
        {
            if (DllBaseAddress != IntPtr.Zero)
                return;

            var cleanupStack = new Stack<Action>();
            var wow64 = IntPtr.Zero;

            if (_processContext.Process.GetArchitecture() == Architecture.X64 && Process.GetCurrentProcess().GetArchitecture() == Architecture.X86)
                Kernel32.Wow64DisableWow64FsRedirection(ref wow64);

            try
            {
                AllocateImage();
                cleanupStack.Push(FreeImage);

                AllocateLoaderEntry();
                cleanupStack.Push(FreeLoaderEntry);

                LoadDependencies();
                cleanupStack.Push(FreeDependencies);

                BuildImportAddressTable();
                RelocateImage();
                MapHeaders();
                MapSections();

                InsertExceptionHandlers();
                cleanupStack.Push(RemoveExceptionHandlers);

                InitializeTlsData();
                cleanupStack.Push(ReleaseTlsData);

                CallInitializationRoutines(DllReason.ProcessAttach);
                EraseHeaders();
            } catch
            {
                while (cleanupStack.Count > 0)
                    Executor.IgnoreExceptions(cleanupStack.Pop());

                throw;
            } finally
            {
                if (_processContext.Process.GetArchitecture() == Architecture.X64 && Process.GetCurrentProcess().GetArchitecture() == Architecture.X86)
                    Kernel32.Wow64RevertWow64FsRedirection(wow64);
            }
        }

        /// <summary>
        /// Unmaps the DLL from the process
        /// </summary>
        public void UnmapLibrary()
        {
            if (DllBaseAddress == IntPtr.Zero)
                return;

            var topLevelException = default(Exception);

            try
            {
                CallInitializationRoutines(DllReason.ProcessDetach);
            }
            catch (Exception exception)
            {
                if (topLevelException == null)
                    topLevelException = exception;
            }

            try
            {
                ReleaseTlsData();
            }
            catch (Exception exception)
            {
                if (topLevelException == null)
                    topLevelException = exception;
            }

            try
            {
                RemoveExceptionHandlers();
            }
            catch (Exception exception)
            {
                if (topLevelException == null)
                    topLevelException = exception;
            }

            try
            {
                FreeDependencies();
            }
            catch (Exception exception)
            {
                if (topLevelException == null)
                    topLevelException = exception;
            }

            try
            {
                FreeImage();
            }
            catch (Exception exception)
            {
                if (topLevelException == null)
                    topLevelException = exception;
            }

            try
            {
                FreeLoaderEntry();
            }
            catch (Exception exception)
            {
                if (topLevelException == null)
                    topLevelException = exception;
            }

            if (topLevelException != null)
                throw topLevelException;
        }

        /// <summary>
        /// Get the absolute address of a function in the mapped library
        /// </summary>
        public IntPtr GetProcAddress(string functionName)
        {
            if (DllBaseAddress == IntPtr.Zero)
                return IntPtr.Zero;

            var function = _peImage.ExportDirectory.GetExportedFunction(functionName);
            if (!function.HasValue)
                return IntPtr.Zero;

            return (IntPtr)((ulong)DllBaseAddress + (ulong)function.Value.RelativeAddress);
        }

        private void AllocateImage()
        {
            DllBaseAddress = _processContext.Process.AllocateBuffer(_peImage.Headers.PEHeader.SizeOfImage, ProtectionType.ReadOnly);
        }

        private void AllocateLoaderEntry()
        {
            if (_peImage.Headers.PEHeader.ThreadLocalStorageTableDirectory.Size == 0)
                return;

            if (_processContext.Architecture == Architecture.X86)
            {
                _ldrEntryAddress = _processContext.Process.AllocateBuffer(Marshal.SizeOf(typeof(LdrDataTableEntry32)), ProtectionType.ReadWrite);
                var loaderEntry = new LdrDataTableEntry32((int)DllBaseAddress);
                _processContext.Process.WriteStruct(_ldrEntryAddress, loaderEntry);
            } else
            {
                _ldrEntryAddress = _processContext.Process.AllocateBuffer(Marshal.SizeOf(typeof(LdrDataTableEntry64)), ProtectionType.ReadWrite);
                var loaderEntry = new LdrDataTableEntry64((long)DllBaseAddress);
                _processContext.Process.WriteStruct(_ldrEntryAddress, loaderEntry);
            }
        }

        private void BuildImportAddressTable()
        {
            foreach (var importDescriptor in _peImage.ImportDirectory.GetImportDescriptors())
            {
                foreach (var function in importDescriptor.Functions)
                {
                    // Write the function address into the import address table
                    var functionAddress = function.Name == null ? _processContext.GetFunctionAddress(importDescriptor.Name, function.Ordinal) : _processContext.GetFunctionAddress(importDescriptor.Name, function.Name);
                    MemoryMarshal.Write(_dllBytes.Array, in functionAddress, _dllBytes.Offset + function.Offset);
                }
            }
        }

        private void CallInitializationRoutines(DllReason reason)
        {
            if ((_mappingFlags & MappingFlags.SkipInitRoutines) > 0)
                return;

            // Call the entry point of any TLS callbacks
            foreach (var callbackAddress in _peImage.TlsDirectory.GetTlsCallbacks().Select(callback => (ulong)DllBaseAddress + (ulong)callback.RelativeAddress))
                _processContext.CallRoutine(callbackAddress, CallingConvention.StdCall, DllBaseAddress, reason, 0);

            if (((_peImage.Headers.CorHeader?.Flags ?? 0) & CorFlags.ILOnly) > 0 || _peImage.Headers.PEHeader.AddressOfEntryPoint == 0)
                return;

            // Call the DLL entry point
            var entryPointAddress = (ulong)DllBaseAddress + (ulong)_peImage.Headers.PEHeader.AddressOfEntryPoint;

            if (!_processContext.CallRoutine<bool>(entryPointAddress, CallingConvention.StdCall, DllBaseAddress, reason, 0))
                throw new ApplicationException($"Failed to call the entry point with {reason:G}");
        }

        private void EraseHeaders()
        {
            if ((_mappingFlags & MappingFlags.DiscardHeaders) == 0)
                return;

            _processContext.Process.WriteSpan(DllBaseAddress, new ArraySegment<byte>(new byte[_peImage.Headers.PEHeader.SizeOfHeaders]));
        }

        private void FreeDependencies()
        {
            foreach(var import in _peImage.ImportDirectory.GetImportDescriptors())
            {
                // Free the dependency using the Windows loader
                var dependencyAddress = _processContext.GetModuleAddress(import.Name);

                if (!_processContext.CallRoutine<bool>(_processContext.GetFunctionAddress("kernel32.dll", "FreeLibrary"), CallingConvention.StdCall, dependencyAddress))
                    throw new ApplicationException($"Failed to free dependency {import.Name} from the process");
            }

            _processContext.ClearModuleCache();
        }

        private void FreeImage()
        {
            try
            {
                _processContext.Process.FreeBuffer(DllBaseAddress);
            } finally
            {
                DllBaseAddress = IntPtr.Zero;
            }
        }

        private void FreeLoaderEntry()
        {
            if (_peImage.Headers.PEHeader.ThreadLocalStorageTableDirectory.Size == 0)
                return;

            try
            {
                _processContext.Process.FreeBuffer(_ldrEntryAddress);
            } finally
            {
                _ldrEntryAddress = IntPtr.Zero;
            }
        }

        private void InitializeTlsData()
        {
            if (_peImage.Headers.PEHeader.ThreadLocalStorageTableDirectory.Size == 0)
                return;

            var status = _processContext.CallRoutine<NtStatus>(_processContext.GetNtdllSymbolAddress("LdrpHandleTlsData"), CallingConvention.FastCall, _ldrEntryAddress);

            if (!status.IsSuccess())
                throw new Win32Exception(Ntdll.RtlNtStatusToDosError(status));
        }

        private void InsertExceptionHandlers()
        {
            if (!_processContext.CallRoutine<bool>(_processContext.GetNtdllSymbolAddress("RtlInsertInvertedFunctionTable"), CallingConvention.FastCall, DllBaseAddress, _peImage.Headers.PEHeader.SizeOfImage))
                throw new ApplicationException("Failed to insert exception handlers");
        }

        private void LoadDependencies()
        {
            var activationContext = new ActivationContext(_peImage.ResourceDirectory.GetManifest(), _processContext.Architecture);

            foreach (var import in _peImage.ImportDirectory.GetImportDescriptors())
            {
                LoadDependency(import.Name, activationContext);

                var module = _processContext.GetModule(import.Name, null);

                foreach (var function in import.Functions)
                {
                    var descriptor = function.Name == null ?
                        module.PEImage.ExportDirectory.GetExportedFunction(function.Ordinal) : module.PEImage.ExportDirectory.GetExportedFunction(function.Name);

                    if (!descriptor.HasValue)
                        continue;

                    var fwd = descriptor.Value.ForwarderString;

                    if (fwd == null)
                        continue;

                    var moduleName = $"{fwd.Split('.')[0]}.dll";

                    try
                    {
                        _processContext.GetModule(moduleName, null);
                        continue;
                    } catch { }

                    LoadDependency(moduleName, activationContext);
                }
            }
        }

        private void LoadDependency(string name, ActivationContext activationContext)
        {
            // Write the dependency file path into the process
            var dependencyFilePath = _fileResolver.ResolveFilePath(_processContext.ResolveModuleName(name, null), activationContext);

            if (dependencyFilePath == null)
                throw new FileNotFoundException($"Failed to resolve the dependency file path for {name}");

            var dependencyFilePathAddress = _processContext.Process.AllocateBuffer(Encoding.Unicode.GetByteCount(dependencyFilePath), ProtectionType.ReadOnly);

            try
            {
                _processContext.Process.WriteString(dependencyFilePathAddress, dependencyFilePath);

                // Load the dependency using the Windows loader
                var dependencyAddress = _processContext.CallRoutinePointerWidth(_processContext.GetFunctionAddress("kernel32.dll", "LoadLibraryW"), CallingConvention.StdCall, dependencyFilePathAddress);

                if (dependencyAddress == 0)
                    throw new ApplicationException($"Failed to load the dependency {name} into the process");

                _processContext.RecordModuleLoad(dependencyAddress, dependencyFilePath);
            }
            finally
            {
                Executor.IgnoreExceptions(() => _processContext.Process.FreeBuffer(dependencyFilePathAddress));
            }
        }

        private void MapHeaders()
        {
            var headerBytes = _dllBytes.GetRange(end: _peImage.Headers.PEHeader.SizeOfHeaders);
            _processContext.Process.WriteSpan(DllBaseAddress, headerBytes);
        }

        private void MapSections()
        {
            var sectionHeaders = _peImage.Headers.SectionHeaders.AsEnumerable();

            if (_peImage.Headers.CorHeader == null || (_peImage.Headers.CorHeader.Flags & CorFlags.ILOnly) == 0)
                sectionHeaders = sectionHeaders.Where(sectionHeader => (sectionHeader.SectionCharacteristics & SectionCharacteristics.MemDiscardable) == 0);

            foreach (var sectionHeader in sectionHeaders)
            {
                var sectionAddress = (IntPtr)((long)DllBaseAddress + sectionHeader.VirtualAddress);

                // Map the raw section if not empty
                if (sectionHeader.SizeOfRawData > 0)
                {
                    var sectionBytes = _dllBytes.GetRange(sectionHeader.PointerToRawData, sectionHeader.PointerToRawData + sectionHeader.SizeOfRawData);
                    _processContext.Process.WriteSpan(sectionAddress, sectionBytes);
                }

                // Determine the protection to apply to the section
                ProtectionType sectionProtection;

                if ((sectionHeader.SectionCharacteristics & SectionCharacteristics.MemExecute) > 0)
                    if ((sectionHeader.SectionCharacteristics & SectionCharacteristics.MemWrite) > 0)
                        sectionProtection = (sectionHeader.SectionCharacteristics & SectionCharacteristics.MemRead) > 0 ? ProtectionType.ExecuteReadWrite : ProtectionType.ExecuteWriteCopy;
                    else
                        sectionProtection = (sectionHeader.SectionCharacteristics & SectionCharacteristics.MemRead) > 0 ? ProtectionType.ExecuteRead : ProtectionType.Execute;
                else if ((sectionHeader.SectionCharacteristics & SectionCharacteristics.MemWrite) > 0)
                    sectionProtection = (sectionHeader.SectionCharacteristics & SectionCharacteristics.MemRead) > 0 ? ProtectionType.ReadWrite : ProtectionType.WriteCopy;
                else
                    sectionProtection = (sectionHeader.SectionCharacteristics & SectionCharacteristics.MemRead) > 0 ? ProtectionType.ReadOnly : ProtectionType.NoAccess;

                if ((sectionHeader.SectionCharacteristics & SectionCharacteristics.MemNotCached) > 0)
                    sectionProtection |= ProtectionType.NoCache;

                // Calculate the aligned section size
                var sectionAlignment = _peImage.Headers.PEHeader.SectionAlignment;
                var alignedSectionSize = Math.Max(sectionHeader.SizeOfRawData, sectionHeader.VirtualSize);
                alignedSectionSize = alignedSectionSize + sectionAlignment - 1 - (alignedSectionSize + sectionAlignment - 1) % sectionAlignment;

                // Adjust the protection of the aligned section
                _processContext.Process.ProtectBuffer(sectionAddress, alignedSectionSize, sectionProtection);
            }
        }

        private void RelocateImage()
        {
            // Calculate the delta from the preferred base address and perform the needed relocations
            if (_processContext.Architecture == Architecture.X86)
            {
                var delta = (uint)DllBaseAddress - (uint)_peImage.Headers.PEHeader.ImageBase;

                foreach (var relocation in _peImage.RelocationDirectory.GetRelocations())
                {
                    if (relocation.Type != RelocationType.HighLow)
                        continue;

                    var relocationValue = MemoryMarshal.Read<uint>(_dllBytes.GetRange(relocation.Offset)) + delta;
                    MemoryMarshal.Write(_dllBytes, in relocationValue, relocation.Offset);
                }
            } else
            {
                var delta = (ulong)DllBaseAddress - _peImage.Headers.PEHeader.ImageBase;

                foreach (var relocation in _peImage.RelocationDirectory.GetRelocations())
                {
                    if (relocation.Type != RelocationType.Dir64)
                        continue;

                    var relocationValue = MemoryMarshal.Read<ulong>(_dllBytes.GetRange(relocation.Offset)) + delta;

                    MemoryMarshal.Write(_dllBytes, in relocationValue, relocation.Offset);
                }
            }
        }

        private void ReleaseTlsData()
        {
            if (_peImage.Headers.PEHeader.ThreadLocalStorageTableDirectory.Size == 0)
                return;

            var status = _processContext.CallRoutine<NtStatus>(_processContext.GetNtdllSymbolAddress("LdrpReleaseTlsEntry"), CallingConvention.FastCall, _ldrEntryAddress, 0);

            if (!status.IsSuccess())
                throw new Win32Exception(Ntdll.RtlNtStatusToDosError(status));
        }

        private void RemoveExceptionHandlers()
        {
            if (!_processContext.CallRoutine<bool>(_processContext.GetNtdllSymbolAddress("RtlRemoveInvertedFunctionTable"), CallingConvention.FastCall, DllBaseAddress))
                throw new ApplicationException("Failed to remove exception handlers");
        }
    }
}
