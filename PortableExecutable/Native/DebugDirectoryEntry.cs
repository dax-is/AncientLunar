namespace AncientLunar.PortableExecutable.Native
{
    /// <summary>Identifies the location, size and format of a block of debug information.</summary>
	public readonly struct DebugDirectoryEntry
    {
        /// <summary>Get the time and date that the debug data was created if the PE/COFF file is not deterministic; otherwise, gets a value based on the hash of the content.</summary>
        /// <returns>for a non-deterministic PE/COFF file, the time and date that the debug data was created; otherwise, a value based on the hash of the content.</returns>
        public uint Stamp { get; }

        /// <summary>Gets the major version number of the debug data format.</summary>
        /// <returns>The major version number of the debug data format.</returns>
        public ushort MajorVersion { get; }

        /// <summary>Gets the minor version number of the debug data format.</summary>
        /// <returns>The minor version number of the debug data format.</returns>
        public ushort MinorVersion { get; }

        /// <summary>Gets the format of the debugging information.</summary>
        /// <returns>The format of the debugging information.</returns>
        public DebugDirectoryEntryType Type { get; }

        /// <summary>Gets the size of the debug data (not including the debug directory itself).</summary>
        /// <returns>the size of the debug data (excluding the debug directory).</returns>
        public int DataSize { get; }

        /// <summary>Gets the address of the debug data when loaded, relative to the image base.</summary>
        /// <returns>The address of the debug data relative to the image base.</returns>
        public int DataRelativeVirtualAddress { get; }

        /// <summary>Gets the file pointer to the debug data.</summary>
        /// <returns>The file pointer to the debug data.</returns>
        public int DataPointer { get; }

        /// <summary>Gets a value that indicates if the entry is a <see cref="F:AncientLunar.PortableExecutable.Native.DebugDirectoryEntryType.CodeView" /> entry that points to a Portable PDB.</summary>
        /// <returns>
        ///   <see langword="true" /> if the entry is a <see cref="F:AncientLunar.PortableExecutable.Native.DebugDirectoryEntryType.CodeView" /> entry pointing to a Portable PDB; otherwise, <see langword="false" />.</returns>
        public bool IsPortableCodeView
        {
            get
            {
                return this.MinorVersion == 20557;
            }
        }

        /// <summary>Initializes a new instance of the <see cref="T:AncientLunar.PortableExecutable.Native.DebugDirectoryEntry" /> structure.</summary>
        /// <param name="stamp" />
        /// <param name="majorVersion" />
        /// <param name="minorVersion" />
        /// <param name="type" />
        /// <param name="dataSize" />
        /// <param name="dataRelativeVirtualAddress" />
        /// <param name="dataPointer" />
        public DebugDirectoryEntry(uint stamp, ushort majorVersion, ushort minorVersion, DebugDirectoryEntryType type, int dataSize, int dataRelativeVirtualAddress, int dataPointer)
        {
            this.Stamp = stamp;
            this.MajorVersion = majorVersion;
            this.MinorVersion = minorVersion;
            this.Type = type;
            this.DataSize = dataSize;
            this.DataRelativeVirtualAddress = dataRelativeVirtualAddress;
            this.DataPointer = dataPointer;
        }

        internal const int Size = 28;
    }
}
