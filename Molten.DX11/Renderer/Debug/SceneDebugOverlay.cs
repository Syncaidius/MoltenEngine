using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class SceneDebugOverlay : ISceneDebugOverlay
    {
        SceneRenderDataDX11 _data;
        List<DebugOverlayPage> _pages;
        int _page;
        RendererDX11 _renderer;

        internal SceneDebugOverlay(RendererDX11 renderer)
        {
            _renderer = renderer;
            _pages = new List<DebugOverlayPage>();
            _pages.Add(new DebugStatsPage());
            _pages.Add(new DebugBuffersPage());
        }

        public void NextPage()
        {
            _page++;
            if (_page == _pages.Count)
                _page = 0;
        }

        public void PreviousPage()
        {
            _page--;
            if (_page < 0)
                _page = _pages.Count - 1;
        }

        public void SetPage(int pageID)
        {
            _page = MathHelper.Clamp(pageID, 0, _pages.Count - 1);
        }

        public void SetScene(SceneRenderData data)
        {
            _data = data as SceneRenderDataDX11;
        }

        public void Render(SpriteBatch sb)
        {
            if (IsVisible && _data != null && Font != null)
                _pages[_page].Render(Font, _renderer, sb, _data, _data.FinalSurface);
        }

        public bool IsVisible { get; set; } = true;

        public SpriteFont Font { get; set; }
    }
}
