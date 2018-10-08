using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Overlays
{
    public interface IRenderOverlay
    {
        void OnRender(Timing time, SpriteBatcher sb, SpriteFont font, RenderProfiler rendererProfiler, RenderProfiler sceneProfiler, RenderProfiler cameraProfiler);

        /// <summary>
        /// Gets the title of the debug overlay. This must be unique when added to a <see cref="Molten.Graphics.OverlayProvider"/>.
        /// </summary>
        string Title { get; }
    }
}
