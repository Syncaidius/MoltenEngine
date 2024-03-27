namespace Molten.Graphics;

internal class CompositionStep : RenderStep
{
    RenderCamera _orthoCamera;
    ObjectRenderData _dummyData;
    IRenderSurface2D _surfaceScene;
    IRenderSurface2D _surfaceLighting;
    IRenderSurface2D _surfaceEmissive;
    Shader _fxCompose;
    ShaderVariable _valLighting;
    ShaderVariable _valEmissive;

    protected override void OnInitialize(RenderService renderer)
    {
        _surfaceScene = renderer.Surfaces[MainSurfaceType.Scene];
        _surfaceLighting = renderer.Surfaces[MainSurfaceType.Lighting];
        _surfaceEmissive = renderer.Surfaces[MainSurfaceType.Emissive];

        ShaderCompileResult result = renderer.Device.Resources.LoadEmbeddedShader("Molten.Assets", "gbuffer_compose.json");
        _fxCompose = result["gbuffer-compose"];

        _valLighting = _fxCompose["mapLighting"];
        _valEmissive = _fxCompose["mapEmissive"];

        _dummyData = new ObjectRenderData();
        _orthoCamera = new RenderCamera(RenderCameraMode.Orthographic);
    }

    public override void Dispose()
    {
        _fxCompose.Dispose();
    }

    internal override void Draw(GpuCommandList cmd, RenderCamera camera, RenderChainContext context, Timing time)
    {
        _orthoCamera.Surface = camera.Surface;

        RectangleF vpBounds = camera.Surface.Viewport.Bounds;

        context.CompositionSurface.ClearImmediate(cmd, camera.BackgroundColor);
        cmd.State.Surfaces.Reset();
        cmd.State.Surfaces[0] = context.CompositionSurface;
        cmd.State.DepthSurface.Value = null;

        cmd.State.Viewports.Reset(camera.Surface.Viewport);
        cmd.State.ScissorRects.Reset((Rectangle)vpBounds);

        _valLighting.Value = _surfaceLighting;
        _valEmissive.Value = _surfaceEmissive;

        ITexture2D sourceSurface = context.HasComposed ? context.PreviousComposition : _surfaceScene;
        RectStyle style = RectStyle.Default;

        Renderer.SpriteBatch.Draw(sourceSurface, vpBounds, Vector2F.Zero, vpBounds.Size, 0, Vector2F.Zero, ref style, _fxCompose, 0, 0);
        Renderer.SpriteBatch.Flush(cmd, _orthoCamera, _dummyData);

        context.SwapComposition();
    }
}
