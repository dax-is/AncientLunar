using AncientLunar.Helpers;
using AncientLunar.Native.Enums;
using AncientLunar.Native.Structs;
using AncientLunar.PortableExecutable.Native;
using AncientLunar.PortableExecutable.Structs;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AncientLunar.PortableExecutable.DataDirectories
{
    internal class RelocationDirectory : DataDirectoryBase
    {
        internal RelocationDirectory(ArraySegment<byte> imageBytes, PEHeaders headers) : base(imageBytes, headers, headers.PEHeader.BaseRelocationTableDirectory) { }

        public IEnumerable<Relocation> GetRelocations()
        {
            if (!IsValid)
                yield break;

            var currentOffset = DirectoryOffset;
            var maxOffset = DirectoryOffset + Headers.PEHeader.BaseRelocationTableDirectory.Size;

            while (currentOffset < maxOffset)
            {
                // Read the relocation block
                var relocationBlock = MemoryMarshal.Read<ImageBaseRelocation>(ImageBytes.GetRange(currentOffset));

                if (relocationBlock.SizeOfBlock == 0)
                    break;

                var relocationCount = (relocationBlock.SizeOfBlock - Marshal.SizeOf(typeof(ImageBaseRelocation))) / sizeof(short);

                for (var i = 0; i < relocationCount; i++)
                {
                    // Read the relocation
                    var relocationOffset = currentOffset + Marshal.SizeOf(typeof(ImageBaseRelocation)) + sizeof(short) * i;
                    var relocation = MemoryMarshal.Read<short>(ImageBytes.GetRange(relocationOffset));
                    var type = (ushort)relocation >> 12;
                    var offset = relocation & 0xFFF;

                    yield return new Relocation((RelocationType)type, RvaToOffset(relocationBlock.VirtualAddress) + offset);
                }

                currentOffset += relocationBlock.SizeOfBlock;
            }
        }
    }
}
