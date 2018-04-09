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
            Lighting.Dispose();
        }

        internal override void Render(RendererDX11 renderer, SceneRenderDataDX11 scene, Timing time, RenderChain.Link link)
        {
            /* Store lights in an array containing struct data
             *  - Disabled/deleted lights will have a radius of 0
             *  - Add a geometry shader stage to the previous deferred lighting implementation which discards/ignores:
             *      -- 0 tess-factor lights (point, spot and capsule)
             *      -- directional lights with a direction length of 0
             * 
             */
            Lighting.Clear(renderer.Device, scene.AmbientLightColor);
        }
    }
}
