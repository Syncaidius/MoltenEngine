

using System.IO;

namespace Molten.Graphics
{
    /// <summary>Stores a rasterizer state for use with a <see cref="GraphicsCommandQueue"/>.</summary>
    public abstract class GraphicsRasterizerState : GraphicsObject, IEquatable<GraphicsRasterizerState>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="source">An existing <see cref="GraphicsRasterizerState"/> instance from which to copy settings.</param>
        protected GraphicsRasterizerState(GraphicsDevice device, GraphicsRasterizerState source) : 
            base(device, GraphicsBindTypeFlags.Input)
        {
            if(source != null)
            {
                FillingMode = source.FillingMode;
                CullingMode = source.CullingMode;
                IsFrontCounterClockwise = source.IsFrontCounterClockwise;
                DepthBias = source.DepthBias;
                SlopeScaledDepthBias = source.SlopeScaledDepthBias;
                DepthBiasClamp = source.DepthBiasClamp;
                IsDepthClipEnabled = source.IsDepthClipEnabled;
                IsScissorEnabled = source.IsScissorEnabled;
                IsMultisampleEnabled = source.IsMultisampleEnabled;
                IsAntialiasedLineEnabled = source.IsAntialiasedLineEnabled;
                ConservativeRaster = source.ConservativeRaster;
                ForcedSampleCount = source.ForcedSampleCount;
            }
            else
            {
                FillingMode = RasterizerFillingMode.Solid;
                CullingMode = RasterizerCullingMode.Back;
                IsFrontCounterClockwise = false;
                DepthBias = 0;
                SlopeScaledDepthBias = 0.0f;
                DepthBiasClamp = 0.0f;
                IsDepthClipEnabled = true;
                IsScissorEnabled = false;
                IsMultisampleEnabled = false;
                IsAntialiasedLineEnabled = false;
                ConservativeRaster = ConservativeRasterizerMode.Off;
                ForcedSampleCount = 0;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is GraphicsRasterizerState other)
                return Equals(other);
            else
                return false;
        }

        public bool Equals(GraphicsRasterizerState other)
        {
            return CullingMode == other.CullingMode &&
                DepthBias == other.DepthBias &&
                DepthBiasClamp == other.DepthBiasClamp &&
                FillingMode == other.FillingMode &&
                IsAntialiasedLineEnabled == other.IsAntialiasedLineEnabled &&
                IsDepthClipEnabled == other.IsDepthClipEnabled &&
                IsFrontCounterClockwise == other.IsFrontCounterClockwise &&
                IsMultisampleEnabled == other.IsMultisampleEnabled &&
                IsScissorEnabled == other.IsScissorEnabled &&
                SlopeScaledDepthBias == other.SlopeScaledDepthBias &&
                ConservativeRaster == other.ConservativeRaster &&
                ForcedSampleCount == other.ForcedSampleCount;
        }

        public abstract RasterizerCullingMode CullingMode { get; set; }

        public abstract int DepthBias { get; set; }

        public abstract float DepthBiasClamp { get; set; }

        public abstract RasterizerFillingMode FillingMode { get; set; }

        public abstract bool IsAntialiasedLineEnabled { get; set; }

        public abstract bool IsDepthClipEnabled { get; set; }

        public abstract bool IsFrontCounterClockwise { get; set; }

        public abstract bool IsMultisampleEnabled { get; set; }

        public abstract bool IsScissorEnabled { get; set; }

        public abstract float SlopeScaledDepthBias { get; set; }

        public abstract ConservativeRasterizerMode ConservativeRaster { get; set; }

        public abstract uint ForcedSampleCount { get; set; }
    }
}
