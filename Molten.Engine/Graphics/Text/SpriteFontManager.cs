using Molten.Collections;
using Molten.Font;
using Molten.Graphics.SDF;

namespace Molten.Graphics
{
    public class SpriteFontManager : EngineObject
    {
        public const char PLACEHOLDER_CHAR = ' ';

        RenderService _renderer;
        Dictionary<string, SpriteFontBinding> _fonts;
        SceneRenderData _renderData;
        RenderCamera _camera;

        IRenderSurface2D _rtTransfer;
        ThreadedQueue<SpriteFontGlyphBinding> _pendingGlyphs;
        List<SpriteFontPage> _pages;
        Interlocker _fontLocker;
        Interlocker _pageLocker;
        SdfGenerator _sdf;

        internal SpriteFontManager(Logger log, RenderService renderer)
        {
            _renderer = renderer;
            _fontLocker = new Interlocker();
            _pageLocker = new Interlocker();
            _fonts = new Dictionary<string, SpriteFontBinding>();
            _pendingGlyphs = new ThreadedQueue<SpriteFontGlyphBinding>();
            _pages = new List<SpriteFontPage>();
            _sdf = new SdfGenerator();

            Log = log;
        }

        internal void Initialize()
        {
            _renderData = _renderer.CreateRenderData();
            _renderData.IsVisible = false;
            LayerRenderData layer = new LayerRenderData("font chars");
            _renderData.AddLayer(layer);

            SpriteRenderer _spriteRenderer = new SpriteRenderer(_renderer, OnDraw);
            ObjectRenderData data = new ObjectRenderData();

            _renderData.AddObject(_spriteRenderer, data, layer);
            _renderData.OnPostRender += _renderData_OnPostRender;
            _camera = new RenderCamera(RenderCameraMode.Orthographic)
            {
                Surface = _renderer.Device.CreateSurface((uint)PageSize, (uint)PageSize, arraySize: 1, allowMipMapGen: true, name: "Sprite Font Sheet"),
                Flags = RenderCameraFlags.DoNotClear,
                Name = "Sprite Font Camera",
                BackgroundColor = Color.Transparent,
            };

            _camera.Surface.Clear(GraphicsPriority.Immediate, Color.Transparent);
            _renderData.AddObject(_camera);
        }

        private void OnDraw(SpriteBatcher sb)
        {
            RectStyle style = RectStyle.Default;

            if (_pages.Count > _camera.Surface.ArraySize)
            {
                uint newArraySize = (uint)_pages.Count;
                _rtTransfer = _camera.Surface;
                _camera.Surface = _renderer.Device.CreateSurface(
                    (uint)PageSize,
                    (uint)PageSize,
                    arraySize: newArraySize,
                    allowMipMapGen: true);
                _camera.Surface.Clear(GraphicsPriority.Immediate, Color.Transparent);

                RectangleF rtBounds = new RectangleF(0, 0, PageSize, PageSize);
                for(uint i = 0; i < _rtTransfer.ArraySize; i++)
                    sb.Draw(rtBounds, rtBounds, Color.White, _rtTransfer, null, i, i);

                _rtTransfer.Dispose();
                _rtTransfer = null;
                return;
            }

            while (_pendingGlyphs.TryDequeue(out SpriteFontGlyphBinding binding))
            {
                RectangleF gBounds = (RectangleF)binding.Glyph.Bounds;
                Vector2F glyphScale = new Vector2F()
                {
                    X = binding.PWidth / gBounds.Width,
                    Y = binding.PHeight / gBounds.Height,
                };

                Vector2F glyphOffset = new Vector2F()
                {
                    X = -DesignToPixels(binding.Font.File, gBounds.Left),
                    Y = -binding.YOffset,
                };

                Shape shape = binding.Glyph.CreateShape();
                _sdf.Normalize(shape);
                shape.ScaleAndOffset(glyphOffset, glyphScale);

                TextureSliceRef<Color3> sdfRef = _sdf.Generate((uint)binding.PWidth, (uint)binding.PHeight, shape, SdfProjection.Default, 6, FillRule.NonZero);
                _sdf.To8Bit(sdfRef);

                ITexture2D tex = _sdf.ConvertToTexture(_renderer, sdfRef);
                sb.Draw((RectangleF)binding.Location, ref style, tex, null, 0, (uint)binding.PageID);

                sdfRef.Slice.Dispose();
               // tex.Dispose(); -- TODO implement proper GPU disposal handling (only disposes gpu resources after X frames)
            }

            _renderData.IsVisible = false;
        }

        private void _renderData_OnPostRender(RenderService renderer, SceneRenderData data)
        {
            _camera.Surface.GenerateMipMaps(GraphicsPriority.Apply);
        }

        internal SpriteFont LoadFont(Stream stream, string path, float size = 16)
        {
            _fontLocker.Lock();
            FontFile fFile = null;
            using (FontReader reader = new FontReader(stream, Log, path))
                fFile = reader.ReadFont(true);

            SpriteFontBinding binding = new SpriteFontBinding(this, fFile);
            SpriteFont font = null;

            if (_fonts.TryAdd(path, binding))
                font = new SpriteFont(this, binding, size);
            else
                font = new SpriteFont(this, _fonts[path], size);

            _fontLocker.Unlock();
            return font;
        }

        internal SpriteFont GetFont(string path, float size = 16)
        {
            _fontLocker.Lock();
            SpriteFont font = null;

            if (_fonts.TryGetValue(path, out SpriteFontBinding binding))
                font = new SpriteFont(this, binding, size);

            _fontLocker.Unlock();
            return font;
        }

        internal void AddCharacter(SpriteFontBinding binding, char c, bool render)
        {
            _pageLocker.Lock();

            ushort gIndex = binding.File.GetGlyphIndex(c);

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

            Glyph glyph = binding.File.GetGlyphByIndex(gIndex);
            Rectangle gBounds = glyph.Bounds;
            gBounds.Width = Math.Max(1, gBounds.Width);
            gBounds.Height = Math.Max(1, gBounds.Height);
            GlyphMetrics gm = binding.File.GetMetricsByIndex(gIndex);

            SpriteFontGlyphBinding glyphBinding = new SpriteFontGlyphBinding(binding)
            {
                Glyph = glyph,
                AdvanceWidth = DesignToPixels(binding.File, gm.AdvanceWidth),
                AdvanceHeight = DesignToPixels(binding.File, binding.File.Header.MaxY),
                PWidth = DesignToPixels(binding.File, gBounds.Width),
                PHeight = DesignToPixels(binding.File, gBounds.Height),
                YOffset = DesignToPixels(binding.File, gBounds.Top),
            };

            binding.Glyphs[gIndex] = glyphBinding;
            binding.Data[c] = new CharData(gIndex);

            if (render)
            {
                // Pack the glyph onto the first page we find with enough space
                bool pageFound = false;

                foreach (SpriteFontPage page in _pages)
                {
                    pageFound = page.Pack(glyphBinding);
                    if (pageFound)
                    {
                        _pendingGlyphs.Enqueue(glyphBinding);
                        break;
                    }
                }

                if (!pageFound)
                {
                    SpriteFontPage page = new SpriteFontPage(this, _pages.Count);
                    _pages.Add(page);

                    if (page.Pack(glyphBinding))
                        _pendingGlyphs.Enqueue(glyphBinding);
                    else
                        Log.Error($"The Font Manager page size is not large enough to fit character {c} for font '{binding.File.Info.FullName}'. Skipped");
                }

                _renderData.IsVisible = true;
            }

            _pageLocker.Unlock();
        }

        /// <summary>
        /// Converts font design-units to pixels.
        /// </summary>
        /// <param name="font">The <see cref="FontFile"/> to use when measuring design units.</param>
        /// <param name="designUnits">The design unit value.</param>
        /// <returns></returns>
        internal int DesignToPixels(FontFile font, float designUnits)
        {
            return (int)Math.Ceiling(BaseFontSize * designUnits / font.Header.DesignUnitsPerEm);
        }

        protected override void OnDispose()
        {
            _camera.Surface?.Dispose();
            _rtTransfer?.Dispose();
            _renderer.DestroyRenderData(_renderData);
        }

        /// <summary>
        /// Gets the padding between characters stored in the underlying texture array, in pixels.
        /// </summary>
        public int Padding { get; } = 2;

        /// <summary>
        /// Gets the base font size.
        /// </summary>
        public int BaseFontSize { get; } = 64;

        /// <summary>
        /// Gets the page size of the underlying font texture array.
        /// </summary>
        public int PageSize { get; } = 512;

        /// <summary>
        /// Gets the <see cref="Logger"/> used by the current <see cref="SpriteFontManager"/> to log font-load messages, warnings and errors.
        /// </summary>
        public Logger Log { get; }

        /// <summary>
        /// Gets or sets the number of spaces that are represented by a single tab character.
        /// </summary>
        public int TabSize { get; set; } = 3;

        /// <summary>
        /// Gets the underlying texture array of the current <see cref="SpriteFontManager"/>.
        /// </summary>
        public ITexture2D UnderlyingTexture => _camera.Surface;
    }
}
