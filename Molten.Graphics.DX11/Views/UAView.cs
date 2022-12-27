using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class UAView : ResourceView<ID3D11UnorderedAccessView, UnorderedAccessViewDesc>
    {
        internal UAView(DeviceDX11 device) : base(device) { }

        protected override unsafe void OnCreateView(ID3D11Resource* resource, ref UnorderedAccessViewDesc desc, ref ID3D11UnorderedAccessView* view)
        {
            Device.NativeDevice->CreateUnorderedAccessView(resource, ref desc, ref view);
        }
    }
}
