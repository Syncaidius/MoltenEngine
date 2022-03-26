using Silk.NET.Core.Native;

namespace Molten.Graphics
{
    public unsafe class MaterialPass : HlslFoundation, IMaterialPass
    {
        internal const int ID_VERTEX = 0;
        internal const int ID_HULL = 1;
        internal const int ID_DOMAIN = 2;
        internal const int ID_GEOMETRY = 3;
        internal const int ID_PIXEL = 4;

        Material _parent;

        internal MaterialPass(Material material, string name) : 
            base(material.Device)
        {
            _parent = material;
            Name = name;

            VS = new VSComposition(material);
            GS = new GSComposition(material);
            HS = new HSComposition(material);
            DS = new DSComposition(material);
            PS = new PSComposition(material);

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

        internal override void PipelineRelease()
        {
            for (int i = 0; i < Compositions.Length; i++)
                Compositions[i].Dispose();
        }

        internal ShaderComposition[] Compositions { get; }

        internal VSComposition VS { get; }

        internal GSComposition GS { get; }

        internal HSComposition HS { get; }

        internal DSComposition DS { get; }

        internal PSComposition PS { get; }

        internal D3DPrimitive GeometryPrimitive { get; set; }

        /// <summary>Gets or sets whether or not the pass will be run.</summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }

        public IMaterial Material => _parent;

    }
}
