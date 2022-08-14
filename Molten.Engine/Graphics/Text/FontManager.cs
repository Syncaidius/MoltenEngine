using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Collections;
using Molten.Font;
using Molten.Graphics.SDF;
using Silk.NET.Core.Native;

namespace Molten.Graphics
{
    public class FontManager
    {
        public const char PLACEHOLDER_CHAR = ' ';

        Engine _engine;
        Dictionary<string, FontBinding> _fonts;
        SceneRenderData _renderData;
        RenderCamera _camera;

        IRenderSurface2D _rtTransfer;
        ThreadedQueue<FontGlyphBinding> _pendingGlyphs;
        List<FontPage> _pages;
        Interlocker _fontLocker;
        Interlocker _pageLocker;
        SdfGenerator _sdf;

        internal FontManager(Logger log, Engine engine)
        {
            _engine = engine;
            _fontLocker = new Interlocker();
            _pageLocker = new Interlocker();
            _fonts = new Dictionary<string, FontBinding>();
            _pendingGlyphs = new ThreadedQueue<FontGlyphBinding>();
            _pages = new List<FontPage>();
            _sdf = new SdfGenerator();

            Log = log;
        }

        internal void Initialize()
        {
            _renderData = _engine.Renderer.CreateRenderData();
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
            _camera = new RenderCamera(RenderCameraMode.Orthographic)
            {
                Surface = _engine.Renderer.Resources.CreateSurface((uint)PageSize, (uint)PageSize, arraySize: 1, flags: TextureFlags.AllowMipMapGeneration),
                Flags = RenderCameraFlags.DoNotClear
            };

            _camera.Surface.Clear(Color.Transparent);
            _renderData.AddObject(_camera);
        }

        private void OnDraw(SpriteBatcher sb)
        {
            RectStyle style = RectStyle.Default;

            if(_pages.Count > _camera.Surface.ArraySize)
            {
                uint newArraySize = (uint)_pages.Count;
                _rtTransfer = _camera.Surface;
                _camera.Surface = _engine.Renderer.Resources.CreateSurface(
                    (uint)PageSize,
                    (uint)PageSize,
                    arraySize: newArraySize,
                    flags: TextureFlags.AllowMipMapGeneration);
                _camera.Surface.Clear(Color.Transparent);

                _camera.Surface = _camera.Surface;

                Rectangle rtBounds = new Rectangle(0, 0, PageSize, PageSize);
                for(uint i = 0; i < _rtTransfer.ArraySize; i++)
                    sb.Draw(rtBounds, rtBounds, Color.White, _rtTransfer, null, i, i);

                _rtTransfer.Dispose();
                _rtTransfer = null;
                return;
            }

            while (_pendingGlyphs.TryDequeue(out FontGlyphBinding binding))
            {
                Rectangle gBounds = binding.Glyph.Bounds;
                Vector2F glyphScale = new Vector2F()
                {
                    X = (float)binding.PWidth / gBounds.Width,
                    Y = (float)binding.PHeight / gBounds.Height,
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

                ITexture2D tex = _sdf.ConvertToTexture(_engine.Renderer, sdfRef);
                sb.Draw(binding.Location, ref style, tex, null, 0, (uint)binding.Page);

                sdfRef.Slice.Dispose();
               // tex.Dispose(); -- TODO implement proper GPU disposal handling (only disposes gpu resources after X frames)
            }

            _renderData.IsVisible = false;
        }

        private void _renderData_OnPostRender(RenderService renderer, SceneRenderData data)
        {
            _camera.Surface.GenerateMipMaps();
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

            FontGlyphBinding glyphBinding = new FontGlyphBinding(binding)
            {
                Glyph = glyph,
                AdvanceWidth = DesignToPixels(binding.File, gm.AdvanceWidth),
                AdvanceHeight = DesignToPixels(binding.File, binding.File.Header.MaxY),
                PWidth = DesignToPixels(binding.File, gBounds.Width),
                PHeight = DesignToPixels(binding.File, gBounds.Height),
                YOffset = DesignToPixels(binding.File, gBounds.Top),
            };

            binding.Glyphs[gIndex] = glyphBinding;

            if (render)
            {
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
        public ITexture2D UnderlyingTexture => _camera.Surface;
    }
}
