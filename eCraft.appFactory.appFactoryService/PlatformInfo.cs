using System;
using System.Runtime.InteropServices;

namespace eCraft.appFactory.appFactoryService
{
    public static class PlatformInfo
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEM_INFO
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public UIntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        };

        public enum Platform
        {
            X86,
            X64,
            IA64,
            Unknown
        }

        internal const ushort PROCESSOR_ARCHITECTURE_INTEL = 0;
        internal const ushort PROCESSOR_ARCHITECTURE_IA64 = 6;
        internal const ushort PROCESSOR_ARCHITECTURE_AMD64 = 9;
        internal const ushort PROCESSOR_ARCHITECTURE_UNKNOWN = 0xFFFF;

        [DllImport("kernel32.dll")]
        internal static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        public static Platform GetPlatformInfo()
        {
            SYSTEM_INFO sysInfo = new SYSTEM_INFO();
            GetNativeSystemInfo(ref sysInfo);
            switch (sysInfo.wProcessorArchitecture)
            {
                case PROCESSOR_ARCHITECTURE_IA64:
                    return Platform.IA64;
                case PROCESSOR_ARCHITECTURE_AMD64:
                    return Platform.X64;
                case PROCESSOR_ARCHITECTURE_INTEL:
                    return Platform.X86;
                default:
                    return Platform.Unknown;
            }
        }
    }
}
