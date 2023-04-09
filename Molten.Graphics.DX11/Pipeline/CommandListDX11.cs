using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public unsafe class CommandListDX11 : GraphicsCommandList
    {
        ID3D11DeviceContext4* _native;
        ID3DUserDefinedAnnotation* _debugAnnotation;

        public CommandListDX11(GraphicsQueue queue, ID3D11DeviceContext4* context) : 
            base(queue)
        {
            _native = context;
            Device = queue.Device as DeviceDX11;

            if (_native->GetType() == DeviceContextType.Immediate)
                Type = CommandQueueType.Immediate;
            else
                Type = CommandQueueType.Deferred;

            Guid debugGuid = ID3DUserDefinedAnnotation.Guid;
            void* ptrDebug = null;
            _native->QueryInterface(ref debugGuid, &ptrDebug);
            _debugAnnotation = (ID3DUserDefinedAnnotation*)ptrDebug;
        }

        internal void Clear()
        {
            _native->ClearState();
        }

        protected override void OnDispose()
        {
            SilkUtil.ReleasePtr(ref _native);
            SilkUtil.ReleasePtr(ref _debugAnnotation);

            // Dispose context.
            if (Type != CommandQueueType.Immediate)
                Device.RemoveDeferredContext(this);
        }

        /// <summary>Gets the current <see cref="GraphicsQueueDX11"/> type. This value will not change during the context's life.</summary>
        internal CommandQueueType Type { get; private set; }

        internal ID3D11DeviceContext4* Ptr => _native;

        internal ID3DUserDefinedAnnotation* Debug => _debugAnnotation;

        internal DeviceDX11 Device { get; }
    }
}
