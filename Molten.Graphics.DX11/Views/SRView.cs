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
    public unsafe class SRView
    {
        ID3D11ShaderResourceView* _native;
        ShaderResourceViewDesc _desc;
        Device _device;

        internal SRView(Device device)
        {
            _device = device;
        }

        internal unsafe ID3D11ShaderResourceView* NativePtr => _native;

        internal ref ShaderResourceViewDesc Desc => ref _desc;

        internal void Recreate(ID3D11Resource* resource)
        {
            SilkUtil.ReleasePtr(ref _native);
            _device.NativeDevice->CreateShaderResourceView(resource, ref _desc, ref _native);
        }

        internal void Release()
        {
            SilkUtil.ReleasePtr(ref _native);
        }
    }
}
