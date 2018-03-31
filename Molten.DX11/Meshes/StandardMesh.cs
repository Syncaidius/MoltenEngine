using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class StandardMesh : Mesh<GBufferVertex>, IStandardMesh
    {
        IShaderValue _matDiffuse;
        IShaderValue _matNormal;
        IShaderValue _matEmissive;

        internal StandardMesh(RendererDX11 renderer, int maxVertices, VertexTopology topology, bool dynamic) : 
            base(renderer, maxVertices, topology, dynamic)
        {

        }

        internal override void Render(GraphicsPipe pipe, RendererDX11 renderer, ObjectRenderData data, SceneRenderDataDX11 sceneData)
        {
            if (_material == null)
            {
                // use whichever default one fits the current configuration.
            }
            else
            {
                // TODO improve method of getting/setting resources
                _matDiffuse.Value = GetResource(0);
                _matNormal.Value = GetResource(1);
                _matEmissive.Value = GetResource(2);
            }

            base.Render(pipe, renderer, data, sceneData);
        }

        protected override void OnSetMaterial(Material newMaterial)
        {
            base.OnSetMaterial(newMaterial);

            if (newMaterial.HasGBufferTextures && newMaterial.HasCommonConstants && newMaterial.HasObjectConstants)
            {
                _matDiffuse = newMaterial[MaterialCompiler.MAP_DIFFUSE];
                _matNormal = newMaterial[MaterialCompiler.MAP_NORMAL];
                _matEmissive = newMaterial[MaterialCompiler.MAP_EMISSIVE];
            }
            else
            {
                if (!newMaterial.HasGBufferTextures)
                    _renderer.Device.Log.WriteLine($"Attempt to set material '{newMaterial.Name}' on standard mesh failed: Missing G-Buffer texture variables.");

                if (!newMaterial.HasCommonConstants)
                    _renderer.Device.Log.WriteLine($"Attempt to set material '{newMaterial.Name}' on standard mesh failed: Missing common scene constants.");

                if (!newMaterial.HasObjectConstants)
                    _renderer.Device.Log.WriteLine($"Attempt to set material '{newMaterial.Name}' on standard mesh failed: Missing object constants.");

                _material = null;
                _matDiffuse = null;
                _matNormal = null;
                _matEmissive = null;
            }
        }
    }
}
