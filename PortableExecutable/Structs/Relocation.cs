using AncientLunar.Native.Enums;

namespace AncientLunar.PortableExecutable.Structs
{
    internal struct Relocation
    {
        public RelocationType Type;
        public int Offset;

        internal Relocation(RelocationType type, int offset)
        {
            Type = type;
            Offset = offset;
        }
    }
}
