using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class SceneDebugOverlay : ISceneDebugOverlay
    {
        SceneRenderData _data;
        int _page;
        RendererDX11 _renderer;

        internal SceneDebugOverlay(RendererDX11 renderer, SceneRenderData scene)
        {
            _renderer = renderer;
            _data = scene;
        }

        public void NextPage()
        {
            _page++;
            if (_page == _renderer.DebugOverlayPages.Count)
                _page = 0;
        }

        public void PreviousPage()
        {
            _page--;
            if (_page < 0)
                _page = _renderer.DebugOverlayPages.Count - 1;
        }

        public void SetPage(int pageID)
        {
            _page = MathHelper.Clamp(pageID, 0, _renderer.DebugOverlayPages.Count - 1);
        }

        public void SetScene(SceneRenderData data)
        {
            _data = data;
        }

        public void Render(SpriteBatch sb)
        {
            if (IsVisible && _data != null && Font != null)
                _renderer.DebugOverlayPages[_page].Render(Font, _renderer, sb, _data, _data.Cameras[0].FinalSurface);
        }

        public bool IsVisible { get; set; } = true;

        public SpriteFont Font { get; set; }
    }
}
