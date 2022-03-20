using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public unsafe class GraphicsShaderFeatures
    {
        DeviceFeaturesDX11 _features;

        internal GraphicsShaderFeatures(DeviceFeaturesDX11 features)
        {
            _features = features;

            Geometry = true;
            HullAndDomain = true;

            FeatureDataDoubles fData = _features.GetFeatureSupport<FeatureDataDoubles>(Feature.FeatureDoubles);
            DoublePrecision = fData.DoublePrecisionFloatShaderOps > 0;

            // DirectX 11.1 or higher features
            if (_features.FeatureLevel >= D3DFeatureLevel.D3DFeatureLevel111)
            {
                FeatureDataShaderMinPrecisionSupport mData =
                    _features.GetFeatureSupport<FeatureDataShaderMinPrecisionSupport>(Feature.FeatureShaderMinPrecisionSupport);
                
                MinimumPrecision = mData.AllOtherShaderStagesMinPrecision;
                MinimumPrecisionPixelShaders = mData.PixelShaderMinPrecision;
            }
        }

        /// <summary>Gets whether or not geometry shaders are supported.</summary>
        public bool Geometry { get; private set; }
        
        /// <summary>Gets whether or not hull and domain shaders are supported.</summary>
        public bool HullAndDomain { get; private set; }

        /// <summary>Gets whether or not the use of the double-precision shaders in HLSL, is supported. Refer to SharpDX.Direct3D11.FeatureDataDoubles.</summary>
        public bool DoublePrecision { get; private set; }

        /// <summary>
        /// Gets the minimum precision of all non-pixel-shader stages.
        /// </summary>
        public uint MinimumPrecision { get; private set; }

        /// <summary>
        /// Gets the minimum precision of the pixel-shader stage.
        /// </summary>
        public uint MinimumPrecisionPixelShaders { get; private set; }
    }
}
