namespace Molten.Graphics.DX12.Pipeline
{
    internal class RootSignatureDX12 : GraphicsObject<DeviceDX12>
    {
        public RootSignatureDX12(DeviceDX12 device) : base(device)
        {
        }

        protected override void OnGraphicsRelease()
        {
            
            throw new NotImplementedException();
        }
    }
}
