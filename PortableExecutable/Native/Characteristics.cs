using System;

namespace AncientLunar.PortableExecutable.Native
{
    /// <summary>Represents the Common Object File Format (COFF) file characteristics.</summary>
    [Flags]
    public enum Characteristics : ushort
    {
        /// <summary>Indicates that the image doesn't contain base relocations and must therefore be loaded at its preferred base address.</summary>
        RelocsStripped = 1,
        /// <summary>Indicates that the image file is valid and can be run.</summary>
        ExecutableImage = 2,
        /// <summary>Indicates that COFF line numbers have been removed from the file. This flag is deprecated and should be zero.</summary>
        LineNumsStripped = 4,
        /// <summary>Indicates that COFF symbol table entries for local symbols have been removed from the file. This flag is deprecated and should be zero.</summary>
        LocalSymsStripped = 8,
        /// <summary>Indicates that the operating system should aggressively trim the working set for this file. This flag is deprecated and should be zero.</summary>
        AggressiveWSTrim = 16,
        /// <summary>Indicates that this executable file can handle memory addresses greater than 2 GB.</summary>
        LargeAddressAware = 32,
        /// <summary>Indicates that this file uses a little endian byte order. This flag is deprecated and should be zero.</summary>
        BytesReversedLo = 128,
        /// <summary>Indicates that this file is for a 32-bit machine.</summary>
        Bit32Machine = 256,
        /// <summary>Indicates that debugging information is removed from the image file.</summary>
        DebugStripped = 512,
        /// <summary>Indicates that the image should be fully loaded and copied into the swap file if it's located on a removable media.</summary>
        RemovableRunFromSwap = 1024,
        /// <summary>Indicates that the image should be fully loaded and copied into the swap file if it's located on a network media.</summary>
        NetRunFromSwap = 2048,
        /// <summary>Indicates that the image file is a system file, not a user program.</summary>
        System = 4096,
        /// <summary>Indicates that the image file is a dynamic-link library (DLL).</summary>
        Dll = 8192,
        /// <summary>Indicates that the file should be run only on a uniprocessor machine.</summary>
        UpSystemOnly = 16384,
        /// <summary>Indicates that this file uses a big endian byte order. This flag is deprecated and should be zero.</summary>
        BytesReversedHi = 32768
    }
}
