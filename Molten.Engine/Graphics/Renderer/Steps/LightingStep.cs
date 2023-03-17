using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    internal class LightingStep : RenderStep
    {
        HlslShader _matPoint;
        HlslShader _matDebugPoint;
        IGraphicsBuffer _lightBuffer;

        internal override void Initialize(RenderService renderer)
        {
            uint stride = (uint)Marshal.SizeOf<LightData>();
            uint maxLights = 2000; // TODO move to graphics settings
            _lightBuffer = renderer.Device.CreateStructuredBuffer<LightData>(BufferFlags.CpuWrite | BufferFlags.GpuRead | BufferFlags.Ring, maxLights, false, true);

            // Load shaders
            ShaderCompileResult result = renderer.Device.LoadEmbeddedShader("Molten.Assets", "light_point.mfx");
            _matPoint = result["light-point"];
            _matDebugPoint = result["light-point-debug"];
        }

        public override void Dispose()
        {
            _lightBuffer.Dispose();
            _matPoint.Dispose();
            _matDebugPoint.Dispose();
        }

        internal override void Render(RenderService renderer, RenderCamera camera, RenderChainContext context, Timing time)
        {
            IRenderSurface2D _surfaceLighting = renderer.Surfaces[MainSurfaceType.Lighting];
            IDepthStencilSurface sDepth = renderer.Surfaces.GetDepth();

            GraphicsCommandQueue cmd = renderer.Device.Cmd;

            _surfaceLighting.Clear(context.Scene.AmbientLightColor, GraphicsPriority.Immediate);
            cmd.ResetRenderSurfaces();
            cmd.SetRenderSurface(_surfaceLighting, 0);
            cmd.DepthSurface.Value = sDepth;
            RenderPointLights(renderer, cmd, camera, context.Scene, sDepth);
        }

        private void RenderPointLights(RenderService renderer, GraphicsCommandQueue cmd, RenderCamera camera, SceneRenderData scene, IDepthStencilSurface dsSurface)
        {
            IRenderSurface2D sScene = renderer.Surfaces[MainSurfaceType.Scene];
            IRenderSurface2D sNormals = renderer.Surfaces[MainSurfaceType.Normals];

            // Calculate camera-specific information for each point light
            LightInstance instance;
            LightData ld;
            for(int i = 0; i < scene.PointLights.ElementCount; i++)
            {
                instance = scene.PointLights.Instances[i];
                ld = scene.PointLights.Data[instance.ID];

                float distFromCam = Vector3F.Distance(camera.Position, scene.PointLights.Data[i].Position);
                float distPercent = Math.Min(1.0f, distFromCam / camera.MaxDrawDistance);
                ld.Transform = Matrix4F.Scaling(instance.Range) * Matrix4F.CreateTranslation(ld.Position) * camera.ViewProjection;
                ld.Transform.Transpose();

                ld.TessFactor = GraphicsSettings.MAX_LIGHT_TESS_FACTOR - (GraphicsSettings.LIGHT_TESS_FACTOR_RANGE * distPercent);
                scene.PointLights.Data[i] = ld;
            }

            _lightBuffer.SetData(GraphicsPriority.Immediate, scene.PointLights.Data);

            // Set data buffer on domain and pixel shaders
            _matPoint.Light.Data.Value = _lightBuffer; // TODO Need to implement a dynamic structured buffer we can reuse here.
            _matPoint.Light.MapDiffuse.Value = sScene;
            _matPoint.Light.MapNormal.Value =  sNormals;
            _matPoint.Light.MapDepth.Value = dsSurface;

            _matPoint.Light.InvViewProjection.Value = camera.InvViewProjection;
            _matPoint.Light.CameraPosition.Value = camera.Position;
            _matPoint.Scene.MaxSurfaceUV.Value = new Vector2F()
            {
                X = (float)camera.Surface.Width / sScene.Width,
                Y = (float)camera.Surface.Height / sScene.Height,
            };

            //set correct buffers and shaders
            cmd.VertexBuffers[0].Value = null;
            cmd.IndexBuffer.Value = null;
            uint pointCount = scene.PointLights.ElementCount * 2;

            cmd.BeginDraw();
            cmd.Draw(_matPoint, pointCount, 0);
            cmd.EndDraw();

            // Draw debug light volumes
            cmd.BeginDraw();
            cmd.Draw(_matDebugPoint, pointCount, 0);
            cmd.EndDraw();
        }
    }
}
