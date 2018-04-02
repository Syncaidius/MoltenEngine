using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class StandardMesh : Mesh<GBufferVertex>
    {
        internal StandardMesh(RendererDX11 renderer, int maxVertices, VertexTopology topology, bool dynamic) : 
            base(renderer, maxVertices, topology, dynamic)
        {

        }

        internal override void Render(GraphicsPipe pipe, RendererDX11 renderer, ObjectRenderData data, SceneRenderDataDX11 sceneData)
        {
            ApplyBuffers(pipe);
            IShaderResource normal = GetResource(1);
            Material mat = _material;

            if (mat == null)
            {
                // Use whichever default one fits the current configuration.
                if (normal == null)
                    mat = renderer.StandardMeshMaterial_NoNormalMap;
                else
                    mat = renderer.StandardMeshMaterial;

                if (mat.HasFlags(MaterialCommonFlags.GBuffer))
                {
                    mat.EmissivePower.Value = EmissivePower;
                }
            }

            mat.World.Value = data.RenderTransform;
            mat.Wvp.Value = Matrix4F.Multiply(data.RenderTransform, sceneData.ViewProjection);

            ApplyResources(mat);
            renderer.Device.Draw(mat, _vertexCount, _topology);
        }
    
        protected override void OnSetMaterial(Material newMaterial)
        {
            base.OnSetMaterial(newMaterial);

            if (!newMaterial.HasFlags(MaterialCommonFlags.GBufferTextures | MaterialCommonFlags.GBuffer | MaterialCommonFlags.Common))
            {
                if (!newMaterial.HasFlags(MaterialCommonFlags.GBufferTextures))
                    _renderer.Device.Log.WriteLine($"Attempt to set material '{newMaterial.Name}' on standard mesh failed: Missing G-Buffer texture variables.");

                if (!newMaterial.HasFlags(MaterialCommonFlags.Common))
                    _renderer.Device.Log.WriteLine($"Attempt to set material '{newMaterial.Name}' on standard mesh failed: Missing common scene constants.");

                if (!newMaterial.HasFlags(MaterialCommonFlags.Object))
                    _renderer.Device.Log.WriteLine($"Attempt to set material '{newMaterial.Name}' on standard mesh failed: Missing object constants.");

                _material = null;
            }
        }
    }
}
