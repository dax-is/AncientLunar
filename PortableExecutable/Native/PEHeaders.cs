using AncientLunar.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace AncientLunar.PortableExecutable.Native
{
    public class PEHeaders
    {
        internal const ushort DosSignature = 23117;
        internal const int PESignatureOffsetLocation = 60;
        internal const uint PESignature = 17744U;
        internal const int PESignatureSize = 4;

        public int MetadataStartOffset => _metadataStartOffset;
        public int MetadataSize => _metadataSize;
        public CoffHeader CoffHeader => _coffHeader;
        public int CoffHeaderStartOffset => _coffHeaderStartOffset;
        public bool IsCoffOnly => _peHeader == null;
        public PEHeader PEHeader => _peHeader;
        public int PEHeaderStartOffset => _peHeaderStartOffset;
        public SectionHeader[] SectionHeaders => _sectionHeaders;
        public CorHeader CorHeader => _corHeader;
        public int CorHeaderStartOffset => _corHeaderStartOffset;
        public bool IsConsoleApplication => _peHeader != null && _peHeader.Subsystem == Subsystem.WindowsCui;
        public bool IsDll => (_coffHeader.Characteristics & Characteristics.Dll) > 0;
        public bool IsExe => (_coffHeader.Characteristics & Characteristics.Dll) == 0;

        private readonly CoffHeader _coffHeader;
        private readonly PEHeader _peHeader;
        private readonly SectionHeader[] _sectionHeaders;
        private readonly CorHeader _corHeader;
        private readonly int _metadataStartOffset = -1;
        private readonly int _metadataSize;
        private readonly int _coffHeaderStartOffset = -1;
        private readonly int _corHeaderStartOffset = -1;
        private readonly int _peHeaderStartOffset = -1;

        internal PEHeaders(Stream peStream, int size)
        {
            if (peStream == null)
                throw new ArgumentNullException(nameof(peStream));

            if (!peStream.CanRead || !peStream.CanSeek)
                throw new ArgumentException("Incompatible stream type", nameof(peStream));

            size = StreamExtensions.GetAndValidateSize(peStream, size, nameof(peStream));

            var reader = new PEBinaryReader(peStream, size);

            SkipDosHeader(ref reader, out var isCOFFOnly);

            _coffHeaderStartOffset = reader.CurrentOffset;
            _coffHeader = new CoffHeader(ref reader);

            if (!isCOFFOnly)
            {
                _peHeaderStartOffset = reader.CurrentOffset;
                _peHeader = new PEHeader(ref reader);
            }

            _sectionHeaders = ReadSectionHeaders(ref reader);

            if (!isCOFFOnly && TryCalculateCorHeaderOffset(out _corHeaderStartOffset))
            {
                reader.Seek(_corHeaderStartOffset);
                _corHeader = new CorHeader(ref reader);
            }

            CalculateMetadataLocation((long)size, out _metadataStartOffset, out _metadataSize);
        }

        private bool TryCalculateCorHeaderOffset(out int startOffset)
        {
            if (!TryGetDirectoryOffset(_peHeader.CorHeaderTableDirectory, out startOffset, false))
            {
                startOffset = -1;
                return false;
            }

            var size = _peHeader.CorHeaderTableDirectory.Size;
            if (size < 72)
                throw new Exception("Invalid COR header size");

            return true;
        }

        private SectionHeader[] ReadSectionHeaders(ref PEBinaryReader reader)
        {
            var sectionCount = _coffHeader.NumberOfSections;
            if (sectionCount < 0)
                throw new Exception("Invalid number of sections");

            var sections = new List<SectionHeader>();
            for (var i = 0; i < sectionCount; i++)
                sections.Add(new SectionHeader(ref reader));

            return sections.ToArray();
        }

        public bool TryGetDirectoryOffset(DirectoryEntry directory, out int offset)
        {
            return TryGetDirectoryOffset(directory, out offset, true);
        }

        internal bool TryGetDirectoryOffset(DirectoryEntry directory, out int offset, bool canCrossSectionBounary)
        {
            var sectionIndex = GetContainingSectionIndex(directory.RelativeVirtualAddress);
            if (sectionIndex < 0)
            {
                offset = -1;
                return false;
            }

            var relativeOffset = directory.RelativeVirtualAddress - _sectionHeaders[sectionIndex].VirtualAddress;
            if (!canCrossSectionBounary && directory.Size > _sectionHeaders[sectionIndex].VirtualSize - relativeOffset)
                throw new Exception("Section too small");

            offset = _sectionHeaders[sectionIndex].PointerToRawData + relativeOffset;
            return true;
        }

        public int GetContainingSectionIndex(int relativeVirtualAddress)
        {
            for (var i = 0; i < _sectionHeaders.Length; i++)
                if (_sectionHeaders[i].VirtualAddress <= relativeVirtualAddress && relativeVirtualAddress < _sectionHeaders[i].VirtualAddress + _sectionHeaders[i].VirtualSize)
                    return i;

            return -1;
        }

        internal int IndexOfSection(string name)
        {
            for (var i = 0; i < SectionHeaders.Length; i++)
                if (SectionHeaders[i].Name.Equals(name, StringComparison.Ordinal))
                    return i;

            return -1;
        }

        private void CalculateMetadataLocation(long peImageSize, out int start, out int size)
        {
            if (IsCoffOnly)
            {
                var sect = IndexOfSection(".cormeta");
                if (sect == -1)
                {
                    start = -1;
                    size = 0;
                    return;
                }

                start = SectionHeaders[sect].PointerToRawData;
                size = SectionHeaders[sect].SizeOfRawData;
            }
            else if (_corHeader == null)
            {
                start = 0;
                size = 0;

                return;
            }
            else
            {
                if (!TryGetDirectoryOffset(_corHeader.MetadataDirectory, out start, false))
                    throw new Exception("Missing data directory");

                size = _corHeader.MetadataDirectory.Size;
            }

            if (start < 0 || (long)start >= peImageSize || size <= 0 || (long)start > peImageSize - (long)size)
                throw new Exception("Invalid metadata section span");
        }

        private static void SkipDosHeader(ref PEBinaryReader reader, out bool isCOFFOnly)
        {
            var num = reader.ReadUInt16();
            if (num != DosSignature)
            {
                if (num == 0 && reader.ReadUInt16() == 65535)
                    throw new Exception("Unknown file format");

                isCOFFOnly = true;
                reader.Seek(0);
            }
            else isCOFFOnly = false;

            if (!isCOFFOnly)
            {
                reader.Seek(PESignatureOffsetLocation);

                var offset = reader.ReadInt32();
                reader.Seek(offset);
                
                var magic = reader.ReadUInt32();
                if (magic != PESignature)
                    throw new Exception("Invalid PE signature");
            }
        }
    }
}
