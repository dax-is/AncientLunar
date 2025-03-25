namespace AncientLunar.PortableExecutable.Structs
{
    internal struct ImportedFunction
    {
        public string Name;
        public int Ordinal;
        public int Offset;

        internal ImportedFunction(string name, int ordinal, int offset)
        {
            Name = name;
            Ordinal = ordinal;
            Offset = offset;
        }
    }
}
