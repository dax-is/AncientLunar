using System;

namespace AncientLunar.PortableExecutable.Native
{
    public readonly struct CodeViewDebugDirectoryData
    {
        /// <summary>The Globally Unique Identifier (GUID) of the associated PDB.</summary>
        public Guid Guid { get; }

        /// <summary>The iteration of the PDB. The first iteration is 1. The iteration is incremented each time the PDB content is augmented.</summary>
        public int Age { get; }

        /// <summary>The path to the .pdb file that contains debug information for the PE/COFF file.</summary>
        public string Path { get; }

        internal CodeViewDebugDirectoryData(Guid guid, int age, string path)
        {
            Path = path;
            Guid = guid;
            Age = age;
        }
    }
}
