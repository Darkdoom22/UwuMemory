using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Security;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32.SafeHandles;

namespace UwuMemory
{
    [SuppressUnmanagedCodeSecurity]
    public static unsafe class NtDll
    {
        public static readonly delegate* unmanaged[Stdcall]
            <SafeProcessHandle, IntPtr, void*, int, out IntPtr, uint>
            NtReadVirtualMemory;

        public static readonly delegate* unmanaged[Stdcall]
            <SafeProcessHandle, IntPtr, void*, int, out IntPtr, uint>
            NtWriteVirtualMemory;

        public static readonly delegate* unmanaged[Stdcall]
            <IntPtr, out IntPtr, IntPtr, ref IntPtr, AllocationType, PageProtection, uint>
            NtAllocateVirtualMemory;

        public static readonly delegate* unmanaged[Stdcall]
            <IntPtr, ref IntPtr, ref IntPtr, FreeType, uint>
            NtFreeVirtualMemory;

        public static readonly delegate* unmanaged[Stdcall]
            <IntPtr, ref IntPtr, ref IntPtr, PageProtection, out PageProtection, uint>
            NtProtectVirtualMemory;

        static NtDll()
        {
            var handle = NativeLibrary.Load("ntdll.dll");

            NtReadVirtualMemory =
                (delegate* unmanaged[Stdcall]<SafeProcessHandle, IntPtr, void*, int, out IntPtr, uint>)
                NativeLibrary.GetExport(handle, nameof(NtReadVirtualMemory));

            NtWriteVirtualMemory =
                (delegate* unmanaged[Stdcall]<SafeProcessHandle, IntPtr, void*, int, out IntPtr, uint>)
                NativeLibrary.GetExport(handle, nameof(NtWriteVirtualMemory));
        }

        [DllImport("ntdll", SetLastError = true)]
        public static extern NTSTATUS NtWriteVirtualMemoryManaged(SafeProcessHandle handle, IntPtr baseAddress, byte[] buffer, int bytesToWrite, int bytesWritten = 0);
    }
}
