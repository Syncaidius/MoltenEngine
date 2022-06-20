using Molten.Collections;
using Molten.Font;
using Molten.Graphics.SDF;
using System.Diagnostics;

namespace Molten.Graphics
{
    public class TextFontSource : EngineObject
    {
        struct CharData
        {
            public ushort GlyphIndex;

            public bool Initialized;

            public CharData(ushort gIndex)
            {
                GlyphIndex = gIndex; 
                Initialized = true;
            }
        }

        public const int MIN_PAGE_SIZE = 128;

        public const int MIN_POINTS_PER_CURVE = 2;

        public const char PLACEHOLDER_CHAR = ' ';

        public const int BASE_FONT_SIZE = 64;

        public const int TAB_SIZE = 3; //Number of spaces that one tab character is equivilent to.

        public class CachedGlyph
        {
            // TODO store kerning/GPOS/GSUB offsets in pairs, on-demand.  

            /// <summary>The location of the character glyph on the font atlas texture.</summary>
            public readonly Rectangle Location;

            internal ITexture2D GlyphTex;

            /// <summary> The advance width (horizontal advance) of the character glyph, in pixels. </summary>
            public readonly int AdvanceWidth;

            /// <summary>
            /// The advance height (vertical advance) of the character glyph, in pixels.
            /// </summary>
            public readonly int AdvanceHeight;

            /// <summary>The number of pixels along the Y-axis that the glyph was offset, before fitting on to the font atlas. </summary>
            public readonly float YOffset;

            internal CachedGlyph(int advWidth, int advHeight, Rectangle location, float yOffset)
            {
                AdvanceWidth = advWidth;
                AdvanceHeight = advHeight;
                Location = location;
                YOffset = yOffset;
            }
        }

        FontFile _font;
        IRenderSurface2D _rt;
        int _pageSize;
        int _charPadding;

        SdfGenerator _sdf;
        BinPacker _packer;
        CachedGlyph[] _glyphCache;
        CharData[] _charData;
        SceneRenderData _renderData;
        RenderService _renderer;
        ThreadedQueue<ushort> _pendingGlyphs;
        Interlocker _interlocker;

        /// <summary>
        /// Creates a new instance of <see cref="TextFontSource"/>.
        /// </summary>
        /// <param name="renderer">The renderer with which to prepare and update the sprite font's character sheet.</param>
        /// <param name="font">The font file from which to source character glyphs.</param>
        /// <param name="tabSize">The number of spaces which represent a single tab character (\t).</param>
        /// <param name="texturePageSize">The size (in pixels) of a single sprite font texture page.</param>
        /// A higher number produces smoother curves, while a lower one will produce faceted, low-poly representations of curves. Setting this too low may produce invalid curves.</param>
        /// <param name="initialPages">The initial number of pages in the underlying sprite font texture atlas. Minimum is 1.</param>
        /// <param name="charPadding">The number of pixels to add as padding around each character placed on to the font atlas. 
        /// Default value is 2. Negative padding can cause characters to overlap.</param>
        internal TextFontSource(RenderService renderer,
            FontFile font,
            int texturePageSize,
            int initialPages,
            int charPadding)
        {
            Debug.Assert(texturePageSize >= MIN_PAGE_SIZE, $"Texture page size must be at least {MIN_PAGE_SIZE}");
            Debug.Assert(initialPages >= 1, $"Initial pages must be at least 1");

            _sdf = new SdfGenerator();
            _renderer = renderer;
            _font = font;
            _interlocker = new Interlocker();
            Name = $"{font.Info.FullName} Font - {EOID}";

            if (_font.GlyphCount > 0)
            {
                _glyphCache = new CachedGlyph[_font.GlyphCount];
            }
            else
            {
                _glyphCache = new CachedGlyph[1];
                _glyphCache[0] = new CachedGlyph(1, 1, new Rectangle(0, 0, 1, 1), 0);
            }

            _charData = new CharData[char.MaxValue];
            _pageSize = texturePageSize;
            _packer = new BinPacker(_pageSize, _pageSize);
            _pendingGlyphs = new ThreadedQueue<ushort>();
            _charPadding = charPadding;

            _rt = renderer.Resources.CreateSurface((uint)_pageSize, (uint)_pageSize, arraySize: (uint)initialPages, flags: TextureFlags.AllowMipMapGeneration);
            _rt.Clear(Color.Transparent);

            _renderData = renderer.CreateRenderData();
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

            // Add placeholder character.
            AddCharacter(PLACEHOLDER_CHAR, false);
            Rectangle pcRect = _glyphCache[_charData[' '].GlyphIndex].Location;
            pcRect.Width *= TAB_SIZE;
            AddCharacter('\t', false, pcRect);
        }

        private void OnDraw(SpriteBatcher sb)
        {
            while (_pendingGlyphs.TryDequeue(out ushort gIndex))
            {
                CachedGlyph cache = _glyphCache[gIndex];
                sb.Draw(cache.Location, Color.White, cache.GlyphTex);
            }

            _renderData.IsVisible = false;
        }

        private void _renderData_OnPostRender(RenderService renderer, SceneRenderData data)
        {
            _rt.GenerateMipMaps();
        }

        internal int ToPixels(float designUnits)
        {
            return (int)Math.Ceiling(BASE_FONT_SIZE * designUnits / _font.Header.DesignUnitsPerEm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        internal CachedGlyph GetCharGlyph(char c)
        {
            if (!_charData[c].Initialized)
                AddCharacter(c, true);

            return _glyphCache[_charData[c].GlyphIndex] ?? _glyphCache[_charData[PLACEHOLDER_CHAR].GlyphIndex];
        }

        private void AddCharacter(char c, bool renderGlyph, Rectangle? customBounds = null)
        {
            ushort gIndex = _font.GetGlyphIndex(c);

            // Ensure we're within the glyph cache range. Sometimes this may be false due to unsupported font glyph data.
            if (gIndex < _glyphCache.Length)
            {
                // If the character uses an existing glyph, initialize the character and return.
                if (_glyphCache[gIndex] != null)
                {
                    _charData[c] = new CharData(gIndex);
                    return;
                }
            }
            else
            {
                // Initialize an empty character. Spritefont will be able to use it.
                _charData[c] = new CharData(0);
                return;
            }

            Glyph g = _font.GetGlyphByIndex(gIndex);
            GlyphMetrics gm = _font.GetMetricsByIndex(gIndex);

            Rectangle gBounds = g.Bounds;
            gBounds.Width = Math.Max(1, gBounds.Width);
            gBounds.Height = Math.Max(1, gBounds.Height);

            int padding2 = _charPadding * 2;
            int pWidth, pHeight;
            int advWidth = ToPixels(gm.AdvanceWidth);
            int advHeight = ToPixels(_font.Header.MaxY);

            if (customBounds.HasValue)
            {
                pWidth = customBounds.Value.Width;
                pHeight = customBounds.Value.Height;
            }
            else
            {
                pWidth = ToPixels(gBounds.Width);
                pHeight = ToPixels(gBounds.Height);
            }

            Vector2F glyphScale = new Vector2F()
            {
                X = (float)pWidth / gBounds.Width,
                Y = (float)pHeight / gBounds.Height,
            };

            Rectangle? paddedLoc = null;
            _interlocker.Lock(() => paddedLoc = _packer.Insert(pWidth + padding2, pHeight + padding2));

            if (paddedLoc == null)
            {
                // TODO trigger a sheet resize.
                //      -- If texture arrays are not supported/allowed, try to make the texture bigger along one dimension instead (power of 2).
                //      -- If texture arrays are allowed, add a new array slice to the texture

                // TEMP - Use the default character as a placeholder
                _renderer.Log.Error($"Unable to add character '{c}' to atlas for font '{_font.Info.FullName}'. Font atlas was full.");
                _glyphCache[gIndex] = GetCharGlyph(PLACEHOLDER_CHAR);
                return;
            }

            Rectangle loc = new Rectangle()
            {
                X = paddedLoc.Value.X + _charPadding,
                Y = paddedLoc.Value.Y + _charPadding,
                Width = pWidth,
                Height = pHeight,
            };

            float yOffset = ToPixels(g.Bounds.Top);
            Vector2F glyphOffset = new Vector2F()
            {
                X = -ToPixels(g.Bounds.Left),
                Y = -yOffset,
            };

            _charData[c] = new CharData(gIndex);
            _glyphCache[gIndex] = new CachedGlyph(advWidth, advHeight, loc, yOffset);

            Shape shape = g.CreateShape();
            _sdf.Normalize(shape);
            shape.ScaleAndOffset(glyphOffset, glyphScale);

            TextureSliceRef<Color3> sdfRef = _sdf.Generate((uint)pWidth, (uint)pHeight, shape, SdfProjection.Default, 6, FillRule.NonZero);

            _sdf.Simulate8bit(sdfRef);
            _glyphCache[gIndex].GlyphTex = _sdf.ConvertToTexture(_renderer, sdfRef);
            sdfRef.Slice.Dispose();

            if (renderGlyph)
            {
                _pendingGlyphs.Enqueue(gIndex);
                _renderData.IsVisible = true;
            }
        }

        protected override void OnDispose()
        {
            _rt.Dispose();
            _renderer.DestroyRenderData(_renderData);
            _renderData.Dispose();
        }

        /// <summary>
        /// Gets the underlying font used to generate the sprite-font.
        /// </summary>
        public FontFile Font => _font;

        /// <summary>
        /// Gets the underlying texture atlas of the sprite font.
        /// </summary>
        public ITexture2D UnderlyingTexture => _rt;
    }
}
