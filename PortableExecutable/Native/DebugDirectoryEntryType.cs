namespace AncientLunar.PortableExecutable.Native
{
    /// <summary>An enumeration that describes the format of the debugging information of a <see cref="T:AncientLunar.PortableExecutable.Native.DebugDirectoryEntry" />.</summary>
	public enum DebugDirectoryEntryType
    {
        /// <summary>An unknown value that should be ignored by all tools.</summary>
        Unknown,
        /// <summary>The COFF debug information (line numbers, symbol table, and string table). This type of debug information is also pointed to by fields in the file headers.</summary>
        Coff,
        /// <summary>Associated PDB file description. For more information, see the specification.</summary>
        CodeView,
        /// <summary>
        ///   <para>The presence of this entry indicates a deterministic PE/COFF file. See the Remarks section for more information.</para>
        ///   <para>The tool that produced the deterministic PE/COFF file guarantees that the entire content of the file is based solely on documented inputs given to the tool (such as source files, resource files, and compiler options) rather than ambient environment variables (such as the current time, the operating system, and the bitness of the process running the tool).
        ///     The value of field TimeDateStamp in COFF File Header of a deterministic PE/COFF file does not indicate the date and time when the file was produced and should not be interpreted that way. Instead, the value of the field is derived from a hash of the file content. The algorithm to calculate this value is an implementation detail of the tool that produced the file.
        ///     The debug directory entry of type <see cref="F:AncientLunar.PortableExecutable.Native.DebugDirectoryEntryType.Reproducible" /> must have all fields, except for Type zeroed.</para>
        ///   <para>For more information, see the specification.</para>
        /// </summary>
        Reproducible = 16,
        /// <summary>
        ///   <para>The entry points to a blob containing Embedded Portable PDB. The Embedded Portable PDB blob has the following format:</para>
        ///   <para>- blob ::= uncompressed-size data</para>
        ///   <para>- Data spans the remainder of the blob and contains a Deflate-compressed Portable PDB.</para>
        ///   <para>For more information, see the specification.</para>
        /// </summary>
        EmbeddedPortablePdb,
        /// <summary>The entry stores a crypto hash of the content of the symbol file the PE/COFF file was built with. The hash can be used to validate that a given PDB file was built with the PE/COFF file and not altered in any way. More than one entry can be present if multiple PDBs were produced during the build of the PE/COFF file (for example, private and public symbols). For more information, see the specification.</summary>
        PdbChecksum = 19
    }
}
