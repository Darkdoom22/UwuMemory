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

UwuMem mem = new UwuMem(pId);
```

You can then access the rpm/wpm wrappers with:

```csharp
//Read an Int32(4 bytes) value from some Address in your target process
Int32 someValue = mem.rpm<Int32>(someAddress);
//Read an array of bytes from some Address in your target process
byte[] someBuffer = mem.rpm(someAddress, bytesToRead);

//Write an Int32(4 bytes) value to some Address in your target process
mem.wpm<Int32>(someAddress, someValue);
//Write an array of bytes to some Address in your target process
mem.wpm(someAddress, someBuffer);
```
