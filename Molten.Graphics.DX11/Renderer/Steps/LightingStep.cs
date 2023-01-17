using Silk.NET.Direct3D11;
using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    internal class LightingStep : RenderStepBase
    {
        Material _matPoint;
        Material _matDebugPoint;
        GraphicsBuffer _lightDataBuffer;
        BufferSegment _lightSegment;

        internal override void Initialize(RendererDX11 renderer)
        {
            uint stride = (uint)Marshal.SizeOf<LightData>();
            uint maxLights = 2000; // TODO move to graphics settings
            uint bufferByteSize = stride * maxLights;
            _lightDataBuffer = new GraphicsBuffer(renderer.NativeDevice, BufferMode.DynamicRing, 
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

        internal override void Render(RendererDX11 renderer, RenderCamera camera, RenderChainContext context, Timing time)
        {
            RenderSurface2D _surfaceLighting = renderer.Surfaces[MainSurfaceType.Lighting];
            DepthStencilSurface sDepth = renderer.Surfaces.GetDepth();

            CommandQueueDX11 cmd = renderer.NativeDevice.Cmd;

            _surfaceLighting.Clear(cmd, context.Scene.AmbientLightColor);
            cmd.State.ResetRenderSurfaces();
            cmd.State.SetRenderSurface(_surfaceLighting, 0);
            cmd.State.DepthSurface.Value = sDepth;
            cmd.State.DepthWriteOverride = GraphicsDepthWritePermission.ReadOnly;
            RenderPointLights(renderer, cmd, camera, context.Scene, sDepth);
        }

        private void RenderPointLights(RendererDX11 renderer, CommandQueueDX11 context, RenderCamera camera, SceneRenderData scene, DepthStencilSurface dsSurface)
        {
            RenderSurface2D sScene = renderer.Surfaces[MainSurfaceType.Scene];
            RenderSurface2D sNormals = renderer.Surfaces[MainSurfaceType.Normals];

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
            context.State.VertexBuffers[0].Value = null;
            context.State.IndexBuffer.Value = null;
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
