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

        class GlyphCache
        {
            // TODO store kerning/GPOS/GSUB offsets in pairs, on-demand.  

            public Rectangle Location;

            // The shapes which make up the character glyph. Required for rendering to sheet or generating a 3D model.
            public List<Vector2F> GlyphMesh = new List<Vector2F>();

            public GlyphMetrics Metrics;

            public int AdvanceWidth;
        }

        FontFile _font;
        IRenderSurface _rt;
        int _fontSize;
        int _tabSize;
        int _pageSize;
        int _pointsPerCurve;
        float _glyphScale;

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
        public SpriteFont2(IRenderer renderer, FontFile font, int ptSize, int tabSize = 3, int texturePageSize = 1024, int pointsPerCurve = 12, int initialPages = 1)
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

            _rt = renderer.Resources.CreateSurface(_pageSize, _pageSize, arraySize: initialPages);
            _rt.Clear(Color.Black);
            _renderData = renderer.CreateRenderData();
            _renderData.IsVisible = false;
            _renderData.Flags = SceneRenderFlags.TwoD | SceneRenderFlags.DoNotClear;
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
        public Rectangle GetCharRect(char c)
        {
            Rectangle rect = Rectangle.Empty;
            if (!_charData[c].Initialized)
                AddCharacter(c);

            return _glyphCache[_charData[c].GlyphID].Location;
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
                char c = text[i];
                Rectangle getRect = GetCharRect(c);
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
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="localPosition"></param>
        /// <returns></returns>
        public int NearestCharacter(string text, Vector2F localPosition)
        {
            throw new NotImplementedException();
        }

        private void AddCharacter(char c)
        {
            ushort gIndex = _font.GetGlyphIndex(c);
            Glyph g = _font.GetGlyphByIndex(gIndex);
            GlyphMetrics gm = _font.GetMetricsByIndex(gIndex);

            int pWidth = ToPixels(g.Bounds.Width);
            int pHeight = ToPixels(g.Bounds.Height);
            Rectangle loc = _packer.Insert(pWidth, pHeight);

            _charData[c] = new CharData(gIndex);
            _glyphCache[gIndex] = new GlyphCache()
            {
                AdvanceWidth = gm.AdvanceWidth,
                Location = loc,
                Metrics = gm,
            };

            List<Shape> shapes = g.CreateShapes(_pointsPerCurve);
            for(int i = 0; i < shapes.Count; i++)
                shapes[i].Triangulate(_glyphCache[gIndex].GlyphMesh, Vector2F.Zero, 1);

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
