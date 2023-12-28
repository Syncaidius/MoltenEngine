using System.Runtime.InteropServices;
using Molten.Windows32;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12
{
    internal unsafe class FenceDX12 : GraphicsFence, IDisposable
    {
        ID3D12Fence* _ptr;
        DeviceDX12 _device;
        ulong _value;
        void* _fenceEvent;

        internal FenceDX12(DeviceDX12 device, FenceFlags flags)
        {
            _device = device;
            void* ptr = null;
            Guid guid = ID3D12Fence.Guid;
            HResult hr = device.Ptr->CreateFence(_value, flags, &guid, &ptr);
            if (!device.Log.CheckResult(hr, () => "Failed to create fence"))
                return;

            _value++;
            _ptr = (ID3D12Fence*)ptr;
            void* ptrEvent = Win32Events.CreateEvent(null, false, false, null);
            if (ptrEvent == null)
            {
                hr = Marshal.GetLastWin32Error();
                hr.Throw();
            }

            _ptr->SetEventOnCompletion(_value, ptrEvent);
        }

        public override void Reset()
        {
            Signal(0);
        }   

        internal void Signal(ulong value)
        {
            _value = value;
            _ptr->Signal(_value);
        }

        public override bool Wait(ulong nsTimeout = ulong.MaxValue)
        {
            _device.Queue.Ptr->Signal(_ptr, _value);

            if(_ptr->GetCompletedValue() < _value)
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

            _value++;

            return true;
        }

        public void Dispose()
        {
            SilkUtil.ReleasePtr(ref _ptr);
        }
    }
}
