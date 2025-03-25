using AncientLunar.Helpers;
using AncientLunar.Native.Structs;
using AncientLunar.PortableExecutable.Native;
using AncientLunar.PortableExecutable.Structs;
using System;
using System.Collections.Generic;

namespace AncientLunar.PortableExecutable.DataDirectories
{
    internal class TlsDirectory : DataDirectoryBase
    {
        internal TlsDirectory(ArraySegment<byte> imageBytes, PEHeaders headers) : base(imageBytes, headers, headers.PEHeader.ThreadLocalStorageTableDirectory) { }

        public IEnumerable<TlsCallback> GetTlsCallbacks()
        {
            if (!IsValid)
                yield break;

            if (Headers.PEHeader.Magic == PEMagic.PE32)
            {
                // Read the TLS directory
                var tlsDirectory = MemoryMarshal.Read<ImageTlsDirectory32>(ImageBytes.GetRange(DirectoryOffset));

                if (tlsDirectory.AddressOfCallBacks == 0)
                    yield break;

                for (var i = 0; ; i++)
                {
                    // Read the callback address
                    var callbackAddressOffset = RvaToOffset(VaToRva(tlsDirectory.AddressOfCallBacks)) + sizeof(int) * i;
                    var callbackAddress = MemoryMarshal.Read<int>(ImageBytes.GetRange(callbackAddressOffset));

                    if (callbackAddress == 0)
                        break;

                    yield return new TlsCallback(VaToRva(callbackAddress));
                }
            }
            else
            {
                // Read the TLS directory
                var tlsDirectory = MemoryMarshal.Read<ImageTlsDirectory64>(ImageBytes.GetRange(DirectoryOffset));

                if (tlsDirectory.AddressOfCallBacks == 0)
                    yield break;

                for (var i = 0; ; i++)
                {
                    // Read the callback address
                    var callbackAddressOffset = RvaToOffset(VaToRva(tlsDirectory.AddressOfCallBacks)) + sizeof(long) * i;
                    var callbackAddress = MemoryMarshal.Read<long>(ImageBytes.GetRange(callbackAddressOffset));

                    if (callbackAddress == 0)
                        break;

                    yield return new TlsCallback(VaToRva(callbackAddress));
                }
            }
        }
    }
}
