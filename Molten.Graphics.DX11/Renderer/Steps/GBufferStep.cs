namespace Molten.Graphics
{
    internal class GBufferStep : RenderStepBase
    {
        public override void Dispose() { }

        public override void Render(RenderService renderer, RenderCamera camera, RenderChainContext context, Timing time)
        {
            IRenderSurface2D sScene = renderer.Surfaces[MainSurfaceType.Scene];
            IRenderSurface2D sNormals = renderer.Surfaces[MainSurfaceType.Normals];
            IRenderSurface2D sEmissive = renderer.Surfaces[MainSurfaceType.Emissive];

            RendererDX11 dx11Renderer = renderer as RendererDX11;
            CommandQueueDX11 cmd = renderer.Device.Cmd as CommandQueueDX11;

            cmd.SetRenderSurface(sScene, 0);
            cmd.SetRenderSurface(sNormals, 1);
            cmd.SetRenderSurface(sEmissive, 2);
            cmd.DepthSurface.Value = renderer.Surfaces.GetDepth() as DepthStencilSurface;

            SetMaterialCommon(dx11Renderer.StandardMeshMaterial, camera, sScene as RenderSurface2D);
            SetMaterialCommon(dx11Renderer.StandardMeshMaterial_NoNormalMap, camera, sScene as RenderSurface2D);

            cmd.SetViewports(camera.Surface.Viewport);

            cmd.BeginDraw(context.BaseStateConditions);
            dx11Renderer.RenderSceneLayer(cmd, context.Layer, camera);
            cmd.EndDraw();
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
