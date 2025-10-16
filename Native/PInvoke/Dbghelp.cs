using AncientLunar.Native.Enums;
using System;
using System.Runtime.InteropServices;

namespace AncientLunar.Native.PInvoke
{
    internal static class Dbghelp
    {
        [DllImport("dbghelp.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SymCleanup(IntPtr processHandle);

        [DllImport("dbghelp.dll", EntryPoint = "SymFromNameW", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SymFromName(IntPtr processHandle, string name, IntPtr symbolInfo);

        [DllImport("dbghelp.dll", EntryPoint = "SymInitializeW", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SymInitialize(IntPtr processHandle, IntPtr searchPath, [MarshalAs(UnmanagedType.Bool)] bool invadeProcess);

        [DllImport("dbghelp.dll", EntryPoint = "SymLoadModuleExW", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern long SymLoadModule(IntPtr processHandle, IntPtr fileHandle, string imageName, IntPtr moduleName, long dllBase, int dllSize, IntPtr data, int flags);

        [DllImport("dbghelp.dll", SetLastError = true)]
        internal static extern SymbolOptions SymSetOptions(SymbolOptions options);
    }
}
