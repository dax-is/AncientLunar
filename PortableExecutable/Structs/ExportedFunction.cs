namespace AncientLunar.PortableExecutable.Structs
{
    internal struct ExportedFunction
    {
        public int RelativeAddress;
        public string ForwarderString;

        internal ExportedFunction(int relativeAddress, string forwarderString)
        {
            RelativeAddress = relativeAddress;
            ForwarderString = forwarderString;
        }
    }
}
