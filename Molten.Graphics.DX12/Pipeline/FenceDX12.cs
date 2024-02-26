using Molten.Windows32;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using System.Runtime.InteropServices;

namespace Molten.Graphics.DX12;

internal unsafe class FenceDX12 : GraphicsFence
{
    ID3D12Fence* _ptr;
    DeviceDX12 _device;
    void* _fenceEvent;
    ulong _value;

    internal FenceDX12(DeviceDX12 device, FenceFlags flags) : base(device)
    {
        _device = device;
        void* ptr = null;
        Guid guid = ID3D12Fence.Guid;
        HResult hr = device.Ptr->CreateFence(_value, flags, &guid, &ptr);
        if (!device.Log.CheckResult(hr, () => "Failed to create fence"))
            return;

        _value++;
        _ptr = (ID3D12Fence*)ptr;
        _fenceEvent = Win32Events.CreateEvent(null, false, false, null);
        if (_fenceEvent == null)
        {
            hr = Marshal.GetLastWin32Error();
            hr.Throw();
        }

        _ptr->SetEventOnCompletion(Value, _fenceEvent);
    }

    public override void Reset()
    {
        Set(0);
    }   

    /// <summary>
    /// Sets the fence value from the CPU side.
    /// </summary>
    /// <param name="value">The value to set the current <see cref="FenceDX12"/> to.</param>
    internal void Set(ulong value)
    {
        _value = value;
        _ptr->Signal(_value);
    }

    public override bool Wait(ulong nsTimeout = ulong.MaxValue)
    {
        ulong fenceVal = Interlocked.Increment(ref _value);
        _device.Queue.Handle->Signal(_ptr, fenceVal);

        if (_ptr->GetCompletedValue() < fenceVal)
        {
            uint msTimeout = (uint)nsTimeout / 1000000U; // Convert from nanoseconds to milliseconds.
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
        NativeUtil.ReleasePtr(ref _ptr);
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
}
