namespace AncientLunar.PortableExecutable.Native
{
    /// <summary>Specifies the target machine's CPU architecture.</summary>
    public enum Machine : ushort
    {
        /// <summary>The target CPU is unknown or not specified.</summary>
        Unknown,
        /// <summary>Intel 386.</summary>
        I386 = 332,
        /// <summary>MIPS little-endian WCE v2.</summary>
        WceMipsV2 = 361,
        /// <summary>Alpha.</summary>
        Alpha = 388,
        /// <summary>Hitachi SH3 little endian.</summary>
        SH3 = 418,
        /// <summary>Hitachi SH3 DSP.</summary>
        SH3Dsp,
        /// <summary>Hitachi SH3 little endian.</summary>
        SH3E,
        /// <summary>Hitachi SH4 little endian.</summary>
        SH4 = 422,
        /// <summary>Hitachi SH5.</summary>
        SH5 = 424,
        /// <summary>ARM little endian.</summary>
        Arm = 448,
        /// <summary>Thumb.</summary>
        Thumb = 450,
        /// <summary>ARM Thumb-2 little endian.</summary>
        ArmThumb2 = 452,
        /// <summary>Matsushita AM33.</summary>
        AM33 = 467,
        /// <summary>IBM PowerPC little endian.</summary>
        PowerPC = 496,
        /// <summary>PowerPCFP.</summary>
        PowerPCFP,
        /// <summary>Intel 64.</summary>
        IA64 = 512,
        /// <summary>MIPS.</summary>
        MIPS16 = 614,
        /// <summary>ALPHA64.</summary>
        Alpha64 = 644,
        /// <summary>MIPS with FPU.</summary>
        MipsFpu = 870,
        /// <summary>MIPS16 with FPU.</summary>
        MipsFpu16 = 1126,
        /// <summary>Infineon.</summary>
        Tricore = 1312,
        /// <summary>EFI Byte Code.</summary>
        Ebc = 3772,
        /// <summary>AMD64 (K8).</summary>
        Amd64 = 34404,
        /// <summary>M32R little-endian.</summary>
        M32R = 36929,
        /// <summary>ARM64.</summary>
        Arm64 = 43620,
        /// <summary>LOONGARCH32</summary>
        LoongArch32 = 25138,
        /// <summary>LOONGARCH64</summary>
        LoongArch64 = 25188
    }
}
