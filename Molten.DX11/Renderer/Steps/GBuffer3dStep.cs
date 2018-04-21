using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class GBuffer3dStep : RenderStepBase
    {
        internal override void Initialize(RendererDX11 renderer, int width, int height)
        {

        }

        internal override void UpdateSurfaces(RendererDX11 renderer, int width, int height) { }

        public override void Dispose() { }

        internal override void Render(RendererDX11 renderer, SceneRenderDataDX11 scene, Timing time, RenderChain.Link link)
        {
            GraphicsDevice device = renderer.Device;

            switch (link.Previous.Step)
            {
                case StartStep start:
                    device.SetRenderSurface(start.Scene, 0);
                    device.SetRenderSurface(start.Normals, 1);
                    device.SetRenderSurface(start.Emissive, 2);
                    device.SetDepthSurface(start.Depth, GraphicsDepthMode.Enabled);

                    SetMaterialCommon(renderer.StandardMeshMaterial, scene);
                    SetMaterialCommon(renderer.StandardMeshMaterial_NoNormalMap, scene);
                    break;

                    // TODO add alternate HDR start step here (which should be used in conjunction HDR textures, HDR RTs and so on).
            }

            device.DepthStencil.SetPreset(DepthStencilPreset.Default);
            device.Rasterizer.SetViewports(scene.FinalSurface.Viewport);
            scene.Render3D(device, renderer);
        }

        private void SetMaterialCommon(Material material, SceneRenderDataDX11 scene)
        {
            material.Common.View.Value = scene.View;
            material.Common.Projection.Value = scene.Projection;
            material.Common.InvViewProjection.Value = Matrix4F.Invert(scene.ViewProjection);
            material.Common.ViewProjection.Value = scene.ViewProjection;
        }
    }
}
