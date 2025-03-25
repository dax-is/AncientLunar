using AncientLunar.Extensions;
using AncientLunar.FileResolution.Structs;
using AncientLunar.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace AncientLunar.FileResolution
{
    internal class ActivationContext
    {
        private readonly Architecture _architecture;
        private readonly ILookup<int, ManifestDirectory> _directoryCache;
        private readonly XDocument _manifest;

        internal ActivationContext(XDocument manifest, Architecture architecture)
        {
            _architecture = architecture;
            _directoryCache = GetManifestDirectories(architecture).ToLookup(directory => directory.Hash);
            _manifest = manifest;
        }

        internal string ProbeManifest(string fileName)
        {
            if (_manifest?.Root is null)
                return null;

            // Search the manifest tree that holds the dependency references
            var @namespace = _manifest.Root.GetDefaultNamespace();
            var elementName1 = @namespace + "dependency";
            var elementName2 = @namespace + "dependentAssembly";
            var elementName3 = @namespace + "assemblyIdentity";

            foreach (var dependency in _manifest.Descendants(elementName1).Elements(elementName2).Elements(elementName3))
            {
                // Parse the dependency attributes
                var architecture = dependency.Attribute("processorArchitecture")?.Value;
                var language = dependency.Attribute("language")?.Value;
                var name = dependency.Attribute("name")?.Value;
                var token = dependency.Attribute("publicKeyToken")?.Value;
                var version = dependency.Attribute("version")?.Value;

                if (architecture is null || language is null || name is null || token is null || version is null)
                    continue;

                if (architecture == "*")
                    architecture = _architecture == Architecture.X86 ? "x86" : "amd64";

                if (language == "*")
                    language = "none";

                // Query the cache for matching directories
                var dependencyHash = $"{architecture}{name.ToLower()}{token}".GetHashCode();

                if (!_directoryCache.Contains(dependencyHash))
                    continue;

                var matchingDirectories = _directoryCache[dependencyHash].Where(directory => directory.Language.Equals(language, StringComparison.OrdinalIgnoreCase));

                // Search for the directory that holds the dependency
                var dependencyVersion = new Version(version);
                var matchingDirectory = (dependencyVersion.Build == 0 && dependencyVersion.Revision == 0) ? matchingDirectories.Where(directory => directory.Version.Major == dependencyVersion.Major && directory.Version.Minor == dependencyVersion.Minor).MaxBy(directory => directory.Version) : matchingDirectories.FirstOrDefault(directory => directory.Version == dependencyVersion);

                if (matchingDirectory.Hash == 0)
                    continue;

                var sxsFilePath = Path.Combine(matchingDirectory.Path, fileName);

                if (File.Exists(sxsFilePath))
                    return sxsFilePath;
            }

            return null;
        }

        private static IEnumerable<ManifestDirectory> GetManifestDirectories(Architecture architecture)
        {
            var sxsDirectory = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "..\\WinSxS"));
            var directoryPrefix = architecture == Architecture.X86 ? "x86" : "amd64";

            foreach (var directory in sxsDirectory.GetDirectories().Where(directory => directory.Name.StartsWith(directoryPrefix)))
            {
                var nameComponents = directory.Name.Split('_');
                var language = nameComponents[nameComponents.Length - 2];
                var version = new Version(nameComponents[nameComponents.Length - 3]);

                // Hash the directory name without the version, language and hash
                var directoryHash = string.Join(string.Empty, nameComponents.Take(nameComponents.Length - 3).ToArray()).GetHashCode();

                yield return new ManifestDirectory(directory.FullName, directoryHash, language, version);
            }
        }
    }
}
