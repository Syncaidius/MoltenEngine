namespace Molten.Graphics;

internal class GBufferStep : RenderStep
{
    public override void Dispose() { }

    protected override void OnInitialize(RenderService service) { }

    internal override void Draw(GpuCommandList cmd, RenderCamera camera, RenderChainContext context, Timing time)
    {
        IRenderSurface2D sScene = Renderer.Surfaces[MainSurfaceType.Scene];
        IRenderSurface2D sNormals = Renderer.Surfaces[MainSurfaceType.Normals];
        IRenderSurface2D sEmissive = Renderer.Surfaces[MainSurfaceType.Emissive];

        cmd.State.Surfaces.Reset();
        cmd.State.Surfaces[0] = sScene;
        cmd.State.Surfaces[1] = sNormals;
        cmd.State.Surfaces[2] = sEmissive;
        cmd.State.DepthSurface.Value = Renderer.Surfaces.GetDepth();

        if (context.Layer.Renderables.Count == 0)
            return;

        SetShaderCommon(Renderer.FxStandardMesh, camera, sScene);
        SetShaderCommon(Renderer.FxStandardMesh_NoNormalMap, camera, sScene);

        cmd.State.Viewports.Reset(camera.Surface.Viewport);
        Renderer.RenderSceneLayer(cmd, context.Layer, camera);
    }

    private void SetShaderCommon(Shader shader, RenderCamera camera, IRenderSurface2D gBufferScene)
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
