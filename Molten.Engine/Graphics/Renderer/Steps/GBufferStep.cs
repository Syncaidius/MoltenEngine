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

            GraphicsQueue queue = renderer.Device.Queue;

            queue.State.Surfaces.Reset();
            queue.State.Surfaces[0].Value = sScene;
            queue.State.Surfaces[1].Value = sNormals;
            queue.State.Surfaces[2].Value = sEmissive;
            queue.State.DepthSurface.Value = renderer.Surfaces.GetDepth();

            if (context.Layer.Renderables.Count == 0)
                return;

            SetShaderCommon(renderer.FxStandardMesh, camera, sScene);
            SetShaderCommon(renderer.FxStandardMesh_NoNormalMap, camera, sScene);

            queue.State.Viewports.Reset(camera.Surface.Viewport);

            queue.Begin();
            renderer.RenderSceneLayer(queue, context.Layer, camera);
            queue.End();
        }

        private void SetShaderCommon(HlslShader shader, RenderCamera camera, IRenderSurface2D gBufferScene)
        {
            shader.Scene.View.Value = camera.View;
            shader.Scene.Projection.Value = camera.Projection;
            shader.Scene.InvViewProjection.Value = Matrix4F.Invert(camera.ViewProjection);
            shader.Scene.ViewProjection.Value = camera.ViewProjection;
            shader.Scene.MaxSurfaceUV.Value = new Vector2F()
            {
                X = (float)camera.Surface.Width / gBufferScene.Width,
                Y = (float)camera.Surface.Height / gBufferScene.Height,
            };
        }
    }
}
