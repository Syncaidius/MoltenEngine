namespace Molten.Graphics
{
    internal class CompositionStep : RenderStepBase
    {
        RenderCamera _orthoCamera;
        ObjectRenderData _dummyData;
        IRenderSurface2D _surfaceScene;
        IRenderSurface2D _surfaceLighting;
        IRenderSurface2D _surfaceEmissive;
        Material _matCompose;
        IShaderValue _valLighting;
        IShaderValue _valEmissive;

        public override void Initialize(RenderService renderer)
        {
            _surfaceScene = renderer.Surfaces[MainSurfaceType.Scene];
            _surfaceLighting = renderer.Surfaces[MainSurfaceType.Lighting];
            _surfaceEmissive = renderer.Surfaces[MainSurfaceType.Emissive];

            ShaderCompileResult result = renderer.Resources.LoadEmbeddedShader("Molten.Graphics.Assets", "gbuffer_compose.mfx");
            _matCompose = result[ShaderClassType.Material, "gbuffer-compose"] as Material;

            _valLighting = _matCompose["mapLighting"];
            _valEmissive = _matCompose["mapEmissive"];

            _dummyData = new ObjectRenderData();
            _orthoCamera = new RenderCamera(RenderCameraMode.Orthographic);
        }

        public override void Dispose()
        {
            _matCompose.Dispose();
        }

        public override void Render(RenderService renderer, RenderCamera camera, RenderChainContext context, Timing time)
        {
            _orthoCamera.Surface = camera.Surface;

            RectangleF vpBounds = camera.Surface.Viewport.Bounds;
            CommandQueueDX11 cmd = renderer.Device.Cmd as CommandQueueDX11;

            context.CompositionSurface.Clear(context.Scene.BackgroundColor, GraphicsPriority.Immediate);
            cmd.ResetRenderSurfaces();
            cmd.SetRenderSurface(context.CompositionSurface, 0);
            cmd.DepthSurface.Value = null;
            cmd.DepthWriteOverride = GraphicsDepthWritePermission.Disabled;
            cmd.SetViewports(camera.Surface.Viewport);
            cmd.SetScissorRectangle((Rectangle)vpBounds);

            StateConditions conditions = context.BaseStateConditions | StateConditions.ScissorTest;

            _valLighting.Value = _surfaceLighting;
            _valEmissive.Value = _surfaceEmissive;

            ITexture2D sourceSurface = context.HasComposed ? context.PreviousComposition : _surfaceScene;

            RectStyle style = RectStyle.Default;

            cmd.BeginDraw(conditions); // TODO correctly use pipe + conditions here.
            renderer.SpriteBatch.Draw(sourceSurface, vpBounds, Vector2F.Zero, vpBounds.Size, 0, Vector2F.Zero, ref style, _matCompose, 0, 0);
            renderer.SpriteBatch.Flush(cmd, _orthoCamera, _dummyData);
            cmd.EndDraw();

            context.SwapComposition();
        }
    }
}
