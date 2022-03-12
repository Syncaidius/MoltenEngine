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
    internal unsafe class UAView : ResourceView<ID3D11UnorderedAccessView, UnorderedAccessViewDesc>
    {
        internal UAView(Device device) : base(device) { }

        protected override unsafe void OnCreateView(ID3D11Resource* resource, ref UnorderedAccessViewDesc desc, ref ID3D11UnorderedAccessView* view)
        {
            Device.NativeDevice->CreateUnorderedAccessView(resource, ref desc, ref view);
        }
    }
}
