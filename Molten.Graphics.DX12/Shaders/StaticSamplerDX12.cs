using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public unsafe class StaticSamplerDX12 : ShaderSampler
{
    StaticSamplerDesc _desc;

    internal unsafe StaticSamplerDX12(DeviceDX12 device, ShaderSamplerParameters parameters) :
        base(device, parameters)
    {
        _desc = new StaticSamplerDesc()
        {
            AddressU = parameters.AddressU.ToApi(),
            AddressV = parameters.AddressV.ToApi(),
            AddressW = parameters.AddressW.ToApi(),
            ComparisonFunc = parameters.Comparison.ToApi(),
            Filter = Filter.MinMagMipLinear,
            MaxAnisotropy = Math.Max(1, parameters.MaxAnisotropy),
            MinLOD = parameters.MinMipMapLod,
            MaxLOD = parameters.MaxMipMapLod,
            MipLODBias = parameters.LodBias,
            BorderColor = StaticBorderColor.TransparentBlack, // TODO Add support for setting this.
            ShaderRegister = parameters.Slot.HasValue ? parameters.Slot.Value : 0,
            RegisterSpace = parameters.SlotSpace.HasValue ? parameters.SlotSpace.Value : 0,
            ShaderVisibility = ShaderVisibility.All, // TODO Add support for setting this.
        };

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

    internal ref StaticSamplerDesc Desc => ref _desc;
}
