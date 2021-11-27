using Silk.NET.Direct3D11;
using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Interop between SharpDX DX11 and engine types.</summary>
    public static class SilkInterop
    {
        /// <summary>Converts a comparison mode to a SharpDX.Direct3D11.Comparison.</summary>
        /// <param name="val">The value to convert.</param>
        /// <returns></returns>
        public static ComparisonFunc ToApi(this ComparisonMode val)
        {
            return (ComparisonFunc)val;
        }

        public static Filter ToApi(this SamplerFilter val)
        {
            return (Filter)val;
        }

        public static TextureAddressMode ToApi(this SamplerAddressMode mode)
        {
            return (TextureAddressMode)mode;
        }

        public static InputClassification ToApi(this VertexInputType type)
        {
            return (InputClassification)type;
        }

        public static SharpDX.Direct3D.PrimitiveTopology ToApi(this VertexTopology type)
        {
            return (SharpDX.Direct3D.PrimitiveTopology)type;
        }

        public static Rectangle<int> ToApi(this Rectangle r)
        {
            return new Rectangle<int>(r.X, r.Y, r.Width, r.Height);
        }

        /// <summary>Converts a comparison mode to a SharpDX.Direct3D11.Comparison.</summary>
        /// <param name="val">The value to convert.</param>
        /// <returns></returns>
        public static ComparisonMode FromApi(this ComparisonFunc val)
        {
            return (ComparisonMode)val;
        }

        public static SamplerFilter FromApi(this Filter val)
        {
            return (SamplerFilter)val;
        }

        public static SamplerAddressMode FromApi(this TextureAddressMode mode)
        {
            return (SamplerAddressMode)mode;
        }

        public static VertexInputType FromApi(this InputClassification type)
        {
            return (VertexInputType)type;
        }

        public static VertexTopology FromApi(this SharpDX.Direct3D.PrimitiveTopology topology)
        {
            return (VertexTopology)topology;
        }

        public static Rectangle FromApi(this Rectangle<int> rect)
        {
            return new Rectangle(rect.Origin.X, rect.Origin.Y, rect.Size.X, rect.Size.Y);
        }
    }
}
