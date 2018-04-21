using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Represents an indexed mesh. These store mesh data by referring to vertices using index values stored in an index buffer. 
    /// In most cases this reduces the vertex data size drastically.</summary>
    /// <seealso cref="Molten.Graphics.IIndexedMesh" />
    public class StandardIndexedMesh : IndexedMesh<GBufferVertex>
    {
        internal StandardIndexedMesh(RendererDX11 renderer, int maxVertices, int maxIndices, VertexTopology topology, IndexBufferFormat indexFormat, bool dynamic) :
            base(renderer, maxVertices, maxIndices, topology, indexFormat, dynamic)
        { }

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
                    mat.GBuffer.EmissivePower.Value = EmissivePower;
                }
            }

            mat.Object.World.Value = data.RenderTransform;
            mat.Object.Wvp.Value = Matrix4F.Multiply(data.RenderTransform, sceneData.ViewProjection);

            ApplyResources(mat);
            renderer.Device.DrawIndexed(mat, _indexCount, _topology);
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
