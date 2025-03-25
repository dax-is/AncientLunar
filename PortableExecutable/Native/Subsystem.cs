namespace AncientLunar.PortableExecutable.Native
{
    /// <summary>Describes the subsystem requirement for the image.</summary>
	public enum Subsystem : ushort
    {
        /// <summary>Unknown subsystem.</summary>
        Unknown,
        /// <summary>The image doesn't require a subsystem.</summary>
        Native,
        /// <summary>The image runs in the Windows GUI subsystem.</summary>
        WindowsGui,
        /// <summary>The image runs in the Windows character subsystem.</summary>
        WindowsCui,
        /// <summary>The image runs in the OS/2 character subsystem.</summary>
        OS2Cui = 5,
        /// <summary>The image runs in the Posix character subsystem.</summary>
        PosixCui = 7,
        /// <summary>The image is a native Win9x driver.</summary>
        NativeWindows,
        /// <summary>The image runs in the Windows CE subsystem.</summary>
        WindowsCEGui,
        /// <summary>Extensible Firmware Interface (EFI) application.</summary>
        EfiApplication,
        /// <summary>EFI driver with boot services.</summary>
        EfiBootServiceDriver,
        /// <summary>EFI driver with run-time services.</summary>
        EfiRuntimeDriver,
        /// <summary>EFI ROM image.</summary>
        EfiRom,
        /// <summary>Xbox system.</summary>
        Xbox,
        /// <summary>Boot application.</summary>
        WindowsBootApplication = 16
    }
}