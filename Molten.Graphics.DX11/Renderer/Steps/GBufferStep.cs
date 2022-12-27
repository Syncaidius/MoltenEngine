using Molten.Font;

namespace Molten.Graphics
{
    internal class GBufferStep : RenderStepBase
    {
        public override void Dispose() { }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, RenderChainContext cxt, Timing time)
        {
            RenderSurface2D sScene = renderer.Surfaces[MainSurfaceType.Scene];
            RenderSurface2D sNormals = renderer.Surfaces[MainSurfaceType.Normals];
            RenderSurface2D sEmissive = renderer.Surfaces[MainSurfaceType.Emissive];

            cxt.Context.State.SetRenderSurface(sScene, 0);
            cxt.Context.State.SetRenderSurface(sNormals, 1);
            cxt.Context.State.SetRenderSurface(sEmissive, 2);
            cxt.Context.State.DepthSurface.Value = renderer.Surfaces.GetDepth();

            SetMaterialCommon(renderer.StandardMeshMaterial, camera, sScene);
            SetMaterialCommon(renderer.StandardMeshMaterial_NoNormalMap, camera, sScene);

            cxt.Context.State.SetViewports(camera.Surface.Viewport);

            cxt.Context.BeginDraw(cxt.BaseStateConditions);
            renderer.RenderSceneLayer(cxt.Context, cxt.Layer, camera);
            cxt.Context.EndDraw();
        }

        private void SetMaterialCommon(Material material, RenderCamera camera, RenderSurface2D gBufferScene)
        {
            material.Scene.View.Value = camera.View;
            material.Scene.Projection.Value = camera.Projection;
            material.Scene.InvViewProjection.Value = Matrix4F.Invert(camera.ViewProjection);
            material.Scene.ViewProjection.Value = camera.ViewProjection;
            material.Scene.MaxSurfaceUV.Value = new Vector2F()
            {
                X = (float)camera.Surface.Width / gBufferScene.Width,
                Y = (float)camera.Surface.Height / gBufferScene.Height,
            };
        }
    }
}
