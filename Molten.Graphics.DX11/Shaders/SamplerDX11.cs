using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11;

public unsafe class SamplerDX11 : ShaderSampler
{
    public unsafe ID3D11SamplerState* NativePtr => _native;

    ID3D11SamplerState* _native;

    internal unsafe SamplerDX11(DeviceDX11 device, ref ShaderSamplerParameters parameters) :
        base(device, ref parameters)
    {
        SamplerDesc desc = new SamplerDesc()
        {
            AddressU = parameters.AddressU.ToApi(),
            AddressV = parameters.AddressV.ToApi(),
            AddressW = parameters.AddressW.ToApi(),
            ComparisonFunc = parameters.Comparison.ToApi(),
            Filter = Filter.MinMagMipLinear,
            MaxAnisotropy = Math.Max(1, parameters.MaxAnisotropy),
            MaxLOD = parameters.MaxMipMapLod,
            MinLOD = parameters.MinMipMapLod,
            MipLODBias = parameters.LodBias
        };

        ref Color4 bColor = ref parameters.BorderColor;
        desc.BorderColor[0] = bColor.R;
        desc.BorderColor[1] = bColor.G;
        desc.BorderColor[2] = bColor.B;
        desc.BorderColor[3] = bColor.A;

        // Figure out which DX11 filter mode to use. 
        if (parameters.MaxAnisotropy > 0)
        {
            desc.Filter = parameters.IsComparison ? Filter.ComparisonAnisotropic : Filter.Anisotropic;
        }
        else
        {
            string filterName = "";
            if (parameters.MinFilter == parameters.MagFilter)
            {
                if (parameters.MagFilter == parameters.MipFilter)
                    filterName = $"MinMagMip{parameters.MinFilter}";
                else
                    filterName = $"MinMag{parameters.MinFilter}Mip{parameters.MipFilter}";
            }
            else if (parameters.MagFilter == parameters.MipFilter)
            {
                filterName = $"Min{parameters.MinFilter}MagMip{parameters.MagFilter}";
            }
            else
            {
                filterName = $"Min{parameters.MinFilter}Mag{parameters.MagFilter}Mip{parameters.MipFilter}";
            }

            if (parameters.IsComparison)
                filterName = "Comparison" + filterName;

            if (Enum.TryParse(filterName, true, out Filter filter))
                desc.Filter = filter;
            else
                throw new Exception($"Failed to parse filter name '{filterName}'");
        }

        device.Ptr->CreateSamplerState(&desc, ref _native);
    }

    protected override void OnGraphicsRelease()
    {
        NativeUtil.ReleasePtr(ref _native);
    }
}
