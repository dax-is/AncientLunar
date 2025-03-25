using System;

namespace AncientLunar.PortableExecutable.Native
{
    /// <summary>Describes the characteristics of a dynamic link library.</summary>
	[Flags]
    public enum DllCharacteristics : ushort
    {
        /// <summary>Reserved.</summary>
        ProcessInit = 1,
        /// <summary>Reserved.</summary>
        ProcessTerm = 2,
        /// <summary>Reserved.</summary>
        ThreadInit = 4,
        /// <summary>Reserved.</summary>
        ThreadTerm = 8,
        /// <summary>The image can handle a high entropy 64-bit virtual address space.</summary>
        HighEntropyVirtualAddressSpace = 32,
        /// <summary>The DLL can be relocated.</summary>
        DynamicBase = 64,
        /// <summary>Code integrity checks are enforced.</summary>
        ForceIntegrity = 128,
        /// <summary>The image is NX compatible.</summary>
        NxCompatible = 256,
        /// <summary>The image understands isolation and doesn't want it.</summary>
        NoIsolation = 512,
        /// <summary>The image does not use SEH. No SE handler may reside in this image.</summary>
        NoSeh = 1024,
        /// <summary>Do not bind this image.</summary>
        NoBind = 2048,
        /// <summary>The image must run inside an AppContainer.</summary>
        AppContainer = 4096,
        /// <summary>The driver uses the WDM model.</summary>
        WdmDriver = 8192,
        /// <summary>The image supports Control Flow Guard.</summary>
        ControlFlowGuard = 16384,
        /// <summary>The image is Terminal Server aware.</summary>
        TerminalServerAware = 32768
    }
}