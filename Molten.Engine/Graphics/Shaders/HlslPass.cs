namespace Molten.Graphics
{
    /// <summary>
    /// An a base class implementation for key shader components, such as materials, material passes or compute tasks.
    /// </summary>
    public abstract class HlslPass : HlslGraphicsObject
    {
        /// <summary>
        /// The texture samplers to be used with the shader/component.
        /// </summary>
        public GraphicsSampler[] Samplers;

        /// <summary>
        /// Creates a new instance of <see cref="HlslPass"/>. Can only be called by a derived class.
        /// </summary>
        /// <param name="parent">The parnet shader that owns this new instance of <see cref="HlslPass"/>.</param>
        protected HlslPass(HlslShader parent) : 
            base(parent.Device, GraphicsBindTypeFlags.Input)
        {
            Samplers = new GraphicsSampler[0];
            Parent = parent;
            IsEnabled = true;
        }

        /// <summary>
        /// Gets or sets the number of iterations the shader/component should be run.
        /// </summary>
        public int Iterations { get; set; } = 1;

        /// <summary>Gets or sets whether or not the pass will be run.</summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets whether the current pass is initialized.
        /// </summary>
        public bool IsInitialized { get; protected set; }

        public HlslShader Parent { get; }
    }
}
