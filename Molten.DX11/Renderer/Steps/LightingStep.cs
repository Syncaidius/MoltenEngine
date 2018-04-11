using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class LightingStep : RenderStepBase
    {
        internal RenderSurface Lighting;
        Material _matPoint;

        internal override void Initialize(RendererDX11 renderer, int width, int height)
        {
            UpdateSurfaces(renderer, width, height);
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
            Lighting = new RenderSurface(renderer.Device, width, height, Format.R16G16B16A16_Float);
        }

        public override void Dispose()
        {
            Lighting.Dispose();
        }

        internal override void Render(RendererDX11 renderer, SceneRenderDataDX11 scene, Timing time, RenderChain.Link link)
        {
            Lighting.Clear(renderer.Device, scene.AmbientLightColor);
        }
    }
}
