namespace Molten.Graphics
{
    internal class GBufferStep : RenderStepBase
    {
        public override void Dispose() { }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, RenderChain.Context context, Timing time)
        {
            RenderSurface2D sScene = renderer.Surfaces[MainSurfaceType.Scene];
            RenderSurface2D sNormals = renderer.Surfaces[MainSurfaceType.Normals];
            RenderSurface2D sEmissive = renderer.Surfaces[MainSurfaceType.Emissive];

            Device device = renderer.Device;

            device.State.SetRenderSurface(sScene, 0);
            device.State.SetRenderSurface(sNormals, 1);
            device.State.SetRenderSurface(sEmissive, 2);
            device.State.DepthSurface.Value = renderer.Surfaces.GetDepth();

            SetMaterialCommon(renderer.StandardMeshMaterial, camera, sScene);
            SetMaterialCommon(renderer.StandardMeshMaterial_NoNormalMap, camera, sScene);

            device.State.SetViewports(camera.Surface.Viewport);

            device.BeginDraw(context.BaseStateConditions);
            renderer.RenderSceneLayer(device, context.Layer, camera);
            device.EndDraw();
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
