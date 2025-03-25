using System;

namespace AncientLunar.FileResolution.Structs
{
    internal struct ManifestDirectory
    {
        public string Path;
        public int Hash;
        public string Language;
        public Version Version;

        internal ManifestDirectory(string path, int hash, string language, Version version)
        {
            Path = path;
            Hash = hash;
            Language = language;
            Version = version;
        }
    }
}
