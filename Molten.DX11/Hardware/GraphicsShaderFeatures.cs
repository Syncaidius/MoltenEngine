using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class GraphicsShaderFeatures
    {
        ID3D11Device _d3d;

        internal GraphicsShaderFeatures(ref ID3D11Device d3d, D3DFeatureLevel level)
        {
            _d3d = d3d;

            Geometry = true;
            HullAndDomain = true;
            DoublePrecision = _d3d.CheckFeatureSupport(Feature.ShaderDoubles);

            if (level >= D3DFeatureLevel.D3DFeatureLevel111)
            {
                FeatureDataShaderMinimumPrecisionSupport min = _d3d.CheckShaderMinimumPrecisionSupport();
                MinimumPrecision = (ShaderMinimumPrecisionSupport)min.AllOtherShaderStagesMinPrecision;
                MinimumPrecisionPixelShaders = (ShaderMinimumPrecisionSupport)min.PixelShaderMinPrecision;
            }
        }

        /// <summary>Gets whether or not geometry shaders are supported.</summary>
        public bool Geometry { get; private set; }
        
        /// <summary>Gets whether or not hull and domain shaders are supported.</summary>
        public bool HullAndDomain { get; private set; }

        /// <summary>Gets whether or not the use of the double-precision shaders in HLSL, is supported. Refer to SharpDX.Direct3D11.FeatureDataDoubles.</summary>
        public bool DoublePrecision { get; private set; }

        public ShaderMinimumPrecisionSupport MinimumPrecision { get; set; }

        public ShaderMinimumPrecisionSupport MinimumPrecisionPixelShaders { get; set; }
    }
}
