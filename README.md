# Ancient Lunar

A .NET 3.5 backport for [lunar](https://github.com/jdewera/lunar), with wow64->x64 injection support. For usage, check out the original [README.md](https://github.com/jdewera/lunar/blob/main/README.md).

## Wow64 -> x64 injection?

This is achieved by using a [Heaven's Gate](https://0xk4n3ki.github.io/posts/Heavens-Gate-Technique/). While this is commonly used for defense evasion, this is very useful when needing to inject code into a 64-bit process from a wow64 context. Even though most memory APIs are perfectly usable across this boundary (+ using NtWow64ReadVirtualMemory64 and NtWow64WriteVirtualMemory64 to access 64-bit addresses), it is not possible to create threads from a wow64 context.

Here's where the Heaven's Gate comes into play, since if we transition to x64, we can call `RtlCreateUserThread` from the 64-bit version of ntdll.dll, which is also loaded by wow64 processes. The address for this function can be found by enumerating the loaded modules from the 64-bit PEB.

To locate the 64-bit PEB, we could do some magic to first locate the current thread's 64-bit TEB (yes I stole the code and I do not remember from where), but you could also just call `NtWow64QueryInformationProcess64` and get the PEB address from that. Then it's just a matter of using `NtWow64ReadVirtualMemory` to read the PEB_LDR_DATA until you hit `ntdll.dll`.
