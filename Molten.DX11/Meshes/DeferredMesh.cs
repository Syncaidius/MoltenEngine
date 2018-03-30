using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Meshes
{
    public class DeferredMesh : Mesh<GBufferVertex>
    {
        IShaderValue _matDiffuse;
        IShaderValue _matNormal;
        IShaderValue _matEmissive;

        ITexture2D _texDiffuse;
        ITexture2D _texNormal;
        ITexture2D _texEmissive;

        internal DeferredMesh(RendererDX11 renderer, int maxVertices, VertexTopology topology, bool dynamic) : 
            base(renderer, maxVertices, topology, dynamic)
        {

        }

        internal override void Render(GraphicsPipe pipe, RendererDX11 renderer, ObjectRenderData data, SceneRenderDataDX11 sceneData)
        {
            _matDiffuse.Value = _texDiffuse;
            _matNormal.Value = _texNormal;
            _matEmissive.Value = _texEmissive;

            base.Render(pipe, renderer, data, sceneData);
        }

        protected override void OnSetMaterial(Material newMaterial)
        {
            base.OnSetMaterial(newMaterial);

            if (newMaterial.HasGBufferTextures)
            {
                _matDiffuse = newMaterial[MaterialCompiler.MAP_DIFFUSE];
                _matNormal = newMaterial[MaterialCompiler.MAP_NORMAL];
                _matEmissive = newMaterial[MaterialCompiler.MAP_EMISSIVE];
            }
            else
            {
                // TODO log warning, then use the default material.
                // TODO get shader values for defualt material
            }
        }

        public ITexture2D DiffuseTexture
        {
            get => _texDiffuse;
            set => _texDiffuse = value;
        }

        public ITexture2D NormalTexture
        {
            get => _texNormal;
            set => _texNormal = value;
        }

        public ITexture2D EmissiveTexture
        {
            get => _texEmissive;
            set => _texEmissive = value;
        }
    }
}
