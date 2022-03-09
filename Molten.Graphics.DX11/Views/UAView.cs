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
    public unsafe class UAView
    {
        ID3D11UnorderedAccessView* _native;
        UnorderedAccessViewDesc _desc;
        Device _device;

        internal UAView(Device device) 
        {
            _device = device;
        }

        internal unsafe ID3D11UnorderedAccessView* NativePtr => _native;

        internal ref UnorderedAccessViewDesc Desc => ref _desc;

        internal void Recreate(ID3D11Resource* resource)
        {
            SilkUtil.ReleasePtr(ref _native);
            _device.NativeDevice->CreateUnorderedAccessView(resource, ref _desc, ref _native);
        }

        internal void Release()
        {
            SilkUtil.ReleasePtr(ref _native);
        }
    }
}
