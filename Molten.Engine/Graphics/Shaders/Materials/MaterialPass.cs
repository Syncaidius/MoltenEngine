using System.Collections;

namespace Molten.Graphics
{
    public unsafe class MaterialPass : HlslElement, IEnumerable<ShaderComposition>, IEnumerable<ShaderType>
    {
        Material _parent;
        Dictionary<ShaderType, ShaderComposition> _compositions;

        public void* InputByteCode { get; set; }

        public MaterialPass(Material material, string name) :
            base(material.Device)
        {
            _parent = material;
            Name = name;

            _compositions = new Dictionary<ShaderType, ShaderComposition>();
        }

        internal ShaderComposition AddComposition(ShaderType type)
        {
            if (!_compositions.TryGetValue(type, out ShaderComposition comp))
            {
                comp = new ShaderComposition(_parent, type);
                _compositions.Add(type, comp);
            }

            return comp;
        }

        internal GraphicsBindResult ValidateInput(VertexTopology topology)
        {
            GraphicsBindResult result = GraphicsBindResult.Successful;

            if (_compositions.TryGetValue(ShaderType.Hull, out ShaderComposition hs))
            {
                if (topology < VertexTopology.PatchListWith1ControlPoint)
                    result |= GraphicsBindResult.HullPatchTopologyExpected;
            }

            return result;
        }

        public override void GraphicsRelease()
        {
            foreach (ShaderComposition c in _compositions.Values)
                c.Dispose();
        }

        public IEnumerator<ShaderComposition> GetEnumerator()
        {
            return _compositions.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _compositions.Keys.GetEnumerator();
        }

        IEnumerator<ShaderType> IEnumerable<ShaderType>.GetEnumerator()
        {
            return _compositions.Keys.GetEnumerator();
        }

        /// <summary>
        /// Gets a <see cref="ShaderComposition"/> from the current <see cref="MaterialPass"/>. 
        /// <para>Returns null if no composition exists for the specified <see cref="ShaderType"/>.</para>
        /// </summary>
        /// <param name="type">The type of shader composition to retrieve.</param>
        /// <returns></returns>
        public ShaderComposition this[ShaderType type]
        {
            get
            {
                _compositions.TryGetValue(type, out ShaderComposition comp);
                return comp;
            }
        }

        /// <summary>
        /// Gets or sets available render state.
        /// </summary>
        public GraphicsState State { get; set; }

        /// <summary>
        /// Gets or sets the type of geometry shader primitives to output.
        /// </summary>
        public PrimitiveTopology GeometryPrimitive { get; set; }

        /// <summary>Gets or sets whether or not the pass will be run.</summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets the parent <see cref="Material"/> that the current <see cref="MaterialPass"/> is bound to.
        /// </summary>
        public Material Material => _parent;

    }
}
