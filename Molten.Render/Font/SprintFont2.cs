using Molten.Collections;
using Molten.Font;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class SpriteFont2 : IDisposable
    {
        struct CharData
        {
            public ushort GlyphID;

            public bool Initialized;

            public CharData(ushort gIndex) { GlyphID = gIndex; Initialized = true; }
        }

        public const int MIN_PAGE_SIZE = 128;

        public const int MIN_POINTS_PER_CURVE = 2;

        public class GlyphCache
        {
            // TODO store kerning/GPOS/GSUB offsets in pairs, on-demand.  

            /// <summary>The location of the character glyph on the font atlas texture.</summary>
            public readonly Rectangle Location;

            // The shapes which make up the character glyph. Required for rendering to sheet or generating a 3D model.
            internal List<Vector2F> GlyphMesh = new List<Vector2F>();

            /// <summary>Metrics about the current character glyph.</summary>
            public readonly GlyphMetrics Metrics;

            /// <summary> The advance width (horizontal advance) of the character glyph. </summary>
            public readonly int AdvanceWidth;

            internal GlyphCache(int advWidth, Rectangle location, GlyphMetrics gm)
            {
                AdvanceWidth = advWidth;
                Location = location;
                Metrics = gm;
            }
        }

        FontFile _font;
        IRenderSurface _rt;
        int _fontSize;
        int _tabSize;
        int _pageSize;
        int _pointsPerCurve;
        int _charPadding;

        BinPacker _packer;
        GlyphCache[] _glyphCache;
        CharData[] _charData;
        SceneRenderData _renderData;
        IRenderer _renderer;
        ThreadedQueue<ushort> _pendingGlyphs;

        /// <summary>
        /// Creates a new instance of <see cref="SpriteFont2"/>.
        /// </summary>
        /// <param name="renderer">The renderer with which to prepare and update the sprite font's character sheet.</param>
        /// <param name="font">The font file from which to source character glyphs.</param>
        /// <param name="ptSize">The size of the characters ,in font points (e.g. 12pt, 16pt, 18pt, etc).</param>
        /// <param name="tabSize">The number of spaces which represent a single tab character (\t).</param>
        /// <param name="texturePageSize">The size (in pixels) of a single sprite font texture page.</param>
        /// <param name="pointsPerCurve">The number of points allowed per curve when generating glyph shapes. This can be used as a detail level for character glyphs. <para/>
        /// A higher number produces smoother curves, while a lower one will produce faceted, low-poly representations of curves. Setting this too low may produce invalid curves.</param>
        /// <param name="initialPages">The initial number of pages in the underlying sprite font texture atlas. Minimum is 1.</param>
        /// <param name="charPadding">The number of pixels to add as padding around each character placed on to the font atlas. 
        /// Default value is 2. Negative padding can cause characters to overlap.</param>
        public SpriteFont2(IRenderer renderer, FontFile font, int ptSize, int tabSize = 3, int texturePageSize = 512, int pointsPerCurve = 12, int initialPages = 1, int charPadding = 2)
        {
            Debug.Assert(texturePageSize >= MIN_PAGE_SIZE, $"Texture page size must be at least {MIN_PAGE_SIZE}");
            Debug.Assert(pointsPerCurve >= 2, $"Points per curve must be at least {MIN_POINTS_PER_CURVE}");
            Debug.Assert(initialPages >= 1, $"Initial pages must be at least 1");

            _renderer = renderer;
            _font = font;
            _glyphCache = new GlyphCache[_font.GlyphCount];
            _charData = new CharData[char.MaxValue];
            _tabSize = tabSize;
            _fontSize = ptSize;
            _pageSize = texturePageSize;
            _pointsPerCurve = pointsPerCurve;
            _packer = new BinPacker(_pageSize, _pageSize);
            _pendingGlyphs = new ThreadedQueue<ushort>();
            _charPadding = charPadding;

            _rt = renderer.Resources.CreateSurface(_pageSize, _pageSize, arraySize: initialPages);
            _rt.Clear(Color.Black);
            _renderData = renderer.CreateRenderData();
            _renderData.IsVisible = false;
            _renderData.Flags = SceneRenderFlags.TwoD | SceneRenderFlags.DoNotClear | SceneRenderFlags.NoDebugOverlay;
            _renderData.AddSprite(new FontContainer(this));
            _renderData.SpriteCamera = new Camera2D()
            {
                OutputSurface = _rt,
            };

            AddCharacter(' ');
            AddCharacter('\t');
        }

        private int ToPixels(float designUnits)
        {
            return (int)Math.Ceiling(_fontSize * designUnits / _font.Header.DesignUnitsPerEm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public int GetAdvanceWidth(char c)
        {
            if (!_charData[c].Initialized)
                AddCharacter(c);

            return _glyphCache[_charData[c].GlyphID].AdvanceWidth;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public GlyphCache GetChar(char c)
        {
            Rectangle rect = Rectangle.Empty;
            if (!_charData[c].Initialized)
                AddCharacter(c);

            return _glyphCache[_charData[c].GlyphID];
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Vector2F MeasureString(string text)
        {
            return MeasureString(text, text.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public Vector2F MeasureString(string text, int maxLength)
        {
            Vector2F result = new Vector2F();

            for(int i = 0; i < maxLength; i++)
            {
                GlyphCache gc = GetChar(text[i]);
                result.X += gc.AdvanceWidth;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public Rectangle MeasureString(string text, int startIndex, int length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the index of the nearest character within the specified string, based on the provided local point position.
        /// </summary>
        /// <param name="text">A string of text.</param>
        /// <param name="localPoint">The local point to test, relative to the string's screen or world position.</param>
        /// <returns></returns>
        public int NearestCharacter(string text, Vector2F localPoint)
        {
            throw new NotImplementedException();
        }

        private void AddCharacter(char c)
        {
            ushort gIndex = _font.GetGlyphIndex(c);

            // If the character uses an existing glyph, initialize the character and return.
            if(_glyphCache[gIndex] != null)
            {
                _charData[c] = new CharData(gIndex);
                return;
            }

            Glyph g = _font.GetGlyphByIndex(gIndex);
            GlyphMetrics gm = _font.GetMetricsByIndex(gIndex);

            Rectangle gBounds = g.Bounds;
            int padding2 = _charPadding * 2;
            int pWidth = ToPixels(gBounds.Width);
            int pHeight = ToPixels(gBounds.Height);
            Vector2F glyphScale = new Vector2F()
            {
                X = (float)pWidth / gBounds.Width,
                Y = (float)pHeight / gBounds.Height,
            };

            Rectangle loc = _packer.Insert(pWidth + padding2, pHeight + padding2);
            _charData[c] = new CharData(gIndex);
            _glyphCache[gIndex] = new GlyphCache(gm.AdvanceWidth, loc, gm);

            List<Shape> shapes = g.CreateShapes(_pointsPerCurve);
            for (int i = 0; i < shapes.Count; i++)
            {
                shapes[i].ScaleAndOffset(new Vector2F(loc.X + _charPadding, loc.Y + _charPadding), glyphScale);
                shapes[i].Triangulate(_glyphCache[gIndex].GlyphMesh, Vector2F.Zero, 1);
            }

            _pendingGlyphs.Enqueue(gIndex);
            _renderData.IsVisible = true;
        }

        /// <summary>
        /// A container sprite for drawing glyphs to the font texture.
        /// </summary>
        class FontContainer : ISprite
        {
            SpriteFont2 _font;

            public FontContainer(SpriteFont2 font) { _font = font; }

            public void Render(SpriteBatch sb)
            {
                while(_font._pendingGlyphs.TryDequeue(out ushort gIndex))
                {
                    GlyphCache cache = _font._glyphCache[gIndex];
                    sb.DrawTriangleList(cache.GlyphMesh, Color.White);
                }

                _font._renderData.IsVisible = false;
            }
        }

        public void Dispose()
        {
            _rt.Dispose();
            _renderer.DestroyRenderData(_renderData);
            _renderData.Dispose();
        }

        /// <summary>
        /// The font size, in font points (e.g. 12pt, 16pt, 18pt, etc).
        /// </summary>
        public int FontSize => _fontSize;

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
