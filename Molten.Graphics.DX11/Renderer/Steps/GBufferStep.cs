namespace Molten.Graphics
{
    internal class GBufferStep : RenderStepBase
    {
        public override void Dispose() { }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, RenderChainContext context, Timing time)
        {
            RenderSurface2D sScene = renderer.Surfaces[MainSurfaceType.Scene];
            RenderSurface2D sNormals = renderer.Surfaces[MainSurfaceType.Normals];
            RenderSurface2D sEmissive = renderer.Surfaces[MainSurfaceType.Emissive];

            CommandQueueDX11 cmd = renderer.Device.Cmd;

            cmd.State.SetRenderSurface(sScene, 0);
            cmd.State.SetRenderSurface(sNormals, 1);
            cmd.State.SetRenderSurface(sEmissive, 2);
            cmd.State.DepthSurface.Value = renderer.Surfaces.GetDepth();

            SetMaterialCommon(renderer.StandardMeshMaterial, camera, sScene);
            SetMaterialCommon(renderer.StandardMeshMaterial_NoNormalMap, camera, sScene);

            cmd.State.SetViewports(camera.Surface.Viewport);

            cmd.BeginDraw(context.BaseStateConditions);
            renderer.RenderSceneLayer(cmd, context.Layer, camera);
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
