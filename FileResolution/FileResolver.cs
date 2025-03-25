using AncientLunar.Extensions;
using AncientLunar.Interop;
using AncientLunar.Native.PInvoke;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AncientLunar.FileResolution
{
    internal class FileResolver
    {
        private readonly Architecture _architecture;
        private readonly string _processDirectoryPath;
        private readonly string _rootDirectoryPath;

        internal FileResolver(Process process, string rootDirectoryPath)
        {
            _architecture = process.GetArchitecture();
            _processDirectoryPath = Path.GetDirectoryName(process.GetFileName());
            _rootDirectoryPath = rootDirectoryPath;
        }

        internal string ResolveFilePath(string fileName, ActivationContext activationContext)
        {
            // Check for .local redirection
            var dotLocalFilePath = Path.Combine(Path.Combine(_processDirectoryPath, ".local"), fileName);

            if (File.Exists(dotLocalFilePath))
                return dotLocalFilePath;

            // Check for SxS redirection
            var sxsFilePath = activationContext.ProbeManifest(fileName);

            if (sxsFilePath != null)
                return sxsFilePath;

            // Search the DLL root directory
            if (_rootDirectoryPath != null)
            {
                var rootDirectoryFilePath = Path.Combine(_rootDirectoryPath, fileName);

                if (File.Exists(rootDirectoryFilePath))
                    return rootDirectoryFilePath;
            }

            // Search the directory from which the process was loaded
            var processDirectoryFilePath = Path.Combine(_processDirectoryPath, fileName);

            if (File.Exists(processDirectoryFilePath))
                return processDirectoryFilePath;

            // Search the system directory
            var systemDirectoryFilePath = Environment.SystemDirectory;

            if (_architecture == Architecture.X86 && Process.GetCurrentProcess().GetArchitecture() == Architecture.X64)
                systemDirectoryFilePath = Path.GetFullPath(Path.Combine(systemDirectoryFilePath, "..\\SysWOW64"));

            systemDirectoryFilePath = Path.Combine(systemDirectoryFilePath, fileName);

            if (File.Exists(systemDirectoryFilePath))
                return systemDirectoryFilePath;

            // Search the Windows directory
            var windowsDirectoryFilePath = Path.GetFullPath(Path.Combine(Path.Combine(Environment.SystemDirectory, ".."), fileName));

            if (File.Exists(windowsDirectoryFilePath))
                return windowsDirectoryFilePath;

            // Search the current directory
            var currentDirectoryFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            if (File.Exists(currentDirectoryFilePath))
                return currentDirectoryFilePath;

            // Search the directories liste din the PATH environment variable
            return Environment.GetEnvironmentVariable("PATH").Split(';').Where(Directory.Exists).Select(directory => Path.Combine(directory, fileName)).FirstOrDefault(File.Exists);
        }
    }
}
