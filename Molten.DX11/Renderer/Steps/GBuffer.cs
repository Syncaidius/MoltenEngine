using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class GBuffer : DeferredRenderStep
    {
        internal RenderSurface Scene;
        internal RenderSurface Normals;
        internal RenderSurface Specular;
        internal RenderSurface Emissive;
        internal DepthSurface Depth;

        internal override void Initialize(RendererDX11 renderer, int width, int height)
        {
            UpdateSurfaces(renderer, width, height);
        }

        internal override void UpdateSurfaces(RendererDX11 renderer,int width, int height)
        {
            // Dispose of current surfaces
            DisposeSurfaces();

            Scene = new RenderSurface(renderer.Device, width, height, Format.R8G8B8A8_UNorm);
            Normals = new RenderSurface(renderer.Device, width, height, Format.R11G11B10_Float);
            Specular = new RenderSurface(renderer.Device, width, height, Format.R8G8B8A8_UNorm);
            Emissive = new RenderSurface(renderer.Device, width, height, Format.R8G8B8A8_UNorm);
            Depth = new DepthSurface(renderer.Device, width, height, DepthFormat.R24G8_Typeless);
        }

        private void DisposeSurfaces()
        {
            Scene?.Dispose();
            Normals?.Dispose();
            Specular?.Dispose();
            Emissive?.Dispose();
        }

        public override void Dispose()
        {
            DisposeSurfaces();
        }

        internal override void Render(RendererDX11 renderer, SceneRenderDataDX11 scene)
        {
            // Clear surfaces
            Scene.Clear(renderer.Device, scene.BackgroundColor);
            Normals.Clear(renderer.Device, Color.White * 0.5f);
            Specular.Clear(renderer.Device, Color.Black);
            Emissive.Clear(renderer.Device, Color.Black);
        }
    }
}
