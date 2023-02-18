using static Molten.Graphics.GraphicsBlendState;

namespace Molten.Graphics
{
    /// <summary>Stores a depth-stencil state for use with a <see cref="GraphicsCommandQueue"/>.</summary>
    public abstract class GraphicsDepthState : GraphicsObject, IEquatable<GraphicsDepthState>
    {
        public abstract class Face
        {  
            /// <summary>
            /// Gets or sets the comparison function for the current depth-stencil facing.
            /// </summary>
            public abstract ComparisonFunction Comparison { get; set; }

            /// <summary>
            /// Gets or sets the stencil pass operation for the current depth-stencil facing.
            /// </summary>
            public abstract DepthStencilOperation PassOperation { get; set; }

            /// <summary>
            /// Gets or sets the stencil fail operation for the current depth-stencil facing.
            /// </summary>
            public abstract DepthStencilOperation FailOperation { get; set; }

            /// <summary>
            /// Gets or sets the depth fail operation for the current depth-stencil facing.
            /// </summary>
            public abstract DepthStencilOperation DepthFailOperation { get; set; }

            /// <summary>
            /// Sets the values of the current <see cref="Face"/> to match that of another.
            /// </summary>
            /// <param name="other">The <see cref="Face"/> from which to copy values.</param>
            public void Set(Face other)
            {
                Comparison = other.Comparison;
                PassOperation = other.PassOperation;
                FailOperation = other.FailOperation;
                DepthFailOperation = other.DepthFailOperation;
            }
        }

        protected GraphicsDepthState(GraphicsDevice device, GraphicsDepthState source) :
            base(device, GraphicsBindTypeFlags.Input)
        {
            FrontFace = CreateFace(true);
            BackFace = CreateFace(false);

            if(source != null)
            {
                BackFace.Set(source.BackFace);
                FrontFace.Set(source.FrontFace);
                IsDepthEnabled = source.IsDepthEnabled;
                IsStencilEnabled = source.IsStencilEnabled;
                DepthWriteEnabled = source.DepthWriteEnabled;
                DepthComparison = source.DepthComparison;
                StencilReadMask = source.StencilReadMask;
                StencilWriteMask = source.StencilWriteMask;
                StencilReference = source.StencilReference;
            }
            else
            {
                // Based on the default DX11 values: https://learn.microsoft.com/en-us/windows/win32/api/d3d11/ns-d3d11-d3d11_depth_stencil_desc
                IsDepthEnabled = true;
                DepthWriteEnabled = true;
                DepthComparison = ComparisonFunction.Less;
                IsStencilEnabled = false;
                StencilReadMask = 255;
                StencilWriteMask = 255;
                FrontFace.Comparison = ComparisonFunction.Always;
                FrontFace.DepthFailOperation = DepthStencilOperation.Keep;
                FrontFace.PassOperation = DepthStencilOperation.Keep;
                FrontFace.FailOperation = DepthStencilOperation.Keep;
                BackFace.Set(FrontFace);
            }
        }

        /// <summary>
        /// Invoked when a new <see cref="Face"/> instance is required for the current <see cref="GraphicsDepthState"/>.
        /// </summary>
        /// <param name="isFrontFace"></param>
        /// <returns></returns>
        protected abstract Face CreateFace(bool isFrontFace);

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

        /// <summary>
        /// Gets or sets whether or not depth-mapping is enabled.
        /// </summary>
        public abstract bool IsDepthEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether or not stencil-mapping is enabled.
        /// </summary>
        public abstract bool IsStencilEnabled { get; set; }

        /// <summary>
        /// Gets or sets the depth write flags.
        /// </summary>
        public abstract bool DepthWriteEnabled { get; set; }

        /// <summary>
        /// Gets or sets the depth comparison function.
        /// </summary>
        public abstract ComparisonFunction DepthComparison { get; set; }

        /// <summary>
        /// Gets or sets the stencil read mask.
        /// </summary>
        public abstract byte StencilReadMask { get; set; }

        /// <summary>
        /// Gets or sets the stencil write mask.
        /// </summary>
        public abstract byte StencilWriteMask { get; set; }

        /// <summary>
        /// Gets or sets whether or not depth bounds testing is enabled. 
        /// <para>This property should throw a <see cref="NotSupportedException"/>, 
        /// if <see cref="GraphicsCapabilities.DepthBoundsTesting"/> is false in <see cref="GraphicsDevice.Adapter"/>.</para>
        /// </summary>
        public abstract bool DepthBoundsTestEnabled { get; set; }

        /// <summary>
        /// Gets or sets the maximum bounds during depth bounds testing.
        /// <para>This value may be ignored if <see cref="DepthBoundsTestEnabled"/> is false, or bounds testing is unsupported.</para>
        /// </summary>
        public abstract float MaxDepthBounds { get; set; }

        /// <summary>
        /// Gets or sets the minimum bounds during depth bounds testing.
        /// <para>This value may be ignored if <see cref="DepthBoundsTestEnabled"/> is false, or bounds testing is unsupported.</para>
        /// </summary>
        public abstract float MinDepthBounds { get; set; }

        /// <summary>Gets the description for the front-face depth operation description.</summary>
        public Face FrontFace { get; }

        /// <summary>Gets the description for the back-face depth operation description.</summary>
        public Face BackFace { get; }

        /// <summary>Gets or sets the stencil reference value. The default value is 0.</summary>
        public uint StencilReference { get; set; }

        /// <summary>
        /// Gets or sets the depth write permission. the default value is <see cref="GraphicsDepthWritePermission.Enabled"/>.
        /// </summary>
        public GraphicsDepthWritePermission WritePermission { get; set; } = GraphicsDepthWritePermission.Enabled;
    }
}
