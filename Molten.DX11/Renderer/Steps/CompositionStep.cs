using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class CompositionStep : RenderStepBase
    {
        RenderCamera _orthoCamera;
        ObjectRenderData _dummyData;
        RenderSurface _surfaceScene;
        RenderSurface _surfaceLighting;
        RenderSurface _surfaceEmissive;
        Material _matCompose;
        IShaderValue _valLighting;
        IShaderValue _valEmissive;

        internal override void Initialize(RendererDX11 renderer)
        {
            _surfaceScene = renderer.GetSurface<RenderSurface>(MainSurfaceType.Scene);
            _surfaceLighting = renderer.GetSurface<RenderSurface>(MainSurfaceType.Lighting);
            _surfaceEmissive = renderer.GetSurface<RenderSurface>(MainSurfaceType.Emissive);

            string source = null;
            string namepace = "Molten.Graphics.Assets.gbuffer_compose.sbm";
            using (Stream stream = EmbeddedResource.GetStream(namepace, typeof(RendererDX11).Assembly))
            {
                using (StreamReader reader = new StreamReader(stream))
                    source = reader.ReadToEnd();
            }

            if (!string.IsNullOrWhiteSpace(source))
            {
                ShaderCompileResult result = renderer.ShaderCompiler.Compile(source, namepace);
                _matCompose = result["material", "gbuffer-compose"] as Material;

                _valLighting = _matCompose["mapLighting"];
                _valEmissive = _matCompose["mapEmissive"];
            }

            _dummyData = new ObjectRenderData();
            _orthoCamera = new RenderCamera(RenderCameraMode.Orthographic);
        }

        public override void Dispose()
        {
            _matCompose.Dispose();
        }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, RenderChain.Context context, Timing time)
        {
            _orthoCamera.OutputSurface = camera.OutputSurface;

            Rectangle bounds = new Rectangle(0, 0, camera.OutputSurface.Width, camera.OutputSurface.Height);
            GraphicsDeviceDX11 device = renderer.Device;
            RenderSurfaceBase finalSurface = camera.OutputSurface as RenderSurfaceBase;
            if (!camera.HasFlags(RenderCameraFlags.DoNotClear))
                renderer.ClearIfFirstUse(device, finalSurface, context.Scene.BackgroundColor);

            device.SetRenderSurface(context.CompositionSurface, 0);
            device.DepthSurface = null;
            device.DepthWriteOverride = GraphicsDepthWritePermission.Disabled;
            device.Rasterizer.SetViewports(camera.OutputSurface.Viewport);
            device.Rasterizer.SetScissorRectangle(camera.OutputSurface.Viewport.Bounds);

            StateConditions conditions = StateConditions.ScissorTest;
            conditions |= camera.OutputSurface.SampleCount > 1 ? StateConditions.Multisampling : StateConditions.None;

            _valLighting.Value = _surfaceLighting;
            _valEmissive.Value = _surfaceEmissive;

            renderer.Device.BeginDraw(conditions); // TODO correctly use pipe + conditions here.
            renderer.SpriteBatcher.Draw(_surfaceScene, bounds, Vector2F.Zero, camera.OutputSurface.Viewport.Bounds.Size, Color.White, 0, Vector2F.Zero, _matCompose, 0);
            renderer.SpriteBatcher.Flush(device, _orthoCamera, _dummyData);
            renderer.Device.EndDraw();

            context.SwapComposition();
        }
    }
}
