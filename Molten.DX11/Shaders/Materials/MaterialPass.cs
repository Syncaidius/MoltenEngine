using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class MaterialPass : HlslFoundation, IMaterialPass
    {
        internal const int ID_VERTEX = 0;
        internal const int ID_HULL = 1;
        internal const int ID_DOMAIN = 2;
        internal const int ID_GEOMETRY = 3;
        internal const int ID_PIXEL = 4;

        internal readonly static ShaderType[] ShaderTypes = new ShaderType[]
        {
            ShaderType.VertexShader,
            ShaderType.HullShader,
            ShaderType.DomainShader,
            ShaderType.GeometryShader,
            ShaderType.PixelShader
        };

        Material _parent;

        internal MaterialPass(Material material) : base(material.Device)
        {
            _parent = material;

            VertexShader = new ShaderComposition<VertexShader>(false);
            HullShader = new ShaderComposition<HullShader>(true);
            DomainShader = new ShaderComposition<DomainShader>(true);
            GeometryShader = new ShaderComposition<GeometryShader>(true);
            PixelShader = new ShaderComposition<PixelShader>(false);
            Compositions = new ShaderComposition[ShaderTypes.Length];
            Compositions[ID_VERTEX] = VertexShader;
            Compositions[ID_HULL] = HullShader;
            Compositions[ID_DOMAIN] = DomainShader;
            Compositions[ID_GEOMETRY] = GeometryShader;
            Compositions[ID_PIXEL] = PixelShader;
        }

        internal GraphicsValidationResult ValidateInput(PrimitiveTopology topology)
        {
            GraphicsValidationResult result = GraphicsValidationResult.Successful;

            if(HullShader.RawShader != null)
            {
                if (topology < PrimitiveTopology.PatchListWith1ControlPoints)
                    result |= GraphicsValidationResult.HullPatchTopologyExpected;
            }

            return result;
        }

        internal ShaderComposition[] Compositions;

        internal ShaderComposition<VertexShader> VertexShader;

        internal ShaderComposition<HullShader> HullShader;

        internal ShaderComposition<DomainShader> DomainShader;

        internal ShaderComposition<GeometryShader> GeometryShader;

        internal ShaderComposition<PixelShader> PixelShader;

        internal InputPrimitive GeometryPrimitive;

        /// <summary>Gets or sets whether or not the pass will be run.</summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }

        public IMaterial Material => _parent;

    }
}
