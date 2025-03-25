using AncientLunar.Native.PInvoke;
using AncientLunar.Native.Structs;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace AncientLunar.FileResolution
{
    internal class ApiSetMap
    {
        private readonly IntPtr _address;

        internal ApiSetMap()
        {
            _address = GetLocalAddress();
        }

        internal string ResolveApiSet(string apiSet, string parentName)
        {
            // Read the namespace
            var @namespace = (ApiSetNamespace)Marshal.PtrToStructure(_address, typeof(ApiSetNamespace));

            // Hash the API set without the patch number and suffix
            var charactersToHash = new string(apiSet.Take(apiSet.LastIndexOf('-')).ToArray());
            var apiSetHash = charactersToHash.Aggregate(0, (currentHash, character) => currentHash * @namespace.HashFactor + char.ToLower(character));

            // Search the namespace for the corresponding hash entry
            var low = 0;
            var high = @namespace.Count - 1;

            while (low <= high)
            {
                var middle = (low + high) / 2;

                // Read the hash entry
                var hashEntryAddress = (IntPtr)((long)_address + (@namespace.HashOffset + Marshal.SizeOf(typeof(ApiSetHashEntry)) * middle));
                var hashEntry = (ApiSetHashEntry)Marshal.PtrToStructure(hashEntryAddress, typeof(ApiSetHashEntry));

                if (apiSetHash == hashEntry.Hash)
                {
                    // Read the namespace entry name
                    var namespaceEntryAddress = (IntPtr)((long)_address + @namespace.EntryOffset + Marshal.SizeOf(typeof(ApiSetNamespaceEntry)) * hashEntry.Index);
                    var namespaceEntry = (ApiSetNamespaceEntry)Marshal.PtrToStructure(namespaceEntryAddress, typeof(ApiSetNamespaceEntry));
                    var namespaceEntryNameAddress = (IntPtr)((long)_address + namespaceEntry.NameOffset);
                    var namespaceEntryName = Marshal.PtrToStringUni(namespaceEntryNameAddress, namespaceEntry.NameLength / sizeof(char));

                    // Ensure the correct hash bucket is being used
                    if (!charactersToHash.Equals(new string(namespaceEntryName.Take(namespaceEntryName.LastIndexOf('-')).ToArray())))
                        break;

                    // Read the default value entry name
                    var valueEntryAddress = (IntPtr)((long)_address + namespaceEntry.ValueOffset);
                    var valueEntry = (ApiSetValueEntry)Marshal.PtrToStructure(valueEntryAddress, typeof(ApiSetValueEntry));
                    var valueEntryNameAddress = (IntPtr)((long)_address + valueEntry.ValueOffset);
                    var valueEntryName = Marshal.PtrToStringUni(valueEntryNameAddress, valueEntry.ValueCount / sizeof(char));

                    if (parentName == null || valueEntry.ValueCount == 1)
                        return valueEntryName;

                    // Search for an alternative host using the parent
                    for (var i = namespaceEntry.ValueCount - 1; i>= 0; i--)
                    {
                        // Read the alias value entry name
                        valueEntryAddress = (IntPtr)((long)_address + namespaceEntry.ValueOffset + Marshal.SizeOf(typeof(ApiSetValueEntry)) * i);
                        valueEntry = (ApiSetValueEntry)Marshal.PtrToStructure(valueEntryAddress, typeof(ApiSetValueEntry));
                        var valueEntryAliasNameAddress = (IntPtr)((long)_address + valueEntry.NameOffset);
                        var valueEntryAliasName = Marshal.PtrToStringUni(valueEntryAliasNameAddress, valueEntry.NameLength / sizeof(char));

                        if (parentName.Equals(valueEntryAliasName, StringComparison.OrdinalIgnoreCase))
                        {
                            // Read the value entry name
                            valueEntryNameAddress = (IntPtr)((long)_address + valueEntry.ValueOffset);
                            valueEntryName = Marshal.PtrToStringUni(valueEntryNameAddress, valueEntry.ValueCount / sizeof(char));
                            break;
                        }
                    }

                    return valueEntryName;
                }

                if ((uint)apiSetHash < (uint)hashEntry.Hash)
                    high = middle - 1;
                else
                    low = middle + 1;
            }

            return null;
        }

        private static IntPtr GetLocalAddress()
        {
            var pebAddress = Ntdll.RtlGetCurrentPeb();

            return (IntPtr)(IntPtr.Size == 8 ? ((Peb64)Marshal.PtrToStructure(pebAddress, typeof(Peb64))).ApiSetMap : ((Peb32)Marshal.PtrToStructure(pebAddress, typeof(Peb32))).ApiSetMap);
        }
    }
}
