using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

internal static class ResourceInterop
{
    internal static ResourceFlags ToResourceFlags(this GraphicsResourceFlags flags)
    {
        ResourceFlags result = 0;

        if (flags.Has(GraphicsResourceFlags.UnorderedAccess))
            result |= ResourceFlags.AllowUnorderedAccess;

        if (flags.Has(GraphicsResourceFlags.DenyShaderAccess))
            result |= ResourceFlags.DenyShaderResource;

        if (flags.Has(GraphicsResourceFlags.SharedAccess))
            result |= ResourceFlags.AllowSimultaneousAccess;

        if (flags.Has(GraphicsResourceFlags.CrossAdapter))
            result |= ResourceFlags.AllowCrossAdapter;

        return result;
    }

    internal static uint EncodeShader4ComponentMapping(ShaderComponentMapping Src0, ShaderComponentMapping Src1, ShaderComponentMapping Src2, ShaderComponentMapping Src3)
    {
        const int D3D12_SHADER_COMPONENT_MAPPING_MASK = 0x7;
        const int D3D12_SHADER_COMPONENT_MAPPING_SHIFT = 3;
        const int D3D12_SHADER_COMPONENT_MAPPING_ALWAYS_SET_BIT_AVOIDING_ZEROMEM_MISTAKES = 1 << (D3D12_SHADER_COMPONENT_MAPPING_SHIFT * 4);

        return ((uint)Src0 & D3D12_SHADER_COMPONENT_MAPPING_MASK) |
            (((uint)Src1 & D3D12_SHADER_COMPONENT_MAPPING_MASK) << D3D12_SHADER_COMPONENT_MAPPING_SHIFT) |
            (((uint)Src2 & D3D12_SHADER_COMPONENT_MAPPING_MASK) << (D3D12_SHADER_COMPONENT_MAPPING_SHIFT * 2)) |
            (((uint)Src3 & D3D12_SHADER_COMPONENT_MAPPING_MASK) << (D3D12_SHADER_COMPONENT_MAPPING_SHIFT * 3)) |
        D3D12_SHADER_COMPONENT_MAPPING_ALWAYS_SET_BIT_AVOIDING_ZEROMEM_MISTAKES;
    }

    internal static uint EncodeDefault4ComponentMapping()
    {
        return EncodeShader4ComponentMapping(
            ShaderComponentMapping.FromMemoryComponent0, 
            ShaderComponentMapping.FromMemoryComponent1, 
            ShaderComponentMapping.FromMemoryComponent2,
            ShaderComponentMapping.FromMemoryComponent3);
    }
}
