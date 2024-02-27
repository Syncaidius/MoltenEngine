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
}
