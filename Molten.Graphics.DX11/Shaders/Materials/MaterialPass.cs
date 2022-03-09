using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            VertexShader = new VSComposition(material, false);
            GeometryShader = new GSComposition(material, true);
            HullShader = new HSComposition(material, true);
            DomainShader = new DSComposition(material, true);
            PixelShader = new PSComposition(material, false);

            Compositions = new ShaderComposition[5];
            Compositions[ID_VERTEX] = VertexShader;
            Compositions[ID_HULL] = HullShader;
            Compositions[ID_DOMAIN] = DomainShader;
            Compositions[ID_GEOMETRY] = GeometryShader;
            Compositions[ID_PIXEL] = PixelShader;
        }

        internal GraphicsBindResult ValidateInput(D3DPrimitiveTopology topology)
        {
            GraphicsBindResult result = GraphicsBindResult.Successful;

            if(HullShader.PtrShader != null)
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

        internal ShaderComposition[] Compositions;

        internal VSComposition VertexShader { get; }

        internal GSComposition GeometryShader { get; }

        internal HSComposition HullShader { get; }

        internal DSComposition DomainShader { get; }

        internal PSComposition PixelShader { get; }

        internal D3DPrimitive GeometryPrimitive;

        /// <summary>Gets or sets whether or not the pass will be run.</summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }

        public IMaterial Material => _parent;

    }
}
