using System;

namespace AncientLunar.PortableExecutable.Native
{
    /// <summary>Represents the runtime flags for a .NET executable image.</summary>
    [Flags]
    public enum CorFlags
    {
        /// <summary>Indicates that this image only contains IL code.</summary>
        ILOnly = 1,
        /// <summary>Indicates that this image can only be loaded into a 32-bit process.</summary>
        Requires32Bit = 2,
        /// <summary>Indicates that this image is a library that only contains IL code.</summary>
        ILLibrary = 4,
        /// <summary>Indicates that this image has a strong name signature.</summary>
        StrongNameSigned = 8,
        /// <summary>Reserved, shall be 0.</summary>
        NativeEntryPoint = 16,
        /// <summary>Reserved, shall be 0.</summary>
        TrackDebugData = 65536,
        /// <summary>Indicates that this image should be run as a 32-bit process on a 64-bit operating system.</summary>
        Prefers32Bit = 131072
    }
}
