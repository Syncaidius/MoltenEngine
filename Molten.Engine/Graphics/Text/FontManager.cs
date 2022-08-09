using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Collections;
using Molten.Font;

namespace Molten.Graphics
{
    public class FontManager
    {
        Engine _engine;
        ConcurrentDictionary<string, FontBinding> _fonts;
        SceneRenderData _renderData;
        RenderService _renderer; 
        IRenderSurface2D _rt;
        ThreadedQueue<FontGlyphBinding> _pendingGlyphs;

        internal FontManager(Logger log, Engine engine)
        {
            _engine = engine;
            _fonts = new ConcurrentDictionary<string, FontBinding>();
            _pendingGlyphs = new ThreadedQueue<FontGlyphBinding>();
            Log = log;

            _rt = engine.Renderer.Resources.CreateSurface((uint)PageSize, (uint)PageSize, arraySize: 1, flags: TextureFlags.AllowMipMapGeneration);
            _rt.Clear(Color.Transparent);

            _renderData = engine.Renderer.CreateRenderData();
            _renderData.BackgroundColor = Color.Transparent;
            _renderData.IsVisible = false;
            LayerRenderData layer = _renderData.CreateLayerData("font chars");
            _renderData.AddLayer(layer);

            ISpriteRenderer _spriteRenderer = _renderer.Resources.CreateSpriteRenderer(OnDraw);

            ObjectRenderData ord = new ObjectRenderData()
            {
                DepthWriteOverride = GraphicsDepthWritePermission.Disabled,
            };

            _renderData.AddObject(_spriteRenderer, ord, layer);
            _renderData.OnPostRender += _renderData_OnPostRender;
            _renderData.AddObject(new RenderCamera(RenderCameraMode.Orthographic)
            {
                OutputSurface = _rt,
                Flags = RenderCameraFlags.DoNotClear
            });
        }

        private void OnDraw(SpriteBatcher sb)
        {
            RectStyle style = RectStyle.Default;

            while (_pendingGlyphs.TryDequeue(out FontGlyphBinding binding))
            {
                // TODO generate SDF texture here
                // TODO SpriteBatcher needs a way to render to different slices of a render target array.
                //sb.Draw(cache.Location, ref style, cache.GlyphTex);
            }

            _renderData.IsVisible = false;
        }

        private void _renderData_OnPostRender(RenderService renderer, SceneRenderData data)
        {
            _rt.GenerateMipMaps();
        }

        public Font GetFont(string path)
        {
            return null;
        }

        private void AddPage()
        {
            // TODO generate render surface to add a new page. Copy previous texture data to new texture.
        }

        internal int ToPixels(FontFile font, float designUnits)
        {
            return (int)Math.Ceiling(BaseFontSize * designUnits / font.Header.DesignUnitsPerEm);
        }

        public int Padding { get; } = 2;

        public int BaseFontSize { get; } = 16;

        public int PageSize { get; } = 512;

        public Logger Log { get; }
    }
}
