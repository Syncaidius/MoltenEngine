using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public unsafe class ShaderSamplerDX11 : GraphicsSampler
    {
        public unsafe ID3D11SamplerState* NativePtr => _native;

        ID3D11SamplerState* _native;

        internal ShaderSamplerDX11(DeviceDX11 device, ref GraphicsSamplerParameters parameters) : 
            base(device, ref parameters)
        {
            SamplerDesc desc = new SamplerDesc()
            {
                AddressU = parameters.AddressU.ToApi(),
                AddressV = parameters.AddressV.ToApi(),
                AddressW  = parameters.AddressW.ToApi(),
                ComparisonFunc = parameters.Comparison.ToApi(),
                Filter = parameters.Filter.ToApi(),
                MaxAnisotropy = parameters.MaxAnisotropy,
                MaxLOD = parameters.MaxMipMapLod,
                MinLOD = parameters.MinMipMapLod,
                MipLODBias = parameters.LodBias
            };

            ref Color4 bColor = ref parameters.BorderColor;
            desc.BorderColor[0] = bColor.R;
            desc.BorderColor[1] = bColor.G;
            desc.BorderColor[2] = bColor.B;
            desc.BorderColor[3] = bColor.A;

            device.Ptr->CreateSamplerState(ref desc, ref _native);
        }

        public override void GraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref _native);
        }
    }
}
