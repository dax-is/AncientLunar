using AncientLunar.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace AncientLunar.PortableExecutable.Native
{
    internal class PEReader : IDisposable
    {
        public PEHeaders PEHeaders
        {
            get
            {
                if (_lazyPEHeaders == null)
                    InitializePEHeaders();

                return _lazyPEHeaders;
            }
        }

        private Stream _peImage;
        private long _imageStart;
        private int _imageSize;
        private bool _leaveOpen;

        private PEHeaders _lazyPEHeaders;

        internal PEReader(Stream peStream, bool leaveOpen = false)
        {
            if (peStream == null)
                throw new ArgumentNullException("peStream");

            if (!peStream.CanRead || !peStream.CanSeek)
                throw new ArgumentException("Stream must be readable and seekable", "peSeek");

            _imageSize = StreamExtensions.GetAndValidateSize(peStream, 0, nameof(peStream));
            _imageStart = peStream.Position;
            _leaveOpen = leaveOpen;
            _peImage = peStream;
        }

        private void InitializePEHeaders()
        {
            _lazyPEHeaders = ReadPEHeaders(GetPEImage(), _imageStart, _imageSize);
        }

        private Stream GetPEImage()
        {
            if (_peImage == null)
            {
                if (_lazyPEHeaders == null)
                    throw new ObjectDisposedException(nameof(PEReader));

                throw new InvalidOperationException("PE image not available");
            }

            return _peImage;
        }

        public DebugDirectoryEntry[] ReadDebugDirectory()
        {
            var debugTableDirectory = PEHeaders.PEHeader.DebugTableDirectory;
            if (debugTableDirectory.Size == 0)
                return new DebugDirectoryEntry[0];

            if (!PEHeaders.TryGetDirectoryOffset(debugTableDirectory, out var offset))
                throw new Exception("Invalid directory RVA");

            if (debugTableDirectory.Size % 28 != 0)
                throw new Exception("Invalid directory size");

            return ReadDebugDirectoryEntries(GetPEImage(), offset, debugTableDirectory.Size);
        }

        public CodeViewDebugDirectoryData ReadCodeViewDebugDirectoryData(DebugDirectoryEntry entry)
        {
            if (entry.Type != DebugDirectoryEntryType.CodeView)
                throw new ArgumentException($"Unexpected debug directory type, excpected: CodeView, got: {entry.Type}");

            var offset = entry.DataPointer;
            var size = entry.DataSize;

            return DecodeCodeViewDebugDirectoryData(GetPEImage(), offset, size);
        }

        public void Dispose()
        {
            _lazyPEHeaders = null;

            if (!_leaveOpen)
                _peImage.Dispose();

            _peImage = null;
        }

        private static PEHeaders ReadPEHeaders(Stream stream, long imageStartPosition, int imageSize)
        {
            stream.Seek(imageStartPosition, SeekOrigin.Begin);
            return new PEHeaders(stream, imageSize);
        }

        private static DebugDirectoryEntry[] ReadDebugDirectoryEntries(Stream stream, int start, int size)
        {
            stream.Seek(start, SeekOrigin.Begin);

            var reader = new PEBinaryReader(stream, size);
            var entries = size / 28;
            var list = new List<DebugDirectoryEntry>();

            for (var i = 0; i < entries; i++)
            {
                if (reader.ReadInt32() != 0)
                    throw new BadImageFormatException("Invalid debug directory entry characteristics");

                var stamp = reader.ReadUInt32();
                var majorVersion = reader.ReadUInt16();
                var minorVersion = reader.ReadUInt16();
                var type = (DebugDirectoryEntryType)reader.ReadInt32();
                var dataSize = reader.ReadInt32();
                var dataRVA = reader.ReadInt32();
                var dataPointer = reader.ReadInt32();

                list.Add(new DebugDirectoryEntry(stamp, majorVersion, minorVersion, type, dataSize, dataRVA, dataPointer));
            }

            return list.ToArray();
        }

        private static CodeViewDebugDirectoryData DecodeCodeViewDebugDirectoryData(Stream stream, int start, int size)
        {
            stream.Seek(start, SeekOrigin.Begin);

            var reader = new PEBinaryReader(stream, size);
            if (reader.ReadByte() != 82 || reader.ReadByte() != 83 || reader.ReadByte() != 68 || reader.ReadByte() != 83)
                throw new BadImageFormatException("Unexpected code view data signature");

            var guid = reader.ReadGuid();
            var age = reader.ReadInt32();
            var path = reader.ReadUtf8NullTerminated();

            return new CodeViewDebugDirectoryData(guid, age, path);
        }
    }
}
