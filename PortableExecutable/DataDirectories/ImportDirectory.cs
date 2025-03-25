using AncientLunar.Helpers;
using AncientLunar.Native.Structs;
using AncientLunar.PortableExecutable.Native;
using AncientLunar.PortableExecutable.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AncientLunar.PortableExecutable.DataDirectories
{
    internal class ImportDirectory : DataDirectoryBase
    {
        internal ImportDirectory(ArraySegment<byte> imageBytes, PEHeaders headers) : base(imageBytes, headers, headers.PEHeader.ImportTableDirectory) { }

        public IEnumerable<ImportDescriptor> GetImportDescriptors()
        {
            if (!IsValid)
                yield break;

            for (var i = 0; ; i++)
            {
                // Read the descriptor
                var descriptorOffset = DirectoryOffset + Marshal.SizeOf(typeof(ImageImportDescriptor)) * i;
                var descriptor = MemoryMarshal.Read<ImageImportDescriptor>(ImageBytes.GetRange(descriptorOffset));

                if (descriptor.FirstThunk == 0)
                    break;

                // Read the name
                var nameOffset = RvaToOffset(descriptor.Name);
                var nameLength = Array.IndexOf(ImageBytes.GetRange(nameOffset).ToArray(), byte.MinValue);
                var name = Encoding.UTF8.GetString(ImageBytes.GetRange(nameOffset, nameOffset + nameLength).ToArray());

                // Read the functions imported under the descriptor
                var offsetTableOffset = RvaToOffset(descriptor.FirstThunk);
                var thunkTakeOffset = descriptor.OriginalFirstThunk == 0 ? offsetTableOffset : RvaToOffset(descriptor.OriginalFirstThunk);
                var functions = GetImportedFunctions(offsetTableOffset, thunkTakeOffset);

                yield return new ImportDescriptor(name, functions);
            }
        }

        private ImportedFunction GetImportedFunction(int thunk)
        {
            // Read the ordinal
            var ordinalOffset = RvaToOffset(thunk);
            var ordinal = MemoryMarshal.Read<short>(ImageBytes.GetRange(ordinalOffset));

            // Read the name
            var nameOffset = ordinalOffset + sizeof(short);
            var nameLength = Array.IndexOf(ImageBytes.GetRange(nameOffset).ToArray(), byte.MinValue);
            var name = Encoding.UTF8.GetString(ImageBytes.GetRange(nameOffset, nameOffset + nameLength).ToArray());

            return new ImportedFunction(name, ordinal, 0);
        }

        private IEnumerable<ImportedFunction> GetImportedFunctions(int offsetTableOffset, int thunkTableOffset)
        {
            for (var i = 0; ; i++)
            {
                if (Headers.PEHeader.Magic == PEMagic.PE32)
                {
                    // Read the thunk
                    var thunkOffset = thunkTableOffset + sizeof(int) * i;
                    var thunk = MemoryMarshal.Read<int>(ImageBytes.GetRange(thunkOffset));

                    if (thunk == 0)
                        break;

                    // Check if the function is imported via ordinal
                    var functionOffset = offsetTableOffset + sizeof(int) * i;

                    if ((thunk & int.MinValue) != 0)
                    {
                        var ordinal = thunk & ushort.MaxValue;
                        yield return new ImportedFunction(null, ordinal, functionOffset);
                    } else
                    {
                        var importedFunction = GetImportedFunction(thunk);
                        importedFunction.Offset = functionOffset;
                        
                        yield return importedFunction;
                    }
                } else
                {
                    // Read the thunk
                    var thunkOffset = thunkTableOffset + sizeof(long) * i;
                    var thunk = MemoryMarshal.Read<long>(ImageBytes.GetRange(thunkOffset));

                    if (thunk == 0)
                        break;

                    // Check if the function is imported via ordinal
                    var functionOffset = offsetTableOffset + sizeof(long) * i;

                    if ((thunk & long.MinValue) != 0)
                    {
                        var ordinal = thunk & ushort.MaxValue;
                        yield return new ImportedFunction(null, (int)ordinal, functionOffset);
                    } else
                    {
                        var importedFunction = GetImportedFunction((int)thunk);
                        importedFunction.Offset = functionOffset;

                        yield return importedFunction;
                    }
                }
            }
        }
    }
}
