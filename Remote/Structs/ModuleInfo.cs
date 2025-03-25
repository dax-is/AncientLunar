namespace AncientLunar.Remote.Structs
{
    public struct ModuleInfo
    {
        public readonly ulong Address;
        public readonly string FileName;

        internal ModuleInfo(ulong address, string fileName)
        {
            Address = address;
            FileName = fileName;
        }
    }
}
