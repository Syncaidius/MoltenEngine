using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// An step in the deferred rendering chain.
    /// </summary>
    internal abstract class DeferredRenderStep : IDisposable
    {
        internal abstract void Initialize(RendererDX11 renderer, int width, int height);

        internal abstract void UpdateSurfaces(RendererDX11 renderer, int width, int height);

        internal abstract void Render(RendererDX11 renderer, SceneRenderDataDX11 scene);

        public abstract void Dispose();
    }
}
