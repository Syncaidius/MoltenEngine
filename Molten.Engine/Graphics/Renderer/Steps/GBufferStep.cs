namespace Molten.Graphics
{
    internal class GBufferStep : RenderStep
    {
        public override void Dispose() { }

        internal override void Render(RenderService renderer, RenderCamera camera, RenderChainContext context, Timing time)
        {
            IRenderSurface2D sScene = renderer.Surfaces[MainSurfaceType.Scene];
            IRenderSurface2D sNormals = renderer.Surfaces[MainSurfaceType.Normals];
            IRenderSurface2D sEmissive = renderer.Surfaces[MainSurfaceType.Emissive];

            GraphicsCommandQueue cmd = renderer.Device.Cmd;

            cmd.SetRenderSurface(sScene, 0);
            cmd.SetRenderSurface(sNormals, 1);
            cmd.SetRenderSurface(sEmissive, 2);
            cmd.DepthSurface.Value = renderer.Surfaces.GetDepth();

            if (context.Layer.Renderables.Count == 0)
                return;

            SetMaterialCommon(renderer.StandardMeshMaterial, camera, sScene);
            SetMaterialCommon(renderer.StandardMeshMaterial_NoNormalMap, camera, sScene);

            cmd.SetViewports(camera.Surface.Viewport);

            cmd.BeginDraw(context.BaseStateConditions);
            renderer.RenderSceneLayer(cmd, context.Layer, camera);
            cmd.EndDraw();
        }

        private void SetMaterialCommon(Material material, RenderCamera camera, IRenderSurface2D gBufferScene)
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
