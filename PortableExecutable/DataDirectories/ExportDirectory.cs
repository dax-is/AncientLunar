using AncientLunar.Helpers;
using AncientLunar.Native.Structs;
using AncientLunar.PortableExecutable.Native;
using AncientLunar.PortableExecutable.Structs;
using System;
using System.Text;

namespace AncientLunar.PortableExecutable.DataDirectories
{
    internal class ExportDirectory : DataDirectoryBase
    {
        internal ExportDirectory(ArraySegment<byte> imageBytes, PEHeaders headers) : base(imageBytes, headers, headers.PEHeader.ExportTableDirectory) { }

        public ExportedFunction? GetExportedFunction(string functionName)
        {
            if (!IsValid)
                return null;

            // Search the name table for the function
            var exportDirectory = MemoryMarshal.Read<ImageExportDirectory>(ImageBytes.GetRange(DirectoryOffset));

            var low = 0;
            var high = exportDirectory.NumberOfNames - 1;

            while (low <= high)
            {
                var middle = (low + high) / 2;

                // Read the current name
                var currentNameOffsetOffset = RvaToOffset(exportDirectory.AddressOfNames) + sizeof(int) * middle;
                var currentNameOffset = RvaToOffset(MemoryMarshal.Read<int>(ImageBytes.GetRange(currentNameOffsetOffset)));
                var currentNameLength = Array.IndexOf(ImageBytes.GetRange(currentNameOffset).ToArray(), byte.MinValue);
                var currentName = Encoding.UTF8.GetString(ImageBytes.GetRange(currentNameOffset, currentNameOffset + currentNameLength).ToArray());

                if (functionName.Equals(currentName, StringComparison.OrdinalIgnoreCase))
                {
                    // Read the ordinal
                    var ordinalOffset = RvaToOffset(exportDirectory.AddressOfNameOrdinals) + sizeof(short) * middle;
                    var ordinal = MemoryMarshal.Read<short>(ImageBytes.GetRange(ordinalOffset)) + exportDirectory.Base;

                    return GetExportedFunction(ordinal);
                }

                if (string.CompareOrdinal(functionName, currentName) < 0)
                    high = middle - 1;
                else
                    low = middle + 1;
            }

            return null;
        }

        public ExportedFunction? GetExportedFunction(int functionOrdinal)
        {
            if (!IsValid)
                return null;

            // Read the function address
            var exportDirectory = MemoryMarshal.Read<ImageExportDirectory>(ImageBytes.GetRange(DirectoryOffset));

            if ((functionOrdinal -= exportDirectory.Base) >= exportDirectory.NumberOfFunctions)
                return null;

            var functionAddressOffset = RvaToOffset(exportDirectory.AddressOfFunctions) + sizeof(int) * functionOrdinal;
            var functionAddress = MemoryMarshal.Read<int>(ImageBytes.GetRange(functionAddressOffset));

            // Check if the function is forwarded
            var lowerBound = Headers.PEHeader.ExportTableDirectory.RelativeVirtualAddress;
            var upperBound = lowerBound + Headers.PEHeader.ExportTableDirectory.Size;

            if (functionAddress < lowerBound || functionAddress > upperBound)
                return new ExportedFunction(functionAddress, null);

            // Read the forwarder string
            var forwarderStringOffset = RvaToOffset(functionAddress);
            var forwarderStringLength = Array.IndexOf(ImageBytes.GetRange(forwarderStringOffset).ToArray(), byte.MinValue);
            var forwarderString = Encoding.UTF8.GetString(ImageBytes.GetRange(forwarderStringOffset, forwarderStringOffset + forwarderStringLength).ToArray());

            return new ExportedFunction(functionAddress, forwarderString);
        }
    }
}
