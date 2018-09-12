using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// A base class for render steps.
    /// </summary>
    internal abstract class RenderStepBase : IDisposable
    {
        internal abstract void Initialize(RendererDX11 renderer, int width, int height);

        internal abstract void UpdateSurfaces(RendererDX11 renderer, int width, int height);

        internal abstract void Render(RendererDX11 renderer, RenderCamera camera, SceneRenderData sceneData, Timing time, RenderChain.Link link);

        public abstract void Dispose();
    }
}
