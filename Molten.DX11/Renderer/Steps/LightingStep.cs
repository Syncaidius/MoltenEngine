using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class LightingStep : RenderStepBase
    {
        internal RenderSurface Lighting;

        internal override void Initialize(RendererDX11 renderer, int width, int height)
        {
            UpdateSurfaces(renderer, width, height);
        }

        internal override void UpdateSurfaces(RendererDX11 renderer, int width, int height)
        {
            Lighting = new RenderSurface(renderer.Device, width, height, Format.R16G16B16A16_Float);
        }

        public override void Dispose()
        {

        }

        internal override void Render(RendererDX11 renderer, SceneRenderDataDX11 scene, Timing time, RenderChain.Link link)
        {
            
        }
    }
}
