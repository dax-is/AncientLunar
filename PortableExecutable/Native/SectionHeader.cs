namespace AncientLunar.PortableExecutable.Native
{
    /// <summary>Provides information about the section header of a PE/COFF file.</summary>
    public readonly struct SectionHeader
    {
        /// <summary>Gets the name of the section.</summary>
        /// <returns>The name of the section.</returns>
        public string Name { get; }

        /// <summary>Gets the total size of the section when loaded into memory.</summary>
        /// <returns>The total size of the section when loaded into memory.</returns>
        public int VirtualSize { get; }

        /// <summary>Gets the virtual addess of the section.</summary>
        /// <returns>The virtual address of the section.</returns>
        public int VirtualAddress { get; }

        /// <summary>Gets the size of the section (for object files) or the size of the initialized data on disk (for image files).</summary>
        /// <returns>The size of the section (for object files) or the size of the initialized data on disk (for image files).</returns>
        public int SizeOfRawData { get; }

        /// <summary>Gets the file pointer to the first page of the section within the COFF file.</summary>
        /// <returns>The file pointer to the first page of the section within the COFF file.</returns>
        public int PointerToRawData { get; }

        /// <summary>Gets the file pointer to the beginning of relocation entries for the section.</summary>
        /// <returns>The file pointer to the beginning of relocation entries for the section. It is set to zero for PE images or if there are no relocations.</returns>
        public int PointerToRelocations { get; }

        /// <summary>Gets the file pointer to the beginning of line-number entries for the section.</summary>
        /// <returns>The file pointer to the beginning of line-number entries for the section, or zero if there are no COFF line numbers.</returns>
        public int PointerToLineNumbers { get; }

        /// <summary>Gets the number of relocation entries for the section.</summary>
        /// <returns>The number of relocation entries for the section. Its value is zero for PE images.</returns>
        public ushort NumberOfRelocations { get; }

        /// <summary>Gets the number of line-number entries for the section.</summary>
        /// <returns>The number of line-number entries for the section.</returns>
        public ushort NumberOfLineNumbers { get; }

        /// <summary>Gets the flags that describe the characteristics of the section.</summary>
        /// <returns>The flags that describe the characteristics of the section.</returns>
        public SectionCharacteristics SectionCharacteristics { get; }

        internal SectionHeader(ref PEBinaryReader reader)
        {
            this.Name = reader.ReadNullPaddedUTF8(8);
            this.VirtualSize = reader.ReadInt32();
            this.VirtualAddress = reader.ReadInt32();
            this.SizeOfRawData = reader.ReadInt32();
            this.PointerToRawData = reader.ReadInt32();
            this.PointerToRelocations = reader.ReadInt32();
            this.PointerToLineNumbers = reader.ReadInt32();
            this.NumberOfRelocations = reader.ReadUInt16();
            this.NumberOfLineNumbers = reader.ReadUInt16();
            this.SectionCharacteristics = (SectionCharacteristics)reader.ReadUInt32();
        }

        internal const int NameSize = 8;

        internal const int Size = 40;
    }
}
