using System.Runtime.InteropServices;

namespace Molten.Graphics.DX12
{
    internal static class DX12Win32
    {
        [DllImport("kernel32.dll")]
        internal unsafe static extern void* CreateEvent(void* lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal unsafe static extern uint WaitForSingleObjectEx(void* hHandle, uint dwMilliseconds, bool bAlertable);
    }

    /// <summary>
    /// Represents the result of waiting for a single object in the Win32 API.
    /// </summary>
    public enum Win32WaitForSingleObjectResult : uint
    {
        /// <summary>
        /// The state of the specified object is signaled.
        /// </summary>
        OBJECT_0 = 0x00000000,

        /// <summary>
        /// The wait was ended by one or more user-mode asynchronous procedure calls (APC) queued to the thread.
        /// </summary>
        IO_COMPLETION = 0x000000C0,

        /// <summary>
        /// The specified object is a mutex object that was not released by the thread that owned the mutex object before the owning thread terminated. Ownership of the mutex object is granted to the calling thread and the mutex is set to nonsignaled.
        /// If the mutex was protecting persistent state information, you should check it for consistency.
        /// </summary>
        ABANDONED = 0x00000080,

        /// <summary>
        /// The time-out interval elapsed, and the object's state is nonsignaled.
        /// </summary>
        TIMEOUT = 0x00000102,

        /// <summary>
        /// The function has failed. To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.
        /// </summary>
        FAILED = 0xFFFFFFFF
    }
}
