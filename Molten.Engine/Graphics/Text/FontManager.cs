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
        Dictionary<string, FontBinding> _fonts;
        SceneRenderData _renderData;
        RenderService _renderer; 
        IRenderSurface2D _rt;
        IRenderSurface2D _rtTransfer;
        ThreadedQueue<FontGlyphBinding> _pendingGlyphs;
        ThreadedList<FontPage> _pages;
        object _locker;

        internal FontManager(Logger log, Engine engine)
        {
            _engine = engine;
            _locker = new object();
            _fonts = new Dictionary<string, FontBinding>();
            _pendingGlyphs = new ThreadedQueue<FontGlyphBinding>();
            _pages = new ThreadedList<FontPage>();

            Log = log;

            _rt = _engine.Renderer.Resources.CreateSurface((uint)PageSize, (uint)PageSize, arraySize: 1, flags: TextureFlags.AllowMipMapGeneration);
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

            if(_pages.Count > _rt.ArraySize)
            {
                uint newArraySize = (uint)_pages.Count;
                _rtTransfer = _engine.Renderer.Resources.CreateSurface(
                    (uint)PageSize,
                    (uint)PageSize,
                    arraySize: newArraySize,
                    flags: TextureFlags.AllowMipMapGeneration);

                for(uint i = 0; i < _rt.ArraySize; i++)
                {
                    // TODO support rendering to a particular render surface slice in SpriteBatch.
                    // TODO render each page of the existing _rt to _rtTransfer as one sprite per page.
                    // TODO wait at least 10 frames before disposing of _rt and replacing it with _rtTransfer.
                    // TODO during this time, _pendingGlyphs should render to _rtTransf
                }
                return;
            }

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

        internal Font LoadFont(Stream stream, string path)
        {
            lock (_locker)
            {
                FontFile fFile = null;
                using (FontReader reader = new FontReader(stream, Log, path))
                    fFile = reader.ReadFont(true);

                FontBinding binding = new FontBinding(this, fFile);

                if (_fonts.TryAdd(path, binding))
                    return new Font(this, binding);
                else
                    return new Font(this, _fonts[path]);
            }
        }

        internal Font GetFont(string path)
        {
            lock (_locker)
            {
                if (_fonts.TryGetValue(path, out FontBinding binding))
                    return new Font(this, binding);
                else
                    return null;
            }
        }

        internal int ToPixels(FontFile font, float designUnits)
        {
            return (int)Math.Ceiling(BaseFontSize * designUnits / font.Header.DesignUnitsPerEm);
        }

        public int Padding { get; } = 2;

        public int BaseFontSize { get; } = 16;

        public int PageSize { get; } = 512;

        /// <summary>
        /// Gets the <see cref="Logger"/> used by the current <see cref="FontManager"/> to log font-load messages, warnings and errors.
        /// </summary>
        public Logger Log { get; }
    }
}
