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
        public const char PLACEHOLDER_CHAR = ' ';

        Engine _engine;
        Dictionary<string, FontBinding> _fonts;
        SceneRenderData _renderData;
        IRenderSurface2D _rt;
        IRenderSurface2D _rtTransfer;
        ThreadedQueue<FontGlyphBinding> _pendingGlyphs;
        List<FontPage> _pages;
        Interlocker _fontLocker;
        Interlocker _pageLocker;

        internal FontManager(Logger log, Engine engine)
        {
            _engine = engine;
            _fontLocker = new Interlocker();
            _pageLocker = new Interlocker();
            _fonts = new Dictionary<string, FontBinding>();
            _pendingGlyphs = new ThreadedQueue<FontGlyphBinding>();
            _pages = new List<FontPage>();

            Log = log;

            _rt = _engine.Renderer.Resources.CreateSurface((uint)PageSize, (uint)PageSize, arraySize: 1, flags: TextureFlags.AllowMipMapGeneration);
            _rt.Clear(Color.Transparent);

            _renderData = engine.Renderer.CreateRenderData();
            _renderData.BackgroundColor = Color.Transparent;
            _renderData.IsVisible = false;
            LayerRenderData layer = _renderData.CreateLayerData("font chars");
            _renderData.AddLayer(layer);

            ISpriteRenderer _spriteRenderer = _engine.Renderer.Resources.CreateSpriteRenderer(OnDraw);

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
            _fontLocker.Lock();
            FontFile fFile = null;
            using (FontReader reader = new FontReader(stream, Log, path))
                fFile = reader.ReadFont(true);

            FontBinding binding = new FontBinding(this, fFile);
            Font font = null;

            if (_fonts.TryAdd(path, binding))
                font = new Font(this, binding);
            else
                font = new Font(this, _fonts[path]);

            _fontLocker.Unlock();
            return font;
        }

        internal Font GetFont(string path)
        {
            _fontLocker.Lock();
            Font font = null;

            if (_fonts.TryGetValue(path, out FontBinding binding))
                font = new Font(this, binding);

            _fontLocker.Unlock();
            return font;
        }

        internal void AddCharacter(FontBinding binding, char c, bool render)
        {
            _pageLocker.Lock();

            ushort gIndex = binding.Font.GetGlyphIndex(c);

            // Ensure we're within the bindings array. Sometimes this may be false due to unsupported font glyph data.
            if (gIndex < binding.Glyphs.Length)
            {
                // If the character uses an existing glyph, initialize the character and return.
                if (binding.Glyphs[gIndex] != null)
                {
                    binding.Data[c] = new CharData(gIndex);
                    return;
                }
            }
            else
            {
                // Initialize an empty character. Spritefont will be able to use it.
                binding.Data[c] = new CharData(0);
                return;
            }

            Glyph glyph = binding.Font.GetGlyphByIndex(gIndex);
            Rectangle gBounds = glyph.Bounds;
            gBounds.Width = Math.Max(1, gBounds.Width);
            gBounds.Height = Math.Max(1, gBounds.Height);
            GlyphMetrics gm = binding.Font.GetMetricsByIndex(gIndex);

            FontGlyphBinding glyphBinding = new FontGlyphBinding()
            {
                Glyph = glyph,
                AdvanceWidth = DesignToPixels(binding.Font, gm.AdvanceWidth),
                AdvanceHeight = DesignToPixels(binding.Font, binding.Font.Header.MaxY),
                PWidth = DesignToPixels(binding.Font, gBounds.Width),
                PHeight = DesignToPixels(binding.Font, gBounds.Height),
                YOffset = DesignToPixels(binding.Font, gBounds.Top),
            };

            binding.Glyphs[gIndex] = glyphBinding;

            // Pack the glyph onto the first page we find with enough space
            bool pageFound = false;

            foreach (FontPage page in _pages)
            {
                pageFound = page.Pack(glyphBinding);
                if (pageFound)
                    break;
            }

            if (!pageFound)
            {
                FontPage page = new FontPage(this, _pages.Count);
                _pages.Add(page);

                if (page.Pack(glyphBinding))
                    _pendingGlyphs.Enqueue(glyphBinding);
                else
                    Log.Error($"The Font Manager page size is not large enough to fit character {c} for font '{binding.Font.Info.FullName}'. Skipped");
            }

            _pageLocker.Unlock();
        }

        /// <summary>
        /// Converts font design-units to pixels.
        /// </summary>
        /// <param name="font">The <see cref="FontFile"/> to use when measuring design units.</param>
        /// <param name="designUnits">The design unit value.</param>
        /// <returns></returns>
        private int DesignToPixels(FontFile font, float designUnits)
        {
            return (int)Math.Ceiling(BaseFontSize * designUnits / font.Header.DesignUnitsPerEm);
        }

        /// <summary>
        /// Gets the padding between characters stored in the underlying texture array, in pixels.
        /// </summary>
        public int Padding { get; } = 2;

        /// <summary>
        /// Gets the base font size.
        /// </summary>
        public int BaseFontSize { get; } = 16;

        /// <summary>
        /// Gets the page size of the underlying font texture array.
        /// </summary>
        public int PageSize { get; } = 512;

        /// <summary>
        /// Gets the <see cref="Logger"/> used by the current <see cref="FontManager"/> to log font-load messages, warnings and errors.
        /// </summary>
        public Logger Log { get; }

        /// <summary>
        /// Gets the underlying texture array of the current <see cref="FontManager"/>.
        /// </summary>
        public ITexture2D UnderlyingTexture => _rt;
    }
}
