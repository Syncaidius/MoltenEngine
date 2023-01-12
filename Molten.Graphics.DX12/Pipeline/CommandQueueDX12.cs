using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.Logging;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Molten.Graphics
{
    internal unsafe class CommandQueueDX12 : EngineObject
    {
        CommandQueueDesc _desc;
        ID3D12CommandQueue* _ptr;

        internal CommandQueueDX12(Logger log, DeviceDX12 device, DeviceBuilderDX12 builder, ref CommandQueueDesc desc)
        {
            _desc = desc;
            Device = device;
            Log = log;

            Initialize(builder);
        }

        private void Initialize(DeviceBuilderDX12 builder)
        {
            Guid cmdGuid = ID3D12CommandQueue.Guid;
            void* cmdQueue = null;
            HResult r = Device.Ptr->CreateCommandQueue(ref _desc, &cmdGuid, &cmdQueue);
            if (!builder.CheckResult(r))
            {
                Log.Error($"Failed to initialize '{_desc.Type}' command queue");
                return;
            }
            else
            {
                Log.WriteLine($"Initialized '{_desc.Type}' command queue");
            }

            _ptr = (ID3D12CommandQueue*)cmdQueue;
        }

        protected override void OnDispose()
        {
            SilkUtil.ReleasePtr(ref _ptr);
        }

        internal DeviceDX12 Device { get; }

        internal Logger Log { get; }
    }
}
