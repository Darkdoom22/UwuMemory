using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UwuMemory
{

    public unsafe static class K32
    {

        public static readonly delegate* unmanaged[Stdcall]
            <SafeProcessHandle, IntPtr, void*, int, IntPtr>
            VirtualQueryEx;

        public static readonly delegate* unmanaged[Stdcall]
            <SafeProcessHandle, IntPtr, void*, int, out IntPtr, bool>
            ReadProcessMemory;

        public static readonly delegate* unmanaged[Stdcall]
            <SafeProcessHandle, IntPtr, void*, int, out IntPtr, bool>
            WriteProcessMemory;

        static K32()
        {
            var handle = NativeLibrary.Load("kernel32.dll");
            VirtualQueryEx =
                (delegate* unmanaged[Stdcall]<SafeProcessHandle, IntPtr, void*, int, IntPtr>)
                NativeLibrary.GetExport(handle, nameof(VirtualQueryEx));

            ReadProcessMemory =
                (delegate* unmanaged[Stdcall]<SafeProcessHandle, IntPtr, void*, int, out IntPtr, bool>)
                NativeLibrary.GetExport(handle, nameof(ReadProcessMemory));

            WriteProcessMemory =
                (delegate* unmanaged[Stdcall]<SafeProcessHandle, IntPtr, void*, int, out IntPtr, bool>)
                NativeLibrary.GetExport(handle, nameof(WriteProcessMemory));

        }
      
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern PageProtection VirtualProtectEx(SafeProcessHandle processHandle, IntPtr baseAddress, int protectionSize, PageProtection protectionType, out PageProtection oldProtectionType);

    }
}
