# UwuMemory
# C# Memory Library for .NET 5


suuuper alpha, lots to do. Uses function delegate pointers as outlined here instead of PInvoke: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/function-pointers

  Fully Implemented:
   - NtReadVirtualMemory
   - WriteProcessMemory
   - VirtualQueryEx
 
  Planned:
   - NtWriteVirtualMemory
   - NtAllocateVirtualMemory
   - NtFreeVirtualMemory
   - NtProtectVirtualMemory
   - VirtualProtectEx
   - Pattern scanning

Usage:
  
Create a new UwuMem object by passing the target Process ID to the constructor:


```csharp

UwuMem mem = new UwuMem(pId)
```

You can then access the rpm/wpm wrappers with:

```csharp

Int32 someValue = mem.rpm<Int32>(address)
byte[] someBuffer = mem.rpm(address, bytesToRead)

mem.wpm<Int32>(someAddress, someValue)
mem.wpm(someAddress, someBuffer)```
