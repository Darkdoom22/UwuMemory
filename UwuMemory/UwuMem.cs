using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UwuMemory
{

    [StructLayout(LayoutKind.Sequential)]
    public struct MemoryBasicInformation
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public IntPtr RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }


    [Flags]
    public enum NTSTATUS : uint
    {
        _StatusSuccess = 0x00000000,
        _AccessDenied = 0xC0000022,
        _InvalidProtection = 0xC0000045,
        _SectionProtection = 0xC000004E,
        _ProcedureNotFound = 0xC000007A,
    }

    [Flags]
    public enum PageProtection
    {

        ZeroAccess = 0x00,
        NoAccess = 0x01,
        ReadOnly = 0x02,
        ReadWrite = 0x04,
        WriteCopy = 0x08,
        Execute = 0x10,
        ExecuteRead = 0x20,
        ExecuteReadWrite = 0x40,
        ExecuteWriteCopy = 0x80,
        Guard = 0x100,
        ReadWriteGuard = 0x104,
        NoCache = 0x200,
    }

    [Flags]
    public enum AllocationType : uint
    {

        Invalid = 0,
        Commit = 0x1000,
        Reserve = 0x2000,
        Freed = 0x10000,
        Reset = 0x80000,
        TopDown = 0x100000,
        WriteWatch = 0x200000,
        Physical = 0x400000,
        ResetUndo = 0x1000000,
        LargePages = 0x20000000
    }

    [Flags]
    public enum FreeType : uint
    {
        Invalid = 0,
        CoalescePlaceholders = 0x00000001,
        PreservePlaceholder = 0x00000002,
        Decommit = 0x4000,
        Release = 0x8000
    }

    public class UwuMem
    {
        private readonly SafeProcessHandle _sHandle;

        /// <summary>
        /// opens a safe handle to the target process ID
        /// todo: add an overload that takes a string instead of a pid
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        private SafeProcessHandle _SafeOpenProcess(int pId)
        {

            if (pId == 0)
                return null;

            SafeProcessHandle procHandle;

            try
            {
                procHandle = Process.GetProcessById(pId).SafeHandle;
                return procHandle;
            }
            catch (Exception ex)
            {
                throw new Exception($"[UwuMem] Failed to get process handle: {ex}");
            }

        }

        public MemoryBasicInformation QueryVirtualMemory(IntPtr baseAddress)
        {
            return _QueryVirtualMemory(baseAddress);
        }

        private unsafe MemoryBasicInformation _QueryVirtualMemory(IntPtr baseAddress)
        {

            var buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MemoryBasicInformation)));

            IntPtr query = K32.VirtualQueryEx(_sHandle, baseAddress, (void*)buffer, Marshal.SizeOf(typeof(MemoryBasicInformation)));

            try
            {
               return Marshal.PtrToStructure<MemoryBasicInformation>(buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
           
        }

        /// <summary>
        /// changes the protection of the specified memory page and returns the old protection
        /// </summary>
        /// <param name="baseAddress"></param>
        /// <param name="size"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private PageProtection _ProtectVirtualMemory(IntPtr baseAddress, int size, PageProtection type)
        {

            K32.VirtualProtectEx(_sHandle, baseAddress, size, type, out var oldProtectionType);
            return oldProtectionType;

        }


        /// <summary>
        /// byte rpm wrapper
        /// </summary>
        /// <param name="baseAddress"></param>
        /// <param name="toRead"></param>
        /// <returns></returns>
        public byte[] rpm(IntPtr baseAddress, int toRead)
        {
            return _ReadVirtualMemory(baseAddress, toRead);
        }

        /// <summary>
        /// generic type rpm wrapper
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseAddress"></param>
        /// <returns></returns>
        public T rpm<T>(IntPtr baseAddress) where T : struct
        {
             return _ReadVirtualMemory<T>(baseAddress);
        }

        /// <summary>
        /// read a generic type from target location in process's memory
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseAddress"></param>
        /// <returns></returns>
        private unsafe T _ReadProcessMemory<T>(IntPtr baseAddress) where T : struct
        {
            int size = Marshal.SizeOf<T>();

            IntPtr bytesRead;
            IntPtr buffer = Marshal.AllocHGlobal(size);

            bool status = K32.ReadProcessMemory(_sHandle, baseAddress, (void*)buffer, size, out bytesRead);

            if (!status)
            {
                throw new Exception($"[UwuMem] Failed to read from a region in process memory at: {baseAddress}" + Environment.NewLine);
            }

            try
            {
                return Marshal.PtrToStructure<T>(buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }

        }

        private unsafe T _ReadVirtualMemory<T>(IntPtr baseAddress) where T : struct
        {
            MemoryBasicInformation memInfo = _QueryVirtualMemory(baseAddress);
            if (memInfo.State != (uint)AllocationType.Commit)
            {
                return default(T);
            }
            
            int size = Marshal.SizeOf<T>();
            IntPtr bytesRead;
            byte[] buffer = new byte[size];
            NTSTATUS status;
     
            fixed(byte* bufferPtr = buffer)
            {
                status = (NTSTATUS)NtDll.NtReadVirtualMemory(_sHandle, baseAddress, bufferPtr, size, out bytesRead);

                if (status != NTSTATUS._StatusSuccess)
                {
                    throw new Exception($"[UwuMem] Failed to read from a region in process memory at: {baseAddress.ToString("X")}" + Environment.NewLine);
                }


                return Marshal.PtrToStructure<T>((IntPtr)bufferPtr);

            }

        }

        /// <summary>
        /// read an array of bytes from target memory location in process's memory
        /// </summary>
        /// <param name="baseAddress"></param>
        /// <param name="toRead"></param>
        /// <returns></returns>
        private unsafe byte[] _ReadVirtualMemory(IntPtr baseAddress, int toRead)
        {
            byte[] buffer = new byte[toRead];
            IntPtr bytesRead;

            fixed (byte* bufferPtr = buffer)
            {
                NTSTATUS status = (NTSTATUS)NtDll.NtReadVirtualMemory(_sHandle, baseAddress, bufferPtr, toRead, out bytesRead);

                Marshal.Copy((IntPtr)bufferPtr, buffer, 0, toRead);
                return buffer;
            }
        }

        public bool wpm<T>(IntPtr baseAddress, T data) where T : struct
        {
            return _WriteProcessMemory<T>(baseAddress, data);
        }

        public bool wpm(IntPtr baseAddress, byte[] data)
        {
            return _WriteProcessMemory(baseAddress, data);
        }

        private unsafe bool _WriteProcessMemory<T>(IntPtr baseAddress, T data) where T : struct
        {

            if (SafeHandle.ReferenceEquals(_sHandle, null) || baseAddress == IntPtr.Zero)
                return false;

            int size = Marshal.SizeOf<T>();

            byte[] buffer = new byte[size];
            IntPtr bufferPtr = Marshal.AllocHGlobal(size);
            var gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr bytesWritten;

            Marshal.Copy(gcHandle.AddrOfPinnedObject(), buffer, 0, size);
            Marshal.Copy(buffer, 0, bufferPtr, size);

            PageProtection oldProtect = _ProtectVirtualMemory(baseAddress, size, PageProtection.ExecuteReadWrite);

            bool write = K32.WriteProcessMemory(_sHandle, baseAddress, (void*)bufferPtr, size, out bytesWritten);

            _ProtectVirtualMemory(baseAddress, size, oldProtect);

            if(!write)
            {
                throw new Exception($"[UwuMem] WriteProcessMemory failed at address: {baseAddress}");
            }
            gcHandle.Free();
            Marshal.FreeHGlobal(bufferPtr);

            return write;
        }

        private unsafe bool _WriteProcessMemory(IntPtr baseAddress, byte[] data)
        {

            int size = Marshal.SizeOf(data);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            IntPtr bytesWritten;

            Marshal.Copy(data, 0, buffer, size);

            PageProtection oldProtect = _ProtectVirtualMemory(baseAddress, size, PageProtection.ExecuteReadWrite);
            bool write = K32.WriteProcessMemory(_sHandle, baseAddress, (void*)buffer, size, out bytesWritten);
            _ProtectVirtualMemory(baseAddress, size, oldProtect);

            if(!write)
            {
                throw new Exception($"[UwuMem] WriteProcessMemory failed at address: {baseAddress}");
            }
            Marshal.FreeHGlobal(buffer);

            return write;

        }

        public IntPtr ModuleEnumerator(string moduleName, Process proc)
        {
            return _ModuleEnumerator(moduleName, proc);
        }

        private IntPtr _ModuleEnumerator(string moduleName, Process proc)
        {

            if (String.IsNullOrEmpty(moduleName) || proc == null)
                return IntPtr.Zero;

            IEnumerator enumerator = proc.Modules.GetEnumerator();

            foreach (ProcessModule module in proc.Modules)
            {
                if(module.ModuleName == moduleName)
                {
                    return module.BaseAddress;
                }
            }

            return IntPtr.Zero;

        }


        public List<IntPtr> FindPattern(IntPtr regionStart, byte[] pattern, string mask)
        {
            return new List<IntPtr>();
        }

        public List<IntPtr> FindPattern(IntPtr regionStart, string pattern)
        {

            return new List<IntPtr>();
        }


        /// <summary>
        /// class constructor, initializes a safe handle to a process
        /// </summary>
        /// <param name="pId"></param>
        public UwuMem(int pId)
        {
            _sHandle = _SafeOpenProcess(pId);
        }
    }
}
