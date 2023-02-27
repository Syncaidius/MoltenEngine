namespace Molten.Graphics
{
    public abstract class GraphicsSampler : GraphicsObject
    {
        protected GraphicsSampler(GraphicsDevice device, ref GraphicsSamplerParameters parameters) : 
            base(device, GraphicsBindTypeFlags.Input)
        {
            IsComparisonSampler = parameters.IsComparisonSampler;
        }

        protected override void OnApply(GraphicsCommandQueue cmd) { }

        /// <summary>Gets whether or not the sampler a comparison sampler. This is determined by the <see cref="Filter"/> mode.</summary>
        public bool IsComparisonSampler { get; private set; }
    }
}
