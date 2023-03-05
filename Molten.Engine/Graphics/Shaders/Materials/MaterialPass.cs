using System.Collections;

namespace Molten.Graphics
{
    public abstract class MaterialPass : HlslPass, IEnumerable<ShaderComposition>, IEnumerable<ShaderType>
    {
        Material _parent;
        Dictionary<ShaderType, ShaderComposition> _compositions;

        public unsafe void* InputByteCode;

        public MaterialPass(Material material, string name) :
            base(material.Device)
        {
            _parent = material;
            Name = name;
            _compositions = new Dictionary<ShaderType, ShaderComposition>();
        }

        internal void InitializeState(GraphicsStatePreset preset, PrimitiveTopology topology)
        {
            GraphicsStateParameters p = new GraphicsStateParameters(preset, topology);
            InitializeState(ref p);
        }

        internal void InitializeState(ref GraphicsStateParameters parameters)
        {
            if (IsInitialized)
                return;

            OnInitializeState(ref parameters);
            IsInitialized = true;
        }

        protected abstract void OnInitializeState(ref GraphicsStateParameters parameters);

        internal ShaderComposition AddComposition(ShaderType type)
        {
            if (!_compositions.TryGetValue(type, out ShaderComposition comp))
            {
                comp = new ShaderComposition(_parent, type);
                _compositions.Add(type, comp);
            }

            return comp;
        }

        internal GraphicsBindResult ValidateInput(PrimitiveTopology topology)
        {
            GraphicsBindResult result = GraphicsBindResult.Successful;

            if (_compositions.TryGetValue(ShaderType.Hull, out ShaderComposition hs))
            {
                if (topology < PrimitiveTopology.PatchListWith1ControlPoint)
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
        /// Gets or sets the type of geometry shader primitives to output.
        /// </summary>
        public GeometryHullTopology GeometryPrimitive { get; set; }

        /// <summary>
        /// Gets the parent <see cref="Material"/> that the current <see cref="MaterialPass"/> is bound to.
        /// </summary>
        public Material Material => _parent;


        /// <summary>
        /// Gets or sets the depth write permission. the default value is <see cref="GraphicsDepthWritePermission.Enabled"/>.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Enum)]
        public GraphicsDepthWritePermission WritePermission { get; set; } = GraphicsDepthWritePermission.Enabled;

        public bool IsInitialized { get; private set; }
    }
}
