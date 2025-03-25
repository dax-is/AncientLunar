using System;

namespace AncientLunar.PortableExecutable.Native
{
    /// <summary>Represents the Portable Executable (PE) file header.</summary>
	public sealed class PEHeader
    {
        /// <summary>Gets a value that identifies the format of the image file.</summary>
        /// <returns>The format of the image file.</returns>
        public PEMagic Magic { get; }

        /// <summary>Gets the linker major version number.</summary>
        /// <returns>The linker major version number.</returns>
        public byte MajorLinkerVersion { get; }

        /// <summary>Gets the linker minor version number.</summary>
        /// <returns>The linker minor version number.</returns>
        public byte MinorLinkerVersion { get; }

        /// <summary>Gets the size of the code (text) section, or the sum of all code sections if there are multiple sections.</summary>
        /// <returns>the size of the code (text) section, or the sum of all code sections if there are multiple sections.</returns>
        public int SizeOfCode { get; }

        /// <summary>Gets the size of the initialized data section, or the sum of all such sections if there are multiple data sections.</summary>
        public int SizeOfInitializedData { get; }

        /// <summary>Gets the size of the uninitialized data section (BSS), or the sum of all such sections if there are multiple BSS sections.</summary>
        /// <returns>The size of the uninitialized data section (BSS) or the sum of all such sections.</returns>
        public int SizeOfUninitializedData { get; }

        /// <summary>Gets the address of the entry point relative to the image base when the PE file is loaded into memory.</summary>
        /// <returns>The address of the entry point relative to the image base.</returns>
        public int AddressOfEntryPoint { get; }

        /// <summary>Gets the address of the beginning-of-code section relative to the image base when the image is loaded into memory.</summary>
        /// <returns>The address of the beginning-of-code section relative to the image base.</returns>
        public int BaseOfCode { get; }

        /// <summary>Gets the address of the beginning-of-data section relative to the image base when the image is loaded into memory.</summary>
        /// <returns>The address of the beginning-of-data section relative to the image base.</returns>
        public int BaseOfData { get; }

        /// <summary>Gets the preferred address of the first byte of the image when it is loaded into memory.</summary>
        /// <returns>The preferred address, which is a multiple of 64K.</returns>
        public ulong ImageBase { get; }

        /// <summary>Gets the alignment (in bytes) of sections when they are loaded into memory.</summary>
        /// <returns>A number greater than or equal to <see cref="P:System.Reflection.PortableExecutable.PEHeader.FileAlignment" />. The default is the page size for the architecture.</returns>
        public int SectionAlignment { get; }

        /// <summary>Gets the alignment factor (in bytes) that is used to align the raw data of sections in the image file.</summary>
        /// <returns>A power of 2 between 512 and 64K, inclusive. The default is 512.</returns>
        public int FileAlignment { get; }

        /// <summary>Gets the major version number of the required operating system.</summary>
        /// <returns>The major version number of the required operating system.</returns>
        public ushort MajorOperatingSystemVersion { get; }

        /// <summary>Gets the minor version number of the required operating system.</summary>
        /// <returns>The minor version number of the required operating system.</returns>
        public ushort MinorOperatingSystemVersion { get; }

        /// <summary>Gets the major version number of the image.</summary>
        /// <returns>The major version number of the image.</returns>
        public ushort MajorImageVersion { get; }

        /// <summary>Gets the minor version number of the image.</summary>
        /// <returns>The minor version number of the image.</returns>
        public ushort MinorImageVersion { get; }

        /// <summary>Gets the major version number of the subsystem.</summary>
        /// <returns>The major version number of the subsystem.</returns>
        public ushort MajorSubsystemVersion { get; }

        /// <summary>Gets the minor version number of the subsystem.</summary>
        /// <returns>The minor version number of the subsystem.</returns>
        public ushort MinorSubsystemVersion { get; }

        /// <summary>Gets the size (in bytes) of the image, including all headers, as the image is loaded in memory.</summary>
        /// <returns>The size (in bytes) of the image, which is a multiple of <see cref="P:AncientLunar.PortableExecutable.Native.PEHeader.SectionAlignment" />.</returns>
        public int SizeOfImage { get; }

        /// <summary>Gets the combined size of an MS DOS stub, PE header, and section headers rounded up to a multiple of FileAlignment.</summary>
        /// <returns>The combined size of an MS DOS stub, PE header, and section headers rounded up to a multiple of FileAlignment.</returns>
        public int SizeOfHeaders { get; }

        /// <summary>Gets the image file checksum.</summary>
        /// <returns>The image file checksum.</returns>
        public uint CheckSum { get; }

        /// <summary>Gets the name of the subsystem that is required to run this image.</summary>
        /// <returns>The name of the subsystem that is required to run this image.</returns>
        public Subsystem Subsystem { get; }

        /// <summary>Gets the characteristics of a dynamic link library.</summary>
        /// <returns>A bitwise combination of flags that represents the characteristics of a dynamic link library.</returns>
        public DllCharacteristics DllCharacteristics { get; }

        /// <summary>Gets the size of the stack to reserve. Only <see cref="P:AncientLunar.PortableExecutable.Native.PEHeader.SizeOfStackCommit" /> is committed; the rest is made available one page at a time until the reserve size is reached.</summary>
        /// <returns>The size of the stack to reserve.</returns>
        public ulong SizeOfStackReserve { get; }

        /// <summary>Gets the size of the stack to commit.</summary>
        /// <returns>The size of the stack to commit.</returns>
        public ulong SizeOfStackCommit { get; }

        /// <summary>Gets the size of the local heap space to reserve. Only <see cref="P:AncientLunar.PortableExecutable.Native.PEHeader.SizeOfHeapCommit" /> is committed; the rest is made available one page at a time until the reserve size is reached.</summary>
        /// <returns>The size of the local heap space to reserve.</returns>
        public ulong SizeOfHeapReserve { get; }

        /// <summary>Gets the size of the local heap space to commit.</summary>
        /// <returns>the size of the local heap space to commit.</returns>
        public ulong SizeOfHeapCommit { get; }

        /// <summary>Gets the number of data-directory entries in the remainder of the <see cref="T:AncientLunar.PortableExecutable.Native.PEHeader" />. Each describes a location and size.</summary>
        /// <returns>The number of data-directory entries in the remainder of the <see cref="T:AncientLunar.PortableExecutable.Native.PEHeader" />.</returns>
        public int NumberOfRvaAndSizes { get; }

        /// <summary>Gets the Export Table entry.</summary>
        /// <returns>The Export Table entry.</returns>
        public DirectoryEntry ExportTableDirectory { get; }

        /// <summary>Gets the Import Table entry.</summary>
        /// <returns>The Import Table entry.</returns>
        public DirectoryEntry ImportTableDirectory { get; }

        /// <summary>Gets the Resource Table entry.</summary>
        /// <returns>The Resource Table entry.</returns>
        public DirectoryEntry ResourceTableDirectory { get; }

        /// <summary>Gets the Exception Table entry.</summary>
        /// <returns>The Exception Table entry.</returns>
        public DirectoryEntry ExceptionTableDirectory { get; }

        /// <summary>Gets the Certificate Table entry, which points to a table of attribute certificates.</summary>
        public DirectoryEntry CertificateTableDirectory { get; }

        /// <summary>Gets the Base Relocations Table entry.</summary>
        /// <returns>The Base Relocations Table entry.</returns>
        public DirectoryEntry BaseRelocationTableDirectory { get; }

        /// <summary>Gets the Debug Table entry.</summary>
        /// <returns>The Debug Table entry.</returns>
        public DirectoryEntry DebugTableDirectory { get; }

        /// <summary>Gets the Copyright Table entry.</summary>
        /// <returns>The Copyright Table entry.</returns>
        public DirectoryEntry CopyrightTableDirectory { get; }

        /// <summary>Gets the Global Pointer Table entry.</summary>
        /// <returns>The Global Pointer Table entry.</returns>
        public DirectoryEntry GlobalPointerTableDirectory { get; }

        /// <summary>Gets the Thread-Local Storage Table entry.</summary>
        /// <returns>The Thread-Local Storage Table entry.</returns>
        public DirectoryEntry ThreadLocalStorageTableDirectory { get; }

        /// <summary>Gets the Load Configuration Table entry.</summary>
        /// <returns>The Load Configuration Table entry.</returns>
        public DirectoryEntry LoadConfigTableDirectory { get; }

        /// <summary>Gets the Bound Import Table entry.</summary>
        /// <returns>The Bound Import Table entry.</returns>
        public DirectoryEntry BoundImportTableDirectory { get; }

        /// <summary>Gets the Import Address Table entry.</summary>
        /// <returns>The Import Address Table entry.</returns>
        public DirectoryEntry ImportAddressTableDirectory { get; }

        /// <summary>Gets the Delay-Load Import Table entry.</summary>
        /// <returns>The Delay-Load Import Table entry.</returns>
        public DirectoryEntry DelayImportTableDirectory { get; }

        /// <summary>Gets the CLI Header Table entry.</summary>
        /// <returns>The CLI Header Table entry.</returns>
        public DirectoryEntry CorHeaderTableDirectory { get; }

        internal static int Size(bool is32Bit)
        {
            return 72 + 4 * (is32Bit ? 4 : 8) + 4 + 4 + 128;
        }

        internal PEHeader(ref PEBinaryReader reader)
        {
            var pemagic = (PEMagic)reader.ReadUInt16();
            if (pemagic != PEMagic.PE32 && pemagic != PEMagic.PE32Plus)
                throw new Exception("Unknown PE magic value");
            
            Magic = pemagic;
            MajorLinkerVersion = reader.ReadByte();
            MinorLinkerVersion = reader.ReadByte();
            SizeOfCode = reader.ReadInt32();
            SizeOfInitializedData = reader.ReadInt32();
            SizeOfUninitializedData = reader.ReadInt32();
            AddressOfEntryPoint = reader.ReadInt32();
            BaseOfCode = reader.ReadInt32();
            
            if (pemagic == PEMagic.PE32Plus)
                BaseOfData = 0;
            else
                BaseOfData = reader.ReadInt32();
            
            if (pemagic == PEMagic.PE32Plus)
                ImageBase = reader.ReadUInt64();
            else
                ImageBase = reader.ReadUInt32();
            
            SectionAlignment = reader.ReadInt32();
            FileAlignment = reader.ReadInt32();
            MajorOperatingSystemVersion = reader.ReadUInt16();
            MinorOperatingSystemVersion = reader.ReadUInt16();
            MajorImageVersion = reader.ReadUInt16();
            MinorImageVersion = reader.ReadUInt16();
            MajorSubsystemVersion = reader.ReadUInt16();
            MinorSubsystemVersion = reader.ReadUInt16();
            
            // ignore
            reader.ReadUInt32();
            
            SizeOfImage = reader.ReadInt32();
            SizeOfHeaders = reader.ReadInt32();
            CheckSum = reader.ReadUInt32();
            Subsystem = (Subsystem)reader.ReadUInt16();
            DllCharacteristics = (DllCharacteristics)reader.ReadUInt16();
            
            if (pemagic == PEMagic.PE32Plus)
            {
                SizeOfStackReserve = reader.ReadUInt64();
                SizeOfStackCommit = reader.ReadUInt64();
                SizeOfHeapReserve = reader.ReadUInt64();
                SizeOfHeapCommit = reader.ReadUInt64();
            }
            else
            {
                SizeOfStackReserve = reader.ReadUInt32();
                SizeOfStackCommit = reader.ReadUInt32();
                SizeOfHeapReserve = reader.ReadUInt32();
                SizeOfHeapCommit = reader.ReadUInt32();
            }
            
            // ignore
            reader.ReadUInt32();
            
            NumberOfRvaAndSizes = reader.ReadInt32();
            ExportTableDirectory = new DirectoryEntry(ref reader);
            ImportTableDirectory = new DirectoryEntry(ref reader);
            ResourceTableDirectory = new DirectoryEntry(ref reader);
            ExceptionTableDirectory = new DirectoryEntry(ref reader);
            CertificateTableDirectory = new DirectoryEntry(ref reader);
            BaseRelocationTableDirectory = new DirectoryEntry(ref reader);
            DebugTableDirectory = new DirectoryEntry(ref reader);
            CopyrightTableDirectory = new DirectoryEntry(ref reader);
            GlobalPointerTableDirectory = new DirectoryEntry(ref reader);
            ThreadLocalStorageTableDirectory = new DirectoryEntry(ref reader);
            LoadConfigTableDirectory = new DirectoryEntry(ref reader);
            BoundImportTableDirectory = new DirectoryEntry(ref reader);
            ImportAddressTableDirectory = new DirectoryEntry(ref reader);
            DelayImportTableDirectory = new DirectoryEntry(ref reader);
            CorHeaderTableDirectory = new DirectoryEntry(ref reader);
            
            // ignore
            new DirectoryEntry(ref reader);
        }

        internal const int OffsetOfChecksum = 64;
    }
}
