using Silk.NET.Core.Native;

namespace Molten.Graphics
{
    public unsafe class MaterialPass : HlslFoundation
    {
        internal const int ID_VERTEX = 0;
        internal const int ID_HULL = 1;
        internal const int ID_DOMAIN = 2;
        internal const int ID_GEOMETRY = 3;
        internal const int ID_PIXEL = 4;

        Material _parent;

        public MaterialPass(Material material, string name) : 
            base(material.Device)
        {
            _parent = material;
            Name = name;

            VS = Device.CreateShaderComposition(ShaderType.Vertex, material);
            HS = Device.CreateShaderComposition(ShaderType.Hull, material);
            DS = Device.CreateShaderComposition(ShaderType.Domain, material);
            GS = Device.CreateShaderComposition(ShaderType.Geometry, material);
            PS = Device.CreateShaderComposition(ShaderType.Pixel, material);

            Compositions = new ShaderComposition[5];
            Compositions[ID_VERTEX] = VS;
            Compositions[ID_HULL] = HS;
            Compositions[ID_DOMAIN] = DS;
            Compositions[ID_GEOMETRY] = GS;
            Compositions[ID_PIXEL] = PS;
        }

        internal GraphicsBindResult ValidateInput(D3DPrimitiveTopology topology)
        {
            GraphicsBindResult result = GraphicsBindResult.Successful;

            if(HS.PtrShader != null)
            {
                if (topology < D3DPrimitiveTopology.D3D11PrimitiveTopology1ControlPointPatchlist)
                    result |= GraphicsBindResult.HullPatchTopologyExpected;
            }

            return result;
        }

        public override void GraphicsRelease()
        {
            for (int i = 0; i < Compositions.Length; i++)
                Compositions[i].Dispose();
        }

        public ShaderComposition[] Compositions { get; }

        public ShaderComposition VS { get; }

        public ShaderComposition GS { get; }

        public ShaderComposition HS { get; }

        public ShaderComposition DS { get; }

        public ShaderComposition PS { get; }

        public PrimitiveTopology GeometryPrimitive { get; set; }

        /// <summary>Gets or sets whether or not the pass will be run.</summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets the parent <see cref="IMaterial"/> that the current <see cref="MaterialPass"/> is bound to.
        /// </summary>
        public Material Material => _parent;

    }
}
