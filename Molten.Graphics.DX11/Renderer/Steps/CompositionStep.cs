namespace Molten.Graphics
{
    internal class CompositionStep : RenderStepBase
    {
        RenderCamera _orthoCamera;
        ObjectRenderData _dummyData;
        RenderSurface2D _surfaceScene;
        RenderSurface2D _surfaceLighting;
        RenderSurface2D _surfaceEmissive;
        Material _matCompose;
        IShaderValue _valLighting;
        IShaderValue _valEmissive;

        internal override void Initialize(RendererDX11 renderer)
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

        internal override void Render(RendererDX11 renderer, RenderCamera camera, RenderChainContext cxt, Timing time)
        {
            _orthoCamera.Surface = camera.Surface;
            RectangleF vpBounds = camera.Surface.Viewport.Bounds;

            cxt.CompositionSurface.Clear(cxt.Scene.BackgroundColor);
            cxt.Context.State.ResetRenderSurfaces();
            cxt.Context.State.SetRenderSurface(cxt.CompositionSurface, 0);
            cxt.Context.State.DepthSurface.Value = null;
            cxt.Context.State.DepthWriteOverride = GraphicsDepthWritePermission.Disabled;
            cxt.Context.State.SetViewports(camera.Surface.Viewport);
            cxt.Context.State.SetScissorRectangle((Rectangle)vpBounds);

            StateConditions conditions = cxt.BaseStateConditions | StateConditions.ScissorTest;

            _valLighting.Value = _surfaceLighting;
            _valEmissive.Value = _surfaceEmissive;

            ITexture2D sourceSurface = cxt.HasComposed ? cxt.PreviousComposition : _surfaceScene;

            RectStyle style = RectStyle.Default;

            cxt.Context.BeginDraw(conditions); // TODO correctly use pipe + conditions here.
            renderer.SpriteBatcher.Draw(sourceSurface, vpBounds, Vector2F.Zero, vpBounds.Size, 0, Vector2F.Zero, ref style, _matCompose, 0, 0);
            renderer.SpriteBatcher.Flush(cxt.Context, _orthoCamera, _dummyData);
            cxt.Context.EndDraw();

            cxt.SwapComposition();
        }
    }
}
