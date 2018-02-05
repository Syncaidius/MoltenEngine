using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public static class SharpDX11Interop
    {
        /// <summary>Converts a comparison mode to a SharpDX.Direct3D11.Comparison.</summary>
        /// <param name="val">The value to convert.</param>
        /// <returns></returns>
        public static SharpDX.Direct3D11.Comparison ToApi(this ComparisonMode val)
        {
            return (SharpDX.Direct3D11.Comparison)val;
        }

        public static SharpDX.Direct3D11.Filter ToApi(this SamplerFilter val)
        {
            return (SharpDX.Direct3D11.Filter)val;
        }

        public static SharpDX.Direct3D11.TextureAddressMode ToApi(this SamplerAddressMode mode)
        {
            return (SharpDX.Direct3D11.TextureAddressMode)mode;
        }

        public static SharpDX.Direct3D11.InputClassification ToApi(this VertexInputType type)
        {
            return (SharpDX.Direct3D11.InputClassification)type;
        }

        public static SharpDX.Direct3D.PrimitiveTopology ToApi(this VertexTopology type)
        {
            return (SharpDX.Direct3D.PrimitiveTopology)type;
        }

        /// <summary>Converts a comparison mode to a SharpDX.Direct3D11.Comparison.</summary>
        /// <param name="val">The value to convert.</param>
        /// <returns></returns>
        public static ComparisonMode FromApi(this SharpDX.Direct3D11.Comparison val)
        {
            return (ComparisonMode)val;
        }

        public static SamplerFilter FromApi(this SharpDX.Direct3D11.Filter val)
        {
            return (SamplerFilter)val;
        }

        public static SamplerAddressMode FromApi(this SharpDX.Direct3D11.TextureAddressMode mode)
        {
            return (SamplerAddressMode)mode;
        }

        public static VertexInputType FromApi(this SharpDX.Direct3D11.InputClassification type)
        {
            return (VertexInputType)type;
        }

        public static VertexTopology FromApi(this SharpDX.Direct3D.PrimitiveTopology topology)
        {
            return (VertexTopology)topology;
        }
    }
}
