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
        IShaderValue _valLighting;
        IShaderValue _valEmissive;

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
            GraphicsCommandQueue cmd = renderer.Device.Cmd;

            context.CompositionSurface.Clear(camera.BackgroundColor, GraphicsPriority.Immediate);
            cmd.ResetRenderSurfaces();
            cmd.SetRenderSurface(context.CompositionSurface, 0);
            cmd.DepthSurface.Value = null;
            cmd.SetViewports(camera.Surface.Viewport);
            cmd.SetScissorRectangle((Rectangle)vpBounds);

            _valLighting.Value = _surfaceLighting;
            _valEmissive.Value = _surfaceEmissive;

            ITexture2D sourceSurface = context.HasComposed ? context.PreviousComposition : _surfaceScene;
            RectStyle style = RectStyle.Default;

            cmd.BeginDraw();
            renderer.SpriteBatch.Draw(sourceSurface, vpBounds, Vector2F.Zero, vpBounds.Size, 0, Vector2F.Zero, ref style, _fxCompose, 0, 0);
            renderer.SpriteBatch.Flush(cmd, _orthoCamera, _dummyData);
            cmd.EndDraw();

            context.SwapComposition();
        }
    }
}
