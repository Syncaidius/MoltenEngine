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
                Fill = source.Fill;
                Cull = source.Cull;
                IsFrontCounterClockwise = source.IsFrontCounterClockwise;
                DepthBias = source.DepthBias;
                SlopeScaledDepthBias = source.SlopeScaledDepthBias;
                DepthBiasClamp = source.DepthBiasClamp;
                IsDepthClipEnabled = source.IsDepthClipEnabled;
                IsScissorEnabled = source.IsScissorEnabled;
                IsMultisampleEnabled = source.IsMultisampleEnabled;
                IsAALineEnabled = source.IsAALineEnabled;
                ConservativeRaster = source.ConservativeRaster;
                ForcedSampleCount = source.ForcedSampleCount;
            }
            else
            {
                Fill = RasterizerFillingMode.Solid;
                Cull = RasterizerCullingMode.Back;
                IsFrontCounterClockwise = false;
                DepthBias = 0;
                SlopeScaledDepthBias = 0.0f;
                DepthBiasClamp = 0.0f;
                IsDepthClipEnabled = true;
                IsScissorEnabled = false;
                IsMultisampleEnabled = false;
                IsAALineEnabled = false;
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
            return Cull == other.Cull &&
                DepthBias == other.DepthBias &&
                DepthBiasClamp == other.DepthBiasClamp &&
                Fill == other.Fill &&
                IsAALineEnabled == other.IsAALineEnabled &&
                IsDepthClipEnabled == other.IsDepthClipEnabled &&
                IsFrontCounterClockwise == other.IsFrontCounterClockwise &&
                IsMultisampleEnabled == other.IsMultisampleEnabled &&
                IsScissorEnabled == other.IsScissorEnabled &&
                SlopeScaledDepthBias == other.SlopeScaledDepthBias &&
                ConservativeRaster == other.ConservativeRaster &&
                ForcedSampleCount == other.ForcedSampleCount;
        }

        [ShaderNode(ShaderNodeParseType.Enum)]
        public abstract RasterizerCullingMode Cull { get; set; }

        [ShaderNode(ShaderNodeParseType.Int32)]
        public abstract int DepthBias { get; set; }

        [ShaderNode(ShaderNodeParseType.Float)]
        public abstract float DepthBiasClamp { get; set; }

        [ShaderNode(ShaderNodeParseType.Enum)]
        public abstract RasterizerFillingMode Fill { get; set; }

        /// <summary>
        /// Gets or sets whether or not anti-aliased line rasterization is enabled.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Bool)]
        public abstract bool IsAALineEnabled { get; set; }

        [ShaderNode(ShaderNodeParseType.Bool)]
        public abstract bool IsDepthClipEnabled { get; set; }

        [ShaderNode(ShaderNodeParseType.Bool)]
        public abstract bool IsFrontCounterClockwise { get; set; }

        [ShaderNode(ShaderNodeParseType.Bool)]
        public abstract bool IsMultisampleEnabled { get; set; }

        [ShaderNode(ShaderNodeParseType.Bool)]
        public abstract bool IsScissorEnabled { get; set; }

        [ShaderNode(ShaderNodeParseType.Float)]
        public abstract float SlopeScaledDepthBias { get; set; }

        [ShaderNode(ShaderNodeParseType.Bool)]
        public abstract ConservativeRasterizerMode ConservativeRaster { get; set; }

        [ShaderNode(ShaderNodeParseType.UInt32)]
        public abstract uint ForcedSampleCount { get; set; }
    }
}
