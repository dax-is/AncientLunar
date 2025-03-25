namespace AncientLunar.PortableExecutable.Structs
{
    internal struct TlsCallback
    {
        public int RelativeAddress;

        internal TlsCallback(int relativeAddress)
        {
            RelativeAddress = relativeAddress;
        }
    }
}
