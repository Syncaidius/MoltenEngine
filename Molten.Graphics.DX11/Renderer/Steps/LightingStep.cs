using System.Runtime.InteropServices;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class LightingStep : RenderStepBase
    {
        Material _matPoint;
        Material _matDebugPoint;
        GraphicsBuffer _lightDataBuffer;
        BufferSegment _lightSegment;

        public override void Initialize(RenderService renderer)
        {
            uint stride = (uint)Marshal.SizeOf<LightData>();
            uint maxLights = 2000; // TODO move to graphics settings
            uint bufferByteSize = stride * maxLights;
            _lightDataBuffer = new GraphicsBuffer(renderer.Device as DeviceDX11, BufferMode.DynamicRing, 
                BindFlag.ShaderResource, bufferByteSize, ResourceMiscFlag.BufferStructured, structuredStride: stride);
            _lightSegment = _lightDataBuffer.Allocate<LightData>(maxLights);

            // Load shaders
            ShaderCompileResult result = renderer.Resources.LoadEmbeddedShader("Molten.Graphics.Assets", "light_point.mfx");
            _matPoint = result[ShaderClassType.Material, "light-point"] as Material;
            _matDebugPoint = result[ShaderClassType.Material, "light-point-debug"] as Material;
        }

        public override void Dispose()
        {
            _lightSegment.Dispose();
            _lightDataBuffer.Dispose();
            _matPoint.Dispose();
            _matDebugPoint.Dispose();
        }

        public override void Render(RenderService renderer, RenderCamera camera, RenderChainContext context, Timing time)
        {
            IRenderSurface2D _surfaceLighting = renderer.Surfaces[MainSurfaceType.Lighting];
            IDepthStencilSurface sDepth = renderer.Surfaces.GetDepth();

            GraphicsCommandQueue cmd = renderer.Device.Cmd;

            _surfaceLighting.Clear(context.Scene.AmbientLightColor, GraphicsPriority.Immediate);
            cmd.ResetRenderSurfaces();
            cmd.SetRenderSurface(_surfaceLighting, 0);
            cmd.DepthSurface.Value = sDepth as DepthStencilSurface;
            cmd.DepthWriteOverride = GraphicsDepthWritePermission.ReadOnly;
            RenderPointLights(renderer, cmd as CommandQueueDX11, camera, context.Scene, sDepth);
        }

        private void RenderPointLights(RenderService renderer, CommandQueueDX11 context, RenderCamera camera, SceneRenderData scene, IDepthStencilSurface dsSurface)
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

            _lightSegment.SetData(context, scene.PointLights.Data);

            // Set data buffer on domain and pixel shaders
            _matPoint.Light.Data.Value = _lightSegment; // TODO Need to implement a dynamic structured buffer we can reuse here.
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
            context.VertexBuffers[0].Value = null;
            context.IndexBuffer.Value = null;
            uint pointCount = scene.PointLights.ElementCount * 2;

            context.BeginDraw(StateConditions.None); // TODO expand use of conditions here
            context.Draw(_matPoint, pointCount, VertexTopology.PatchListWith1ControlPoint, 0);
            context.EndDraw();

            // Draw debug light volumes
            context.BeginDraw(StateConditions.Debug);
            context.Draw(_matDebugPoint, pointCount, VertexTopology.PatchListWith1ControlPoint, 0);
            context.EndDraw();
        }
    }
}
