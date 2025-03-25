namespace AncientLunar.PortableExecutable.Native
{
    /// <summary>Represents the header of a COFF file.</summary>
	public sealed class CoffHeader
    {
        /// <summary>Gets the type of the target machine.</summary>
        /// <returns>The type of the target machine.</returns>
        public Machine Machine { get; }

        /// <summary>Gets the number of sections. This indicates the size of the section table, which immediately follows the headers.</summary>
        /// <returns>The number of sections.</returns>
        public short NumberOfSections { get; }

        /// <summary>Gets a value that indicates when the file was created.</summary>
        /// <returns>The low 32 bits of the number of seconds since 00:00 January 1, 1970, which indicates when the file was created.</returns>
        public int TimeDateStamp { get; }

        /// <summary>Gets the file pointer to the COFF symbol table.</summary>
        /// <returns>The file pointer to the COFF symbol table, or zero if no COFF symbol table is present. This value should be zero for a PE image.</returns>
        public int PointerToSymbolTable { get; }

        /// <summary>Gets the number of entries in the symbol table. This data can be used to locate the string table, which immediately follows the symbol table. This value should be zero for a PE image.</summary>
        public int NumberOfSymbols { get; }

        /// <summary>Gets the size of the optional header, which is required for executable files but not for object files. This value should be zero for an object file.</summary>
        /// <returns>The size of the optional header.</returns>
        public short SizeOfOptionalHeader { get; }

        /// <summary>Gets the flags that indicate the attributes of the file.</summary>
        /// <returns>The flags that indicate the attributes of the file.</returns>
        public Characteristics Characteristics { get; }

        internal CoffHeader(ref PEBinaryReader reader)
        {
            Machine = (Machine)reader.ReadUInt16();
            NumberOfSections = reader.ReadInt16();
            TimeDateStamp = reader.ReadInt32();
            PointerToSymbolTable = reader.ReadInt32();
            NumberOfSymbols = reader.ReadInt32();
            SizeOfOptionalHeader = reader.ReadInt16();
            Characteristics = (Characteristics)reader.ReadUInt16();
        }

        internal const int Size = 20;
    }
}