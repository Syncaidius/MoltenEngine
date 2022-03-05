using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public unsafe class UAView : PipeBindable<ID3D11UnorderedAccessView>
    {
        ID3D11UnorderedAccessView* _native;
        UnorderedAccessViewDesc _desc;

        internal UAView(Device device) : base(device, ContextBindTypeFlags.Output)
        {
        }

        internal override unsafe ID3D11UnorderedAccessView* NativePtr => _native;

        internal ref UnorderedAccessViewDesc Desc => ref _desc;

        internal void Recreate(ID3D11Resource* resource)
        {
            SilkUtil.ReleasePtr(ref _native);
            Device.NativeDevice->CreateUnorderedAccessView(resource, ref _desc, ref _native);
        }

        internal void Release()
        {
            SilkUtil.ReleasePtr(ref _native);
        }

        internal override void Refresh(ContextSlot slot, DeviceContext pipe)
        {
            throw new NotSupportedException("UAView does not support Refresh()");
        }

        internal override void PipelineDispose()
        {
            Release();
        }
    }
}
