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

    internal FenceDX12(DeviceDX12 device, FenceFlags flags) : base(device)
    {
        _device = device;
        void* ptr = null;
        Guid guid = ID3D12Fence.Guid;
        HResult hr = device.Ptr->CreateFence(Value, flags, &guid, &ptr);
        if (!device.Log.CheckResult(hr, () => "Failed to create fence"))
            return;

        Value++;
        _ptr = (ID3D12Fence*)ptr;
        void* ptrEvent = Win32Events.CreateEvent(null, false, false, null);
        if (ptrEvent == null)
        {
            hr = Marshal.GetLastWin32Error();
            hr.Throw();
        }

        _ptr->SetEventOnCompletion(Value, ptrEvent);
    }

    public override void Reset()
    {
        Signal(0);
    }   

    internal void Signal(ulong value)
    {
        Value = value;
        _ptr->Signal(Value);
    }

    public override bool Wait(ulong nsTimeout = ulong.MaxValue)
    {
        _device.Queue.Ptr->Signal(_ptr, Value);

        if(_ptr->GetCompletedValue() < Value)
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

        Value++;

        return true;
    }

    protected override void OnGraphicsRelease()
    {
        NativeUtil.ReleasePtr(ref _ptr);
    }

    /// <summary>
    /// Gets or sets the value of the current <see cref="FenceDX12"/>.
    /// </summary>
    internal ulong Value { get; set; }
}
