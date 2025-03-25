using AncientLunar.Extensions;
using System;
using System.Diagnostics;
using System.IO;

namespace AncientLunar.Interop
{
    /// <summary>
    /// Indicates the processor architecture.
    /// </summary>
    public enum Architecture
    {
        /// <summary>
        /// An Intel-based 32-bit processor architecture.
        /// </summary>
        X86 = 0,

        /// <summary>
        /// An Intel-based 64-bit processor architecture.
        /// </summary>
        X64 = 1,

        /// <summary>
        /// A 32-bit ARM processor architecture.
        /// </summary>
        Arm = 2,

        /// <summary>
        /// A 64-bit ARM processor architecture.
        /// </summary>
        Arm64 = 3,

        /// <summary>
        /// The WebAssembly platform.
        /// </summary>
        Wasm = 4,

        /// <summary>
        /// The S390x platform architecture.
        /// </summary>
        S390x = 5,

        /// <summary>
        /// A LoongArch64 processor architecture.
        /// </summary>
        LoongArch64 = 6,

        /// <summary>
        /// A 32-bit ARMv6 processor architecture.
        /// </summary>
        Armv6 = 7,

        /// <summary>
        /// A PowerPC 64-bit (little-endian) processor architecture.
        /// </summary>
        Ppc64le = 8
    }

    internal static class ArchitectureExtensions
    {
        public static string GetSystemDirectory(this Architecture architecture)
        {
            var targetIs64Bit = architecture == Architecture.X64;
            var processIs64Bit = IntPtr.Size == 8;
            var windowsIs64Bit = processIs64Bit || Process.GetCurrentProcess().IsEmulating64();

            if ((processIs64Bit && targetIs64Bit) || !windowsIs64Bit)
                return Environment.SystemDirectory;
            else if (targetIs64Bit && !processIs64Bit)
                return Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "..\\Sysnative"));
            else /* if (!targetIs64Bit && processIs64Bit) */
                return Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "..\\SysWOW64"));
        }
    }
}
