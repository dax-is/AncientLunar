using AncientLunar.Interop;
using AncientLunar.PortableExecutable.Native;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace AncientLunar.SymbolResolution
{
    public static class SymbolDownloader
    {
        public static string FindOrDownloadNtdllSymbols(Architecture architecture)
        {
            var filePath = Path.Combine(architecture.GetSystemDirectory(), "ntdll.dll");

            using (var peReader = new PEReader(File.OpenRead(filePath)))
            {
                var codeViewEntry = peReader.ReadDebugDirectory().First(entry => entry.Type == DebugDirectoryEntryType.CodeView);
                var pdbData = peReader.ReadCodeViewDebugDirectoryData(codeViewEntry);

                // Check if the correct PDB version is already cached to avoid duplicate downloads
                var cacheDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lunar\\Dependencies");
                var cacheDirectory = Directory.CreateDirectory(cacheDirectoryPath);
                var pdbFilePath = Path.Combine(cacheDirectory.FullName, $"{pdbData.Path.Replace(".pdb", string.Empty)}-{pdbData.Guid:N}.pdb");
                var pdbFile = new FileInfo(pdbFilePath);

                if (pdbFile.Exists && pdbFile.Length != 0)
                    return pdbFilePath;

                foreach (var file in cacheDirectory.GetFiles().Where(file => file.Name.StartsWith(pdbData.Path)))
                {
                    try { file.Delete(); } catch { }
                }

                // Download the PDB from the Microsoft symbol server
                using (var webClient = new WebClient())
                {
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                    ServicePointManager.DefaultConnectionLimit = 9999;

                    webClient.DownloadFile($"https://msdl.microsoft.com/download/symbols/{pdbData.Path}/{pdbData.Guid:N}{pdbData.Age}/{pdbData.Path}", pdbFilePath);
                }

                return pdbFilePath;
            }
        }
    }
}
