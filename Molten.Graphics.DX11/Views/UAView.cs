using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class UAView : ResourceView<ID3D11UnorderedAccessView1, UnorderedAccessViewDesc1>
    {
        internal UAView(DeviceDX11 device) : base(device) { }

        protected override unsafe void OnCreateView(ID3D11Resource* resource, ref UnorderedAccessViewDesc1 desc, ref ID3D11UnorderedAccessView1* view)
        {
            Device.NativeDevice->CreateUnorderedAccessView1(resource, ref desc, ref view);
        }
    }
}
