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
        StartStep _startStep;
        GraphicsRasterizerState _lightRasterState;
        GraphicsDepthState _lightDepthState;
        GraphicsBuffer _lightDataBuffer;
        BufferSegment _lightSegment;

        internal override void Initialize(RendererDX11 renderer, int width, int height)
        {
            _startStep = renderer.GetRenderStep<StartStep>();
            _lightRasterState = new GraphicsRasterizerState()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.Front,
                IsFrontCounterClockwise = false,
                IsDepthClipEnabled = false,
                IsAntialiasedLineEnabled = false,
                IsMultisampleEnabled = false,
                IsScissorEnabled = false,
            };

            _lightDepthState = new GraphicsDepthState()
            {
                DepthComparison = Comparison.GreaterEqual,
                DepthWriteMask = DepthWriteMask.Zero,
                IsDepthEnabled = true,
                IsStencilEnabled = true,
            };

            // Light less
            DepthStencilOperationDescription noSkyStencilOp = new DepthStencilOperationDescription()
            {
                Comparison = Comparison.Equal,
                DepthFailOperation = StencilOperation.Keep,
                FailOperation = StencilOperation.Keep,
                PassOperation = StencilOperation.Keep,
            };

            _lightDepthState.SetFrontFace(noSkyStencilOp);
            _lightDepthState.SetBackFace(noSkyStencilOp);

            Lighting = new RenderSurface(renderer.Device, width, height, Format.R16G16B16A16_Float);

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
            }

            /* TODO 
             * - rework/implement dynamic ring-buffer (Write using NO_OVERWRITE, DISCARD once full (or first map)).
             * - implement mapping strategy system in GraphicsBuffer. Implement different strategies based on:
             *      -- Dynamic or static (static can allocate to wherever is free in the buffer).
             *      -- Hardware vendor/series (This should come much later down the line).
             *      -- 
             */
        }

        internal override void UpdateSurfaces(RendererDX11 renderer, int width, int height)
        {
            Lighting.Resize(width, height);
        }

        public override void Dispose()
        {
            Lighting.Dispose();
            _lightRasterState.Dispose();
            _lightDepthState.Dispose();

            _lightSegment.Dispose();
            _lightDataBuffer.Dispose();
        }

        internal override void Render(RendererDX11 renderer, SceneRenderDataDX11 scene, Timing time, RenderChain.Link link)
        {
            GraphicsDevice device = renderer.Device;

            Lighting.Clear(renderer.Device, scene.AmbientLightColor);
            device.ResetRenderSurfaces(RenderSurfaceResetMode.NullSurface);
            device.SetRenderSurface(Lighting, 0);

            device.PushState();
            device.SetDepthSurface(_startStep.Depth, GraphicsDepthMode.ReadOnly);
            device.BlendState.SetPreset(BlendingPreset.Additive);
            device.Rasterizer.Current = _lightRasterState;
            device.DepthStencil.Current = _lightDepthState;
            device.DepthStencil.StencilReference = 0;

            RenderPointLights(device, scene);

            renderer.Device.PopState();
        }

        private void RenderPointLights(GraphicsPipe pipe, SceneRenderDataDX11 scene)
        {
            _lightSegment.SetData(pipe, scene.PointLights.Data);

            // Set data buffer on domain and pixel shaders
            _matPoint["LightData"].Value = _lightSegment; // TODO Need to implement a dynamic structured buffer we can reuse here.
            _matPoint["mapDiffuse"].Value = _startStep.Scene;
            _matPoint["mapNormal"].Value = _startStep.Normals;
            //_matPoint["mapSpecular"].Value = _startStep.Specular;
            _matPoint["mapDepth"].Value = _startStep.Depth;
            //_matPoint["mapAOcclusion"].Value = _manager.AmbientOcclusion.Texture;

            _matPoint["invViewProjection"].Value = scene.InvViewProjection;
            _matPoint["cameraPosition"].Value = scene.Camera.Position;

            //set correct buffers and shaders
            pipe.SetVertexSegment(null, 0);
            pipe.SetIndexSegment(null);
            int pointCount = scene.PointLights.ElementCount * 2;

            pipe.Draw(_matPoint, pointCount, PrimitiveTopology.PatchListWith1ControlPoints, 0);

            //// Draw debug light volumes
            //if (_manager.ShowLightVolumes)
            //{
            //    pipe.Rasterizer.Current = _renderer.RasterStates.LightDebugRaster;
            //    pipe.PixelShader = _manager.DebugPixelEffect;
            //    pipe.DepthStencil.SetPreset(DepthStencilPreset.DefaultNoStencil);

            //    pipe.Draw(pointCount, VertexTopology.PatchListWith1ControlPoint, 0);
            //}
        }
    }
}
