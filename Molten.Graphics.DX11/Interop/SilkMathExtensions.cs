using Silk.NET.Core.Native;
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
    public static class SilkMathExtensions
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

        public static D3DPrimitiveTopology ToApi(this VertexTopology type)
        {
            return (D3DPrimitiveTopology)type;
        }

        public static Rectangle<int> ToApi(this Rectangle r)
        {
            return new Rectangle<int>(r.X, r.Y, r.Width, r.Height);
        }

        public static Silk.NET.Direct3D11.Viewport ToApi(this ViewportF r)
        {
            return new Silk.NET.Direct3D11.Viewport(r.X, r.Y, r.Width, r.Height, r.MinDepth, r.MaxDepth);
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

        public static VertexTopology FromApi(this D3DPrimitiveTopology topology)
        {
            return (VertexTopology)topology;
        }

        public static Rectangle FromApi(this Rectangle<int> rect)
        {
            return new Rectangle(rect.Origin.X, rect.Origin.Y, rect.Size.X, rect.Size.Y);
        }

        public static ViewportF ToApi(this Silk.NET.Direct3D11.Viewport r)
        {
            return new ViewportF(r.TopLeftX, r.TopLeftY, r.Width, r.Height, r.MinDepth, r.MaxDepth);
        }
    }
}
