using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
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
        internal RenderSurface Lighting;
        Material _matPoint;
        Material _matDebugPoint;
        StartStep _startStep;
        GraphicsBuffer _lightDataBuffer;
        BufferSegment _lightSegment;

        internal override void Initialize(RendererDX11 renderer, int width, int height)
        {
            _startStep = renderer.GetRenderStep<StartStep>();
            Lighting = new RenderSurface(renderer, width, height, Format.R16G16B16A16_Float);

            int stride = Marshal.SizeOf<LightData>();
            int maxLights = 2000; // TODO move to graphics settings
            int bufferByteSize = stride * maxLights;
            _lightDataBuffer = new GraphicsBuffer(renderer.Device, BufferMode.DynamicRing, BindFlags.ShaderResource, bufferByteSize, ResourceOptionFlags.BufferStructured, structuredStride: stride);
            _lightSegment = _lightDataBuffer.Allocate<LightData>(maxLights);
            LoadShaders(renderer);
        }

        private void LoadShaders(RendererDX11 renderer)
        {
            string source = null;
            string namepace = "Molten.Graphics.Assets.light_point.sbm";
            using (Stream stream = EmbeddedResource.GetStream(namepace, typeof(RendererDX11).Assembly))
            {
                using (StreamReader reader = new StreamReader(stream))
                    source = reader.ReadToEnd();
            }

            if (!string.IsNullOrWhiteSpace(source))
            {
                ShaderCompileResult result = renderer.ShaderCompiler.Compile(source, namepace);
                _matPoint = result["material", "light-point"] as Material;
                _matDebugPoint = result["material", "light-point-debug"] as Material;
            }
        }

        internal override void UpdateSurfaces(RendererDX11 renderer, int width, int height)
        {
            Lighting.Resize(width, height);
        }

        public override void Dispose()
        {
            Lighting.Dispose();
            _lightSegment.Dispose();
            _lightDataBuffer.Dispose();
        }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, SceneRenderData sceneData, LayerRenderData<Renderable> layerData, Timing time, RenderChain.Link link)
        {
            GraphicsDeviceDX11 device = renderer.Device;

            Lighting.Clear(renderer.Device, sceneData.AmbientLightColor);
            device.UnsetRenderSurfaces();
            device.SetRenderSurface(Lighting, 0);
            device.DepthSurface = _startStep.Depth;
            device.DepthWriteOverride = GraphicsDepthWritePermission.ReadOnly;
            RenderPointLights(device, camera, sceneData);
        }

        private void RenderPointLights(GraphicsPipe pipe, RenderCamera camera, SceneRenderData scene)
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
            _matPoint.Light.MapDiffuse.Value = _startStep.Scene;
            _matPoint.Light.MapNormal.Value =  _startStep.Normals;
            _matPoint.Light.MapDepth.Value = _startStep.Depth;

            _matPoint.Light.InvViewProjection.Value = camera.InvViewProjection;
            _matPoint.Light.CameraPosition.Value = camera.Position;
            _matPoint.Scene.MaxSurfaceUV.Value = new Vector2F()
            {
                X = (float)camera.OutputSurface.Width / _startStep.Scene.Width,
                Y = (float)camera.OutputSurface.Height / _startStep.Scene.Height,
            };

            //set correct buffers and shaders
            pipe.SetVertexSegment(null, 0);
            pipe.SetIndexSegment(null);
            int pointCount = scene.PointLights.ElementCount * 2;

            pipe.BeginDraw(StateConditions.None); // TODO expand use of conditions here
            pipe.Draw(_matPoint, pointCount, PrimitiveTopology.PatchListWith1ControlPoints, 0);
            pipe.EndDraw();

            // Draw debug light volumes
            pipe.BeginDraw(StateConditions.Debug);
            pipe.Draw(_matDebugPoint, pointCount, PrimitiveTopology.PatchListWith1ControlPoints, 0);
            pipe.EndDraw();
        }
    }
}
