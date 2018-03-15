using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>A loaded version of a font file.</summary>
    public class FontFile
    {
        FontInfo _info;
        FontTableList _tables;
        FontFlags _flags;
        Glyph[] _glyphs;
        GlyphMetrics[] _metrics;
        Rectangle _containerBounds;

        Cmap _cmap;
        Maxp _maxp;
        Hmtx _hmtx;
        Head _head;
        Prep _prep;
        Hhea _hhea;
        bool _flipY;

        internal FontFile(FontTableList tables, bool flipYAxis = false)
        {
            _tables = tables;
            _flags = FontFlags.None;
            _flipY = flipYAxis;
        }

        /// <summary>
        /// (Re)Builds the font based on it's set of available tables. <para/>
        /// You can drop any tables you do not want the font to be built with via <see cref="Tables"/>.<para/>
        /// If the font has been built previously, calling this again will cause the font to be rebuilt from the tables present in <see cref="Tables"/>.
        /// </summary>
        public void Build()
        {
            // If the flags are invalid, we cannot make a usable FontFile instance.
            _flags = FontValidator.Validate(_tables);
            if (_flags == FontFlags.Invalid)
            {
                _maxp = new Maxp();
                return;
            }

            if (_flipY)
                _flags |= FontFlags.InvertedYAxis;

            Name nameTable = _tables.Get<Name>();

            _info = new FontInfo(nameTable);
            _cmap = _tables.Get<Cmap>();
            _maxp = _tables.Get<Maxp>();
            _hmtx = _tables.Get<Hmtx>();
            _head = _tables.Get<Head>();
            _prep = _tables.Get<Prep>();
            _hhea = _tables.Get<Hhea>();

            if (_head != null)
            {
                _containerBounds = new Rectangle()
                {
                    Left = _head.MinX,
                    Right = _head.MaxX,
                    Top = _head.MinY,
                    Bottom = _head.MaxY,
                };
            }

            // Create a local copy of the glyph array from the glyf table.
            Glyf glyf = _tables.Get<Glyf>();
            if (glyf != null)
            {
                _glyphs = new Glyph[glyf.Glyphs.Length];
                Array.Copy(glyf.Glyphs, _glyphs, _glyphs.Length);
            }

            // Separate _glyphs null check since there are several TTF/OTF tables that can produce valid glyphs (only glyf supported currently).
            if (_glyphs != null)
            {
                if (_flipY)
                    FlipYAxis();

                _metrics = new GlyphMetrics[_glyphs.Length];
                for (int i = 0; i < _metrics.Length; i++)
                    _metrics[i] = new GlyphMetrics();

                // Populate horizontal metrics.
                Glyph g = null;
                if (_hmtx != null)
                {
                    for (int i = 0; i < _metrics.Length; i++)
                    {
                        g = _glyphs[i];
                        int lsb = _hmtx.GetLeftSideBearing(i);
                        int aw = _hmtx.GetAdvanceWidth(i);

                        _metrics[i].LeftSideBearing = lsb;
                        _metrics[i].AdvanceWidth = aw;
                        _metrics[i].RightSideBearing = aw - (lsb + g.MaxX - g.MinX); // MS Docs: the right side bearing ("rsb") is calculated as follows: rsb = aw - (lsb + xMax - xMin)
                    }
                }
                else // No hmtx table. Improvise using glyph bounds.
                {
                    for (int i = 0; i < _metrics.Length; i++)
                    {
                        g = _glyphs[i];
                        int lsb = 0;
                        int aw = g.MaxX - g.MinX;

                        _metrics[i].LeftSideBearing = lsb;
                        _metrics[i].AdvanceWidth = aw;
                        _metrics[i].RightSideBearing = aw - (lsb + g.MaxX - g.MinX); // MS Docs: the right side bearing ("rsb") is calculated as follows: rsb = aw - (lsb + xMax - xMin)
                    }
                }

                // TODO populate metrics with VMTX (vertical metrics) table data.
            }
        }

        private void FlipYAxis()
        {
            foreach (Glyph glyph in _glyphs)
            {
                Rectangle oldBounds = glyph.Bounds;
                int difTop = glyph.Bounds.Top - _containerBounds.Top;
                int difBottom = _containerBounds.Bottom - glyph.Bounds.Bottom;
                glyph.Bounds = new Rectangle()
                {
                    Left = oldBounds.Left,
                    Right = oldBounds.Right,
                    Top = _containerBounds.Top + difBottom,
                    Bottom = _containerBounds.Bottom - difTop,
                };

                for (int i = 0; i < glyph.Points.Length; i++)
                    glyph.Points[i].Y = glyph.MaxY - (glyph.Points[i].Y - oldBounds.Top);
            }
        }

        /// <summary>
        /// Retrieves a glyph for the specified character, or returns the default one if the character is not part of the font.
        /// </summary>
        /// <param name="character">The character for which to retrieve a glyph.</param>
        /// <returns></returns>
        public Glyph GetGlyph(char character)
        {
            int index = _cmap.LookupIndex(character);
            return _glyphs[index];
        }

        /// <summary>
        /// Returns the index of the glyph for the specified character. If there is no glyph available, the default one (index 0) will be returned.
        /// </summary>
        /// <param name="character">The character for which to retrieve a glyph index.</param>
        /// <returns></returns>
        public ushort GetGlyphIndex(char character)
        {
            return _cmap.LookupIndex(character);
        }

        /// <summary>
        /// Gets a glyph by it's index, as defined by the font, which is not to be confused with a character code/ID.
        /// </summary>
        /// <param name="index">The glyph index.</param>
        /// <returns></returns>
        public Glyph GetGlyphByIndex(int index)
        {
            return _glyphs[index];
        }

        /// <summary>Returns metrics for the specified character, or for the default character if the font does not contain it.</summary>
        /// <param name="character">The character.</param>
        /// <returns></returns>
        public GlyphMetrics GetMetrics(char character)
        {
            int index = _cmap.LookupIndex(character);
            return _metrics[index];
        }

        /// <summary>Returns metrics for the specified glyph, or for the default glyph if the font does not contain it.</summary>
        /// <param name="character">The glyph index.</param>
        /// <returns></returns>
        public GlyphMetrics GetMetricsByIndex(int index)
        {
            return _metrics[index];
        }

        /// <summary>
        /// Calculates the scale to the target pixel size based on the current <see cref="FontFile"/> UnitsPerEm
        /// </summary>
        /// <param name="targetPixelSize">target font size in point unit</param>
        /// <returns></returns>
        public float CalculateScaleToPixel(float targetPixelSize)
        {
            return targetPixelSize / _head.DesignUnitsPerEm;
        }

        /// <summary>Returns true if the current <see cref="Flags"/> contains the specified flag value.</summary>
        /// <param name="flag">The flags to test.</param>
        /// <returns>A boolean value.</returns>
        public bool HasFlag(FontFlags flag)
        {
            return (_flags & flag) == flag;
        }

        /// <summary>Gets the font's table list, which can be used to access any loaded tables, or the headers of tables that were not supported.<para/>
        /// You can also use the table list to drop loaded tables. This may be useful if you wish to reduce the font's footprint after it has been built.</summary>
        public FontTableList Tables => _tables;

        /// <summary>
        /// Gets a flags value containing information about the current <see cref="FontFile"/>.
        /// </summary>
        public FontFlags Flags => _flags;

        /// <summary>
        /// Gets the <see cref="FontInfo"/> bound to the current <see cref="FontFile"/>. Contains basic details such as the font name, description, copyright and designer name.
        /// </summary>
        public FontInfo Info => _info;

        /// <summary>
        /// Gets the number of glyphs in the font.
        /// </summary>
        public int GlyphCount => _glyphs.Length;

        /// <summary>
        /// Gets the maximum profile information for the current <see cref="FontFile"/>.<para/>
        /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/maxp
        /// </summary>
        public Maxp MaximumProfile => _maxp;

        /// <summary>
        /// Gets the <see cref="FontFile"/> header table which contains metric information about the font's overall structure, such as the units-per-em.
        /// </summary>
        public Head Header => _head;

        /// <summary>
        /// Gets the font's horizontal header (hhea) information.
        /// </summary>
        public Hhea HorizonalHeader => _hhea;

        /// <summary>
        /// [Internal] Gets the control value program, if present.
        /// </summary>
        internal Prep ControlValueProgram => _prep;

        /// <summary>
        /// Gets the bounds in which all glyph's individual bounds are restricted to.
        /// </summary>
        public Rectangle ContainerBounds => _containerBounds;
    }
}
