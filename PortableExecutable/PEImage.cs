using AncientLunar.Helpers;
using AncientLunar.PortableExecutable.DataDirectories;
using AncientLunar.PortableExecutable.Native;
using System;
using System.IO;

namespace AncientLunar.PortableExecutable
{
    internal class PEImage
    {
        public ExportDirectory ExportDirectory { get; }
        public PEHeaders Headers { get; }
        public ImportDirectory ImportDirectory { get; }
        public RelocationDirectory RelocationDirectory { get; }
        public ResourceDirectory ResourceDirectory { get; }
        public TlsDirectory TlsDirectory { get; }

        public PEImage(ArraySegment<byte> imageBytes)
        {
            using (var reader = new PEReader(new MemoryStream(imageBytes.ToArray())))
            {
                if (reader.PEHeaders.PEHeader is null || !reader.PEHeaders.IsDll)
                    throw new Exception("The provided file was not a valid DLL");

                Headers = reader.PEHeaders;
                ExportDirectory = new ExportDirectory(imageBytes, reader.PEHeaders);
                ImportDirectory = new ImportDirectory(imageBytes, reader.PEHeaders);
                RelocationDirectory = new RelocationDirectory(imageBytes, reader.PEHeaders);
                ResourceDirectory = new ResourceDirectory(imageBytes, reader.PEHeaders);
                TlsDirectory = new TlsDirectory(imageBytes, reader.PEHeaders);
            }
        }
    }
}
