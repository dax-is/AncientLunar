using System.Collections.Generic;

namespace AncientLunar.PortableExecutable.Structs
{
    internal struct ImportDescriptor
    {
        public string Name;
        public IEnumerable<ImportedFunction> Functions;

        internal ImportDescriptor(string name, IEnumerable<ImportedFunction> functions)
        {
            Name = name;
            Functions = functions;
        }
    }
}
