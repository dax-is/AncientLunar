using AncientLunar.PortableExecutable;

namespace AncientLunar.Remote.Structs
{
    internal struct Module
    {
        public readonly ulong Address;
        public readonly PEImage PEImage;

        internal Module(ulong address, PEImage peImage)
        {
            Address = address;
            PEImage = peImage;
        }
    }
}
