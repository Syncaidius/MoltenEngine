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
    public unsafe class SRView : PipeBindable<ID3D11ShaderResourceView>
    {
        ID3D11ShaderResourceView* _native;
        ShaderResourceViewDesc _desc;

        internal SRView(Device device) : base(device, PipeBindTypeFlags.Input)
        {
        }

        internal override unsafe ID3D11ShaderResourceView* NativePtr => _native;

        internal ref ShaderResourceViewDesc Desc => ref _desc;

        internal void Recreate(ID3D11Resource* resource)
        {
            SilkUtil.ReleasePtr(ref _native);
            Device.NativeDevice->CreateShaderResourceView(resource, ref _desc, ref _native);
        }

        internal void Release()
        {
            SilkUtil.ReleasePtr(ref _native);
        }

        internal override void Refresh(ContextSlot slot, DeviceContext pipe)
        {
            throw new NotSupportedException("SRView does not support Refresh()");
        }

        internal override void PipelineDispose()
        {
            Release();
        }
    }
}
