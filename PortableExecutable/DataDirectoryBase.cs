using AncientLunar.PortableExecutable.Native;
using System;

namespace AncientLunar.PortableExecutable
{
    internal abstract class DataDirectoryBase
    {
        private protected int DirectoryOffset { get; }
        private protected PEHeaders Headers { get; }
        private protected ArraySegment<byte> ImageBytes { get; }
        private protected bool IsValid { get; }

        private protected DataDirectoryBase(ArraySegment<byte> imageBytes, PEHeaders headers, DirectoryEntry directory)
        {
            headers.TryGetDirectoryOffset(directory, out var directoryOffset);

            DirectoryOffset = directoryOffset;
            Headers = headers;
            ImageBytes = imageBytes;
            IsValid = directoryOffset != -1;
        }

        private protected int RvaToOffset(int rva)
        {
            var sectionHeader = Headers.SectionHeaders[Headers.GetContainingSectionIndex(rva)];
            return rva - sectionHeader.VirtualAddress + sectionHeader.PointerToRawData;
        }

        private protected int VaToRva(int va)
        {
            return va - (int)Headers.PEHeader.ImageBase;
        }

        private protected int VaToRva(long va)
        {
            return (int)(va - (long)Headers.PEHeader.ImageBase);
        }
    }
}
