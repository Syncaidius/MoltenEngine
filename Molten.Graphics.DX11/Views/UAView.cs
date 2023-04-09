using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class UAView : ResourceView<ID3D11UnorderedAccessView1, UnorderedAccessViewDesc1>
    {
        internal UAView(GraphicsResource resource) : base(resource, GraphicsResourceFlags.UnorderedAccess) { }

        protected override unsafe void OnCreateView(ID3D11Resource* resource, UnorderedAccessViewDesc1* desc, ref ID3D11UnorderedAccessView1* view)
        {
            Device.Ptr->CreateUnorderedAccessView1(resource, desc, ref view);
        }
    }
}
