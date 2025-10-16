using AncientLunar.Interop;
using AncientLunar.Native;
using AncientLunar.Native.Enums;
using AncientLunar.Native.PInvoke;
using AncientLunar.Native.Structs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace AncientLunar.SymbolResolution
{
    internal class SymbolLookup
    {
        private readonly string _pdbFilePath;
        private readonly Dictionary<string, int> _symbolCache;

        internal SymbolLookup(Architecture architecture)
        {
            _pdbFilePath = SymbolDownloader.FindOrDownloadNtdllSymbols(architecture);
            _symbolCache = new Dictionary<string, int>();
        }

        internal int GetOffset(string symbolName)
        {
            if (_symbolCache.TryGetValue(symbolName, out var offset))
                return offset;

            // Load the PDB into a native symbol handler
            if ((Dbghelp.SymSetOptions(SymbolOptions.UndecorateName) & SymbolOptions.UndecorateName) == 0)
                throw new Win32Exception();

            if (!Dbghelp.SymInitialize((IntPtr)(-1), IntPtr.Zero, false))
                throw new Win32Exception();

            try
            {
                const int pseudoAddress = 0x1000;

                var pdbFileSize = new FileInfo(_pdbFilePath).Length;
                var symbolTableAddress = Dbghelp.SymLoadModule((IntPtr)(-1), IntPtr.Zero, _pdbFilePath, IntPtr.Zero, pseudoAddress, (int)pdbFileSize, IntPtr.Zero, 0);

                if (symbolTableAddress == 0)
                    throw new Win32Exception();

                // Retrieve the symbol info
                var symbolInfoBytes = new byte[(Marshal.SizeOf(typeof(SymbolInfo)) + sizeof(char) * Constants.MaxSymbolName + sizeof(long) - 1) / sizeof(long)];
                var symbolInfo = new SymbolInfo(Marshal.SizeOf(typeof(SymbolInfo)), 0, Constants.MaxSymbolName);

                var handle = GCHandle.Alloc(symbolInfoBytes, GCHandleType.Pinned);

                try
                {
                    Marshal.StructureToPtr(symbolInfo, handle.AddrOfPinnedObject(), false);

                    if (!Dbghelp.SymFromName((IntPtr)(-1), symbolName, handle.AddrOfPinnedObject()))
                        throw new Win32Exception();

                    symbolInfo = (SymbolInfo)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(SymbolInfo));
                }
                finally
                {
                    handle.Free();
                }

                offset = (int)(symbolInfo.Address - pseudoAddress);
                _symbolCache.Add(symbolName, offset);
            }
            finally
            {
                Dbghelp.SymCleanup((IntPtr)(-1));
            }

            return offset;
        }
    }
}
