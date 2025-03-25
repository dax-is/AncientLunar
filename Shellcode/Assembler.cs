using AncientLunar.Native;
using AncientLunar.Shellcode.Structs;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;

namespace AncientLunar.Shellcode
{
    internal static class Assembler
    {
        internal static ArraySegment<byte> AssembleCall32(CallDescriptor<int> descriptor)
        {
            var shellcode = new List<byte>();

            if (descriptor.CallingConvention == CallingConvention.FastCall)
            {
                if (descriptor.Arguments.Count > 0)
                {
                    var argument = descriptor.Arguments[0];

                    if (argument == 0)
                        // xor ecx, ecx
                        shellcode.AddRange(new byte[] { 0x31, 0xC9 });
                    else
                    {
                        // mov ecx, argument
                        shellcode.Add(0xB9);
                        shellcode.AddRange(BitConverter.GetBytes(argument));
                    }
                }

                if (descriptor.Arguments.Count > 1)
                {
                    var argument = descriptor.Arguments[1];

                    if (argument == 0)
                        // xor edx, edx
                        shellcode.AddRange(new byte[] { 0x31, 0xD2 });
                    else
                    {
                        // mov edx, argument
                        shellcode.Add(0xBA);
                        shellcode.AddRange(BitConverter.GetBytes(argument));
                    }
                }
            }

            foreach (var argument in descriptor.Arguments.Skip(descriptor.CallingConvention == CallingConvention.FastCall ? 2 : 0).Reverse())
            {
                if (argument >= sbyte.MinValue && argument <= sbyte.MaxValue)
                    // push argument
                    shellcode.AddRange(new byte[] { 0x6A, unchecked((byte)argument) });
                else
                {
                    // push argument
                    shellcode.Add(0x68);
                    shellcode.AddRange(BitConverter.GetBytes(argument));
                }
            }


            // mov eax, Address
            shellcode.Add(0xB8);
            shellcode.AddRange(BitConverter.GetBytes((int)descriptor.Address));

            // call eax
            shellcode.AddRange(new byte[] { 0xFF, 0xD0 });

            if (descriptor.ReturnAddress.HasValue)
            {
                // mov [ReturnAddress], eax
                shellcode.Add(0xA3);
                shellcode.AddRange(BitConverter.GetBytes((int)descriptor.ReturnAddress.Value));
            }

            // xor eax, eax
            shellcode.AddRange(new byte[] { 0x31, 0xC0 });

            // ret
            shellcode.Add(0xC3);

            return new ArraySegment<byte>(shellcode.ToArray());
        }

        internal static ArraySegment<byte> AssembleCall64(CallDescriptor<long> descriptor)
        {
            var shellcode = new List<byte>();
            var shadowSpaceSize = Constants.ShadowSpaceSize + sizeof(long) * Math.Max(0, descriptor.Arguments.Count - 4);

            // sub rsp, shadowSpaceSize
            shellcode.AddRange(new byte[] { 0x48, 0x83, 0xEC, (byte)shadowSpaceSize });

            if (descriptor.Arguments.Count > 0)
            {
                var argument = descriptor.Arguments[0];

                if (argument == 0)
                    // xor ecx, ecx
                    shellcode.AddRange(new byte[] { 0x31, 0xC9 });
                else if (argument >= int.MinValue && argument <= uint.MaxValue)
                {
                    // mov ecx, argument
                    shellcode.Add(0xB9);
                    shellcode.AddRange(BitConverter.GetBytes((int)argument));
                } else
                {
                    // mov rcx, argument
                    shellcode.AddRange(new byte[] { 0x48, 0xB9 });
                    shellcode.AddRange(BitConverter.GetBytes(argument));
                }
            }

            if (descriptor.Arguments.Count > 1)
            {
                var argument = descriptor.Arguments[1];

                if (argument == 0)
                    // xor edx, edx
                    shellcode.AddRange(new byte[] { 0x31, 0xD2 });
                else if (argument >= int.MinValue && argument <= uint.MaxValue)
                {
                    // mov edx, argument
                    shellcode.Add(0xBA);
                    shellcode.AddRange(BitConverter.GetBytes((int)argument));
                }
                else
                {
                    // mov rdx, argument
                    shellcode.AddRange(new byte[] { 0x48, 0xBA });
                    shellcode.AddRange(BitConverter.GetBytes(argument));
                }
            }

            if (descriptor.Arguments.Count > 2)
            {
                var argument = descriptor.Arguments[2];

                if (argument == 0)
                    // xor r8, r8
                    shellcode.AddRange(new byte[] { 0x4D, 0x31, 0xC0 });
                else if (argument >= int.MinValue && argument <= uint.MaxValue)
                {
                    // mov r8d, argument
                    shellcode.AddRange(new byte[] { 0x41, 0xB8 });
                    shellcode.AddRange(BitConverter.GetBytes((int)argument));
                }
                else
                {
                    // mov r8, argument
                    shellcode.AddRange(new byte[] { 0x49, 0xB8 });
                    shellcode.AddRange(BitConverter.GetBytes(argument));
                }
            }

            if (descriptor.Arguments.Count > 3)
            {
                var argument = descriptor.Arguments[3];

                if (argument == 0)
                    // xor r9, r9
                    shellcode.AddRange(new byte[] { 0x4D, 0x31, 0xC9 });
                else if (argument >= int.MinValue && argument <= uint.MaxValue)
                {
                    // mov r9d, argument
                    shellcode.AddRange(new byte[] { 0x41, 0xB9 });
                    shellcode.AddRange(BitConverter.GetBytes((int)argument));
                }
                else
                {
                    // mov r9, argument
                    shellcode.AddRange(new byte[] { 0x49, 0xB9 });
                    shellcode.AddRange(BitConverter.GetBytes(argument));
                }
            }

            if (descriptor.Arguments.Count > 4)
            {
                foreach (var argument in descriptor.Arguments.Skip(4).Reverse())
                {
                    if (argument >= sbyte.MinValue && argument <= sbyte.MaxValue)
                        // push argument
                        shellcode.AddRange(new byte[] { 0x6A, unchecked((byte)argument) });
                    else if (argument >= int.MinValue && argument <= int.MaxValue)
                    {
                        // push argument
                        shellcode.Add(0x68);
                        shellcode.AddRange(BitConverter.GetBytes((int)argument));
                    } else
                    {
                        // mov rax, argument
                        shellcode.AddRange(new byte[] { 0x48, 0xB8 });
                        shellcode.AddRange(BitConverter.GetBytes(argument));

                        // push rax
                        shellcode.Add(0x50);
                    }
                }
            }

            // mov rax, Address
            shellcode.AddRange(new byte[] { 0x48, 0xB8 });
            shellcode.AddRange(BitConverter.GetBytes((long)descriptor.Address));

            // call rax
            shellcode.AddRange(new byte[] { 0xFF, 0xD0 });

            if (descriptor.ReturnAddress.HasValue)
            {
                // mov [ReturnAddress], rax
                shellcode.AddRange(new byte[] { 0x48, 0xA3 });
                shellcode.AddRange(BitConverter.GetBytes((long)descriptor.ReturnAddress.Value));
            }

            // xor eax, eax
            shellcode.AddRange(new byte[] { 0x31, 0xC0 });

            // add rsp, shadowSpaceSize
            shellcode.AddRange(new byte[] { 0x48, 0x83, 0xC4, (byte)shadowSpaceSize });

            // ret
            shellcode.Add(0xC3);

            return new ArraySegment<byte>(shellcode.ToArray());
        }
    }
}
