namespace Molten.Graphics
{
    /// <summary>
    /// An a base class implementation of key shader components (e.g. name, render states, samplers, etc).
    /// </summary>
    public abstract class HlslFoundation : GraphicsObject, IShaderElement
    {

        /// <summary>
        /// The texture samplers to be used with the shader/component.
        /// </summary>
        internal ShaderStateBank<ShaderSamplerDX11>[] Samplers;

        /// <summary>
        /// The available rasterizer state.
        /// </summary>
        internal ShaderStateBank<RasterizerStateDX11> RasterizerState = new ShaderStateBank<RasterizerStateDX11>();

        /// <summary>
        /// The available blend state.
        /// </summary>
        internal ShaderStateBank<BlendStateDX11> BlendState = new ShaderStateBank<BlendStateDX11>();

        /// <summary>
        ///The available depth state.
        /// </summary>
        internal ShaderStateBank<DepthStateDX11> DepthState = new ShaderStateBank<DepthStateDX11>();

        internal HlslFoundation(GraphicsDevice device) : base(device, GraphicsBindTypeFlags.Input)
        {
            Samplers = new ShaderStateBank<ShaderSamplerDX11>[0];
        }

        protected override sealed void OnApply(GraphicsCommandQueue pipe) { }

        /// <summary>
        /// Gets or sets the number of iterations the shader/component should be run.
        /// </summary>
        public int Iterations { get; set; } = 1;
    }
}
