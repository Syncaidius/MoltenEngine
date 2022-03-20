using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class LightingStep : RenderStepBase
    {
        Material _matPoint;
        Material _matDebugPoint;
        StartStep _startStep;
        GraphicsBuffer _lightDataBuffer;
        BufferSegment _lightSegment;

        RenderSurface2D _surfaceScene;
        RenderSurface2D _surfaceNormals;
        RenderSurface2D _surfaceLighting;
        DepthStencilSurface _surfaceDepth;

        internal override void Initialize(RendererDX11 renderer)
        {
            _surfaceDepth = renderer.GetDepthSurface();
            _surfaceScene = renderer.GetSurface<RenderSurface2D>(MainSurfaceType.Scene);
            _surfaceNormals = renderer.GetSurface<RenderSurface2D>(MainSurfaceType.Normals);
            _surfaceLighting = renderer.GetSurface<RenderSurface2D>(MainSurfaceType.Lighting);

            uint stride = (uint)Marshal.SizeOf<LightData>();
            uint maxLights = 2000; // TODO move to graphics settings
            uint bufferByteSize = stride * maxLights;
            _lightDataBuffer = new GraphicsBuffer(renderer.Device, BufferMode.DynamicRing, 
                BindFlag.BindShaderResource, bufferByteSize, ResourceMiscFlag.ResourceMiscBufferStructured, structuredStride: stride);
            _lightSegment = _lightDataBuffer.Allocate<LightData>(maxLights);
            LoadShaders(renderer);
        }

        private void LoadShaders(RendererDX11 renderer)
        {
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

        internal override void Render(RendererDX11 renderer, RenderCamera camera, RenderChain.Context context, Timing time)
        {
            Device device = renderer.Device;

            _surfaceLighting.Clear(renderer.Device, context.Scene.AmbientLightColor);
            device.State.ResetRenderSurfaces();
            device.State.SetRenderSurface(_surfaceLighting, 0);
            device.State.DepthSurface.Value = _surfaceDepth;
            device.State.DepthWriteOverride = GraphicsDepthWritePermission.ReadOnly;
            RenderPointLights(device, camera, context.Scene);
        }

        private void RenderPointLights(DeviceContext pipe, RenderCamera camera, SceneRenderData scene)
        {
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

            _lightSegment.SetData(pipe, scene.PointLights.Data);

            // Set data buffer on domain and pixel shaders
            _matPoint.Light.Data.Value = _lightSegment; // TODO Need to implement a dynamic structured buffer we can reuse here.
            _matPoint.Light.MapDiffuse.Value = _surfaceScene;
            _matPoint.Light.MapNormal.Value =  _surfaceNormals;
            _matPoint.Light.MapDepth.Value = _surfaceDepth;

            _matPoint.Light.InvViewProjection.Value = camera.InvViewProjection;
            _matPoint.Light.CameraPosition.Value = camera.Position;
            _matPoint.Scene.MaxSurfaceUV.Value = new Vector2F()
            {
                X = (float)camera.OutputSurface.Width / _surfaceScene.Width,
                Y = (float)camera.OutputSurface.Height / _surfaceScene.Height,
            };

            //set correct buffers and shaders
            pipe.State.VertexBuffers[0].Value = null;
            pipe.State.IndexBuffer.Value = null;
            uint pointCount = scene.PointLights.ElementCount * 2;

            pipe.BeginDraw(StateConditions.None); // TODO expand use of conditions here
            pipe.Draw(_matPoint, pointCount, VertexTopology.PatchListWith1ControlPoint, 0);
            pipe.EndDraw();

            // Draw debug light volumes
            pipe.BeginDraw(StateConditions.Debug);
            pipe.Draw(_matDebugPoint, pointCount, VertexTopology.PatchListWith1ControlPoint, 0);
            pipe.EndDraw();
        }
    }
}
