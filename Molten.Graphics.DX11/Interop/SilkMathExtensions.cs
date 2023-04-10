using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace Molten.Graphics.DX11
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

        public static D3DPrimitiveTopology ToApi(this PrimitiveTopology type)
        {
            return (D3DPrimitiveTopology)type;
        }

        public static D3DPrimitive ToApi(this GeometryHullTopology type)
        {
            return (D3DPrimitive)type;
        }

        public static D3DSrvDimension ToApi(this ShaderResourceDimension dimension)
        {
            return (D3DSrvDimension)dimension;
        }

        public static D3DShaderInputType ToApi(this ShaderInputType type)
        {
            return (D3DShaderInputType)type;
        }

        public static D3DResourceReturnType ToApi(this ShaderReturnType type)
        {
            return (D3DResourceReturnType)type;
        }

        public static D3DShaderInputFlags ToApi(this ShaderInputFlags type)
        {
            return (D3DShaderInputFlags)type;
        }
        public static D3DCBufferType ToApi(this ConstantBufferType type)
        {
            return (D3DCBufferType)type;
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

        public static PrimitiveTopology FromApi(this D3DPrimitiveTopology topology)
        {
            return (PrimitiveTopology)topology;
        }

        public static GeometryHullTopology FromApi(this D3DPrimitive topology)
        {
            return (GeometryHullTopology)topology;
        }

        public static ShaderResourceDimension FromApi(this D3DSrvDimension dimension)
        {
            return (ShaderResourceDimension)dimension;
        }

        public static ShaderInputType FromApi(this D3DShaderInputType type)
        {
            return (ShaderInputType)type;
        }

        public static ShaderReturnType FromApi(this D3DResourceReturnType type)
        {
            return (ShaderReturnType)type;
        }

        public static ShaderInputFlags FromApi(this D3DShaderInputFlags type)
        {
            return (ShaderInputFlags)type;
        }

        public static ConstantBufferType FromApi(this D3DCBufferType type)
        {
            return (ConstantBufferType)type;
        }

        public static ViewportF ToApi(this Silk.NET.Direct3D11.Viewport r)
        {
            return new ViewportF(r.TopLeftX, r.TopLeftY, r.Width, r.Height, r.MinDepth, r.MaxDepth);
        }
    }
}
