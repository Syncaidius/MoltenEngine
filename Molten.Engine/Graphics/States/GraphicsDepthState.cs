namespace Molten.Graphics
{
    /// <summary>Stores a depth-stencil state for use with a <see cref="GraphicsCommandQueue"/>.</summary>
    public unsafe abstract class GraphicsDepthState : GraphicsObject, IEquatable<GraphicsDepthState>
    {
        public abstract class Face
        {  
            public abstract ComparisonFunction Comparison { get; set; }

            public abstract DepthStencilOperation PassOperation { get; set; }

            public abstract DepthStencilOperation FailOperation { get; set; }

            public abstract DepthStencilOperation DepthFailOperation { get; set; }
        }

        protected GraphicsDepthState(GraphicsDevice device) :
            base(device, GraphicsBindTypeFlags.Input)
        { }

        public override bool Equals(object obj)
        {
            if (obj is GraphicsDepthState other)
                return Equals(other);
            else
                return false;
        }

        public bool Equals(GraphicsDepthState other)
        {
            if (!CompareOperation(BackFace, other.BackFace) || !CompareOperation(FrontFace, other.FrontFace))
                return false;

            return DepthComparison == other.DepthComparison &&
                IsDepthEnabled == other.IsDepthEnabled &&
                IsStencilEnabled == other.IsStencilEnabled &&
                StencilReadMask == other.StencilReadMask &&
                StencilWriteMask == other.StencilWriteMask;
        }

        private static bool CompareOperation(Face op, Face other)
        {
            return op.Comparison == other.Comparison &&
                op.DepthFailOperation == other.DepthFailOperation &&
                op.FailOperation == other.FailOperation &&
                op.PassOperation == other.PassOperation;
        }

        public abstract bool IsDepthEnabled { get; set; }

        public abstract bool IsStencilEnabled { get; set; }

        public abstract DepthWriteFlags WriteFlags { get; set; }

        public abstract ComparisonFunction DepthComparison { get; set; }

        public abstract byte StencilReadMask { get; set; }

        public abstract byte StencilWriteMask { get; set; }

        /// <summary>Gets the description for the front-face depth operation description.</summary>
        public abstract Face FrontFace { get; }

        /// <summary>Gets the description for the back-face depth operation description.</summary>
        public abstract Face BackFace { get; }

        /// <summary>Gets or sets the stencil reference value. The default value is 0.</summary>
        public uint StencilReference { get; set; }

        /// <summary>
        /// Gets or sets the depth write permission. the default value is <see cref="GraphicsDepthWritePermission.Enabled"/>.
        /// </summary>
        public GraphicsDepthWritePermission WritePermission { get; set; } = GraphicsDepthWritePermission.Enabled;
    }
}
