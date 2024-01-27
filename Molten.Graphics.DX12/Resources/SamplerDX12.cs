using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public unsafe class SamplerDX12 : ShaderSampler
{
    SamplerDesc _desc;

    internal unsafe SamplerDX12(DeviceDX12 device, ref ShaderSamplerParameters parameters) :
        base(device, ref parameters)
    {
        _desc = new SamplerDesc()
        {
            AddressU = parameters.AddressU.ToApi(),
            AddressV = parameters.AddressV.ToApi(),
            AddressW = parameters.AddressW.ToApi(),
            ComparisonFunc = parameters.Comparison.ToApi(),
            Filter = Filter.MinMagMipLinear,
            MaxAnisotropy = Math.Max(1, parameters.MaxAnisotropy),
            MaxLOD = parameters.MaxMipMapLod,
            MinLOD = parameters.MinMipMapLod,
            MipLODBias = parameters.LodBias,
        };

        ref Color4 bColor = ref parameters.BorderColor;
        _desc.BorderColor[0] = bColor.R;
        _desc.BorderColor[1] = bColor.G;
        _desc.BorderColor[2] = bColor.B;
        _desc.BorderColor[3] = bColor.A;

        // Figure out which DX11 filter mode to use. 
        if (parameters.MaxAnisotropy > 0)
        {
            _desc.Filter = parameters.IsComparison ? Filter.ComparisonAnisotropic : Filter.Anisotropic;
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
                _desc.Filter = filter;
            else
                throw new Exception($"Failed to parse filter name '{filterName}'");
        }
    }

    protected override void OnGraphicsRelease() { }

    internal ref SamplerDesc Desc => ref _desc;
}
