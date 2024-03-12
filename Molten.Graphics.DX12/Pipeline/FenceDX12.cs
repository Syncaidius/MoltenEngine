using Molten.Windows32;
using Newtonsoft.Json.Linq;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using System.Runtime.InteropServices;

namespace Molten.Graphics.DX12;

internal unsafe class FenceDX12 : GpuFence
{
    ID3D12Fence* _handle;
    DeviceDX12 _device;
    void* _fenceEvent;
    ulong _value;

    internal FenceDX12(DeviceDX12 device, FenceFlags flags) : base(device)
    {
        _device = device;
        void* ptr = null;
        Guid guid = ID3D12Fence.Guid;
        HResult hr = device.Handle->CreateFence(_value, flags, &guid, &ptr);
        if (!device.Log.CheckResult(hr, () => "Failed to create fence"))
            return;

        _value++;
        _handle = (ID3D12Fence*)ptr;
        _fenceEvent = Win32Events.CreateEvent(null, false, false, null);
        if (_fenceEvent == null)
        {
            hr = Marshal.GetLastWin32Error();
            hr.Throw();
        }
    }

    public override void Reset()
    {
        _value = 0;
        _handle->Signal(_value);
    }   

    internal ulong Signal(CommandQueueDX12 queue)
    {
        ulong fenceValue = Interlocked.Increment(ref _value);
        queue.Handle->Signal(_handle, fenceValue);
        return fenceValue;
    }

    /// <summary>
    /// Halts execution on the current thread until the fence is signaled by the GPU.
    /// </summary>
    /// <param name="queue">The queue that a signal event will be pushed to.</param>
    /// <param name="nsTimeout">A timeout, in nanoseconds. If set to 0, the call will immediately return the fence status as a bool without waiting.</param>
    /// <returns>True if the wait was succesful, or false if the timeout was reached.</returns>
    internal bool Wait(CommandQueueDX12 queue, ulong nsTimeout = ulong.MaxValue)
    {
        if (_handle->GetCompletedValue() < Signal(queue))
        {
            uint msTimeout = nsTimeout > uint.MaxValue ? uint.MaxValue : (uint)nsTimeout / 1000000U; // uint.MaxValue = Infinite timeout.
            _handle->SetEventOnCompletion(Value, _fenceEvent);
            WaitForSingleObjectResult result = (WaitForSingleObjectResult)Win32Events.WaitForSingleObjectEx(_fenceEvent, msTimeout, false);

            // Handle wait result.
            switch (result)
            {
                case WaitForSingleObjectResult.ABANDONED:
                case WaitForSingleObjectResult.TIMEOUT:
                case WaitForSingleObjectResult.FAILED:
                    _device.Log.Error($"Failed to wait for fence - {result}");
                    return false;
            }
        }

        return true;
    }

    protected override void OnGraphicsRelease()
    {
        NativeUtil.ReleasePtr(ref _handle);
        if(_fenceEvent != null)
        {
            Win32Events.CloseHandle(_fenceEvent);
            _fenceEvent = null;
        }
    }

    /// <summary>
    /// Gets or sets the value of the current <see cref="FenceDX12"/>.
    /// </summary>
    internal ulong Value => _value;

    internal ID3D12Fence* Handle => _handle;
}
