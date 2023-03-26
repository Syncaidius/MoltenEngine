using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal static class DescriptionInterop
    {
        internal static Usage ToUsageFlags(this GraphicsResourceFlags flags)
        {
            if (flags.Has(GraphicsResourceFlags.GpuWrite))
            {
                if (flags.Has(GraphicsResourceFlags.CpuRead) || flags.Has(GraphicsResourceFlags.CpuWrite))
                    return Usage.Staging;
                else
                    return Usage.Default;
            }
            else
            {
                if (flags.Has(GraphicsResourceFlags.CpuWrite))
                    return Usage.Dynamic;
                else
                    return Usage.Immutable;
            }
        }

        internal static CpuAccessFlag ToCpuFlags(this GraphicsResourceFlags flags)
        {
            CpuAccessFlag access = CpuAccessFlag.None;

            if (flags.Has(GraphicsResourceFlags.CpuRead))
                access |= CpuAccessFlag.Read;

            if (flags.Has(GraphicsResourceFlags.CpuWrite))
                access |= CpuAccessFlag.Write;

            return access;
        }

        internal static ResourceMiscFlag ToMiscFlags(this GraphicsResourceFlags flags, bool allowMipMapGen)
        {
            ResourceMiscFlag result = 0;

            if (flags.Has(GraphicsResourceFlags.Shared))
                result |= ResourceMiscFlag.Shared;

            if (allowMipMapGen)
                result |= ResourceMiscFlag.GenerateMips;

            return result;
        }

        internal static BindFlag ToBindFlags(this GraphicsResourceFlags flags)
        {
            BindFlag result = 0;

            if (flags.Has(GraphicsResourceFlags.UnorderedAccess))
                result |= BindFlag.UnorderedAccess;

            if (!flags.Has(GraphicsResourceFlags.NoShaderAccess))
                result |= BindFlag.ShaderResource;

            return result;
        }
    }
}
