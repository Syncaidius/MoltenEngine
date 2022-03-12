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
    internal unsafe class SRView : ResourceView<ID3D11ShaderResourceView, ShaderResourceViewDesc>
    {
        internal SRView(Device device) : base(device) { }

        protected override unsafe void OnCreateView(ID3D11Resource* resource, ref ShaderResourceViewDesc desc, ref ID3D11ShaderResourceView* view)
        {
            Device.NativeDevice->CreateShaderResourceView(resource, ref desc, ref view);
        }
    }
}
