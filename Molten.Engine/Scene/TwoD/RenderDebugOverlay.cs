using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class RenderDebugOverlay : ISceneObject, IRenderable2D
    {
        Scene ISceneObject.Scene { get; set; }

        SceneLayer ISceneObject.Layer { get; set; }

        ISceneDebugOverlay _overlay;

        internal RenderDebugOverlay(ISceneDebugOverlay overlay)
        {
            _overlay = overlay;
        }

        public void Render(SpriteBatch sb)
        {
            _overlay.Render(sb);
        }

        public ISceneDebugOverlay Overlay => _overlay;
    }
}
