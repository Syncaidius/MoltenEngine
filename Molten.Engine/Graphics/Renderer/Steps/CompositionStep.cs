namespace Molten.Graphics
{
    internal class CompositionStep : RenderStep
    {
        RenderCamera _orthoCamera;
        ObjectRenderData _dummyData;
        IRenderSurface2D _surfaceScene;
        IRenderSurface2D _surfaceLighting;
        IRenderSurface2D _surfaceEmissive;
        HlslShader _fxCompose;
        ShaderVariable _valLighting;
        ShaderVariable _valEmissive;

        internal override void Initialize(RenderService renderer)
        {
            _surfaceScene = renderer.Surfaces[MainSurfaceType.Scene];
            _surfaceLighting = renderer.Surfaces[MainSurfaceType.Lighting];
            _surfaceEmissive = renderer.Surfaces[MainSurfaceType.Emissive];

            ShaderCompileResult result = renderer.Device.LoadEmbeddedShader("Molten.Assets", "gbuffer_compose.mfx");
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

        internal override void Render(RenderService renderer, RenderCamera camera, RenderChainContext context, Timing time)
        {
            _orthoCamera.Surface = camera.Surface;

            RectangleF vpBounds = camera.Surface.Viewport.Bounds;
            GraphicsQueue cmd = renderer.Device.Queue;

            context.CompositionSurface.Clear(GraphicsPriority.Immediate, camera.BackgroundColor);
            cmd.State.Surfaces.Reset();
            cmd.State.Surfaces[0] = context.CompositionSurface;
            cmd.State.DepthSurface.Value = null;

            cmd.State.Viewports.Reset(camera.Surface.Viewport);
            cmd.State.ScissorRects.Reset((Rectangle)vpBounds);

            _valLighting.Value = _surfaceLighting;
            _valEmissive.Value = _surfaceEmissive;

            ITexture2D sourceSurface = context.HasComposed ? context.PreviousComposition : _surfaceScene;
            RectStyle style = RectStyle.Default;

            cmd.Begin();
            renderer.SpriteBatch.Draw(sourceSurface, vpBounds, Vector2F.Zero, vpBounds.Size, 0, Vector2F.Zero, ref style, _fxCompose, 0, 0);
            renderer.SpriteBatch.Flush(cmd, _orthoCamera, _dummyData);
            cmd.End();

            context.SwapComposition();
        }
    }
}
