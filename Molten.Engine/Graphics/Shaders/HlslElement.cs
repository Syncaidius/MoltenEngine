namespace Molten.Graphics
{
    /// <summary>
    /// An a base class implementation of key shader components (e.g. name, render states, samplers, etc).
    /// </summary>
    public abstract class HlslElement : GraphicsObject
    {

        /// <summary>
        /// The texture samplers to be used with the shader/component.
        /// </summary>
        public GraphicsSampler[] Samplers;

        /// <summary>
        /// The available render state.
        /// </summary>
        public GraphicsState State { get; set; }

        protected HlslElement(GraphicsDevice device) : 
            base(device, GraphicsBindTypeFlags.Input)
        {
            Samplers = new GraphicsSampler[0];
        }

        protected override sealed void OnApply(GraphicsCommandQueue cmd) { }

        /// <summary>
        /// Gets or sets the number of iterations the shader/component should be run.
        /// </summary>
        public int Iterations { get; set; } = 1;
    }
}
