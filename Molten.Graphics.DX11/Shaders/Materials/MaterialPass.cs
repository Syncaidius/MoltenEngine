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

        internal readonly static ShaderType[] ShaderTypes = new ShaderType[]
        {
            ShaderType.VertexShader,
            ShaderType.HullShader,
            ShaderType.DomainShader,
            ShaderType.GeometryShader,
            ShaderType.PixelShader
        };

        Material _parent;

        internal MaterialPass(Material material, string name) : 
            base(material.Device)
        {
            _parent = material;
            Name = name;

            VertexShader = new ShaderComposition<ID3D11VertexShader>(material, false);
            GeometryShader = new ShaderComposition<ID3D11GeometryShader>(material, true);
            HullShader = new ShaderComposition<ID3D11HullShader>(material, true);
            DomainShader = new ShaderComposition<ID3D11DomainShader>(material, true);
            PixelShader = new ShaderComposition<ID3D11PixelShader>(material, false);

            Compositions = new ShaderComposition[ShaderTypes.Length];
            Compositions[ID_VERTEX] = VertexShader;
            Compositions[ID_HULL] = HullShader;
            Compositions[ID_DOMAIN] = DomainShader;
            Compositions[ID_GEOMETRY] = GeometryShader;
            Compositions[ID_PIXEL] = PixelShader;
        }

        internal GraphicsValidationResult ValidateInput(D3DPrimitiveTopology topology)
        {
            GraphicsValidationResult result = GraphicsValidationResult.Successful;

            if(HullShader.RawShader != null)
            {
                if (topology < D3DPrimitiveTopology.D3D11PrimitiveTopology1ControlPointPatchlist)
                    result |= GraphicsValidationResult.HullPatchTopologyExpected;
            }

            return result;
        }

        internal override void PipelineDispose()
        {
            for (int i = 0; i < Compositions.Length; i++)
                Compositions[i].Dispose();
        }

        internal ShaderComposition[] Compositions;

        internal ShaderComposition<ID3D11VertexShader> VertexShader { get; }

        internal ShaderComposition<ID3D11GeometryShader> GeometryShader { get; }

        internal ShaderComposition<ID3D11HullShader> HullShader { get; }

        internal ShaderComposition<ID3D11DomainShader> DomainShader { get; }

        internal ShaderComposition<ID3D11PixelShader> PixelShader { get; }

        internal D3DPrimitive GeometryPrimitive;

        /// <summary>Gets or sets whether or not the pass will be run.</summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }

        public IMaterial Material => _parent;

    }
}
