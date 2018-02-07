using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public sealed class SpriteFont : EngineObject, ISpriteFont
    {
        struct ExpectedState
        {
            public int Size;
        }

        const int TAB_SPACING = 3; // The size of a tab character represented as spaces. A spacing of 3 will equal the size of 3 space characters.
        const int DATA_INCREMENT = 10;

        D2DSurface _surface;

        static FontCollection _collection;
        static int _staticReferences;

        int _size;
        FontFamily _family;
        Font _font;
        FontFace _face;
        FontMetrics _metrics;
        TextFormat _format;

        Rectangle[] _charData;
        ushort[] _ids;
        bool[] _lookup;
        ushort _nextID;

        int _tileHeight;
        int _nextX;
        int _nextY;

        GraphicsDevice _device;
        D2DSolidBrush _debugBrush;

        ExpectedState _expected;
        ThreadedList<char> _pendingChars;
        int _pendingResize;

        internal SpriteFont(GraphicsDevice device, string fontName,
            FontWeight weight = FontWeight.Regular,
            FontStretch stretch = FontStretch.Normal,
            FontStyle style = FontStyle.Normal, int size = 12, int sheetSize = 512)
        {
            if (sheetSize < 1)
                throw new ArgumentException("Sheet size cannot be less than 1");

            _staticReferences++;
            _device = device;
            _expected = new ExpectedState()
            {
                Size = sheetSize,
            };

            _pendingChars = new ThreadedList<char>();

            if (_collection == null || _collection.IsDisposed)
                _collection = device.DirectWrite.GetSystemFontCollection(false);

            int index = 0;
            if (_collection.FindFamilyName(fontName, out index) == false)
                index = 0;

            //retrieve the actual font
            _size = size;
            _family = _collection.GetFontFamily(index);
            _font = _family.GetFirstMatchingFont(weight, stretch, style);
            _face = new FontFace(_font);
            _format = new TextFormat(device.DirectWrite, fontName, weight, style, stretch, size);
            _metrics = _font.Metrics;

            _surface = new D2DSurface(_device, sheetSize, sheetSize);
            _surface.TextAntiAliasMode = SharpDX.Direct2D1.TextAntialiasMode.Grayscale;
            _surface.OnRefreshed += _d2d_OnRefreshed;

            _charData = new Rectangle[10];
            _ids = new ushort[10];
            _lookup = new bool[10];

            _tileHeight = _size + ToPixels(_metrics.Descent);

            _debugBrush = new D2DSolidBrush(_surface);
            _debugBrush.Color = new Color(255, 255, 0, 255);

            // Add space and tab characters.
            AddCharacter(' ');
            Rectangle spaceRect = GetCharRect(' ');
            Rectangle tabRect = new Rectangle()
            {
                X = (int)_nextX,
                Y = (int)_nextY,
                Width = TAB_SPACING * spaceRect.Width,
                Height = _tileHeight,
            };
            AddCharacter('\t', ref tabRect);
        }

        private void _d2d_OnRefreshed(GraphicsPipe pipe, D2DSurface surface)
        {
            ApplyChanges(pipe);
        }

        /// <summary>Expands the sheet to twice its width and height.</summary>
        /// <param name="isRecursion">True if the call is a recursion.</param>
        private void Expand()
        {
            _expected.Size *= 2;

            // Now rebuild the entire character map
            _nextX = 0;
            _nextY = 0;

            int[] codePoints = new int[1];
            short[] glyphIndices;
            GlyphMetrics[] glyphMetrics;

            for (char c = (char)0; c < _lookup.Length; c++)
            {
                //if the character was never added, skip it
                if (_lookup[c] == false)
                    continue;

                ushort id = _ids[c];

                codePoints[0] = c;
                glyphIndices = _face.GetGlyphIndices(codePoints);
                glyphMetrics = _face.GetDesignGlyphMetrics(glyphIndices, false);
                GlyphMetrics metric = glyphMetrics[0];

                int actualWidth = ToPixels(metric.AdvanceWidth - metric.RightSideBearing);
                int charWidth = actualWidth != 0 ? actualWidth : ToPixels(metric.AdvanceWidth);

                //build character draw/sheet rectangle
                Rectangle rect = new Rectangle()
                {
                    X = (int)_nextX,
                    Y = (int)_nextY,
                    Width = charWidth,
                    Height = _tileHeight,
                };

                //store rectangle
                _charData[id] = rect;

                // Validate the new space of the character.
                ValidateSpace(rect);
            }
        }

        private void AddCharacter(char c, ref Rectangle rect)
        {
            //expand char data array if needed.
            if (_nextID == _charData.Length)
                Array.Resize(ref _charData, _charData.Length + DATA_INCREMENT);

            //expand the ID array to fit the highest value character.
            if (c >= _ids.Length)
            {
                Array.Resize(ref _ids, c + 1);
                Array.Resize(ref _lookup, c + 1);
            }

            ushort id = _nextID;
            _ids[c] = id;
            _lookup[c] = true;
            _nextID++;

            //store rectangle
            _charData[id] = rect;

            int pendResize = ValidateSpace(rect);

            // Draw new character, but only if the sheet hasn't resized.
            if (_pendingChars.Contains(c) == false)
                _pendingChars.Add(c);

            // Set pending resize flag.
            Interlocked.CompareExchange(ref _pendingResize, pendResize, 0);
        }

        /// <summary>Checks whether or not there is space to fit the provided rectangle.</summary>
        /// <param name="rect">The rectangle.</param>
        /// <returns>A value indicating whether the font surface needs resizing. 1 is true. 0 is false.</returns>
        private int ValidateSpace(Rectangle rect)
        {
            int result = 0;

            // Ensure the character can fit on the sheet.
            int reqX = _nextX + rect.Width;
            if (reqX >= _expected.Size)
            {
                _nextX = 0;
                int reqY = _nextY + _tileHeight;

                //check if the sheet needs to expand.
                if (reqY >= _expected.Size)
                {
                    result = 1;
                    Expand();
                }
                else
                {
                    _nextY += _tileHeight;
                }
            }

            // Move to next empty space.
            _nextX += rect.Width;
            return result;
        }

        private void AddCharacter(char c)
        {
            int[] codePoints = new int[1];
            short[] glyphIndices;
            GlyphMetrics[] glyphMetrics;

            //NOTE: helpful measurement image: http://www.freetype.org/freetype2/docs/glyphs/Image3.png

            codePoints[0] = c;
            glyphIndices = _face.GetGlyphIndices(codePoints);
            glyphMetrics = _face.GetDesignGlyphMetrics(glyphIndices, false);
            GlyphMetrics metric = glyphMetrics[0];
            

            /* NOTES:
             * TopSideBearing = space between top of the character and the top of the layout box
             * XHeight = baseline to top of lower-case X.
             * LeftSideBearing = space between the left side of the layout box and the left side of the character
             * AdvanceWidth = width of character + RightSideBearing
             * RightSideBearing = space between right side of character and right side of layout box
             * AdvanceHeight = height of character + TopSideBearing
             * BottomSideBearing = space between bottom of character and bottom of layout box.
             */

            int actualWidth = ToPixels(metric.AdvanceWidth - metric.RightSideBearing);
            int charWidth = actualWidth != 0 ? actualWidth : ToPixels(metric.AdvanceWidth);

            //build character draw/sheet rectangle
            Rectangle rect = new Rectangle()
            {
                X = (int)_nextX,
                Y = (int)_nextY,
                Width = charWidth,
                Height = _tileHeight,
            };

            AddCharacter(c, ref rect);
        }

        private void ApplyChanges(GraphicsPipe pipe)
        {
            if (1 == Interlocked.CompareExchange(ref _pendingResize, 0, 0))
            {
                _surface.SetSize(_expected.Size, _expected.Size, 1 ,1);
                Interlocked.Exchange(ref _pendingResize, 0);

                // Redraw all existing characters + pending
                _surface.BeginDraw();
                for (char c = (char)0; c < _lookup.Length; c++)
                {
                    //if the character was never added, skip it
                    if (_lookup[c] == false)
                        continue;

                    ushort cID = _ids[c];
                    _surface.CurrentBrush = null;
                    _surface.DrawText(c.ToString(), _format, _charData[cID], DrawTextOptions.Clip);
                }
                _surface.EndDraw();

                // Clear pending list. Automatically processed via redraw.
                _pendingChars.Clear();
            }
            else
            {
                // Draw all pending chars.
                if (_pendingChars.Count > 0)
                {
                    _surface.BeginDraw();
                    while (_pendingChars.TryTake(out char c))
                    {
                        ushort cID = _ids[c];
                        _surface.CurrentBrush = null;
                        _surface.DrawText(c.ToString(), _format, _charData[cID], DrawTextOptions.Clip);
                    }
                    _surface.EndDraw();

                    // Clear pending list. Automatically processed via redraw.
                    _pendingChars.Clear();
                }
            }
        }

        public Rectangle GetCharRect(char c)
        {
            //if the character does not exist, attempt add it.
            if (c >= _ids.Length || _lookup[c] == false)
                AddCharacter(c);

            return _charData[_ids[c]];
        }

        /// <summary>Returns the size of a string of text, in pixels, if it was drawn with this font. 
        /// <remarks>Does not currently support line breaks.</remarks></summary>
        /// <param name="text">The string of text to measure.</param>
        /// <returns></returns>
        public Vector2 MeasureString(string text)
        {
            if (text == null)
                return new Vector2();
            else
                return MeasureString(text, text.Length);
        }


        /// <summary>Returns the size of a string of text, in pixels, if it was drawn with this font. 
        /// <remarks>Does not currently support line breaks.</remarks></summary>
        /// <param name="text">The string of text to measure.</param>
        /// <param name="maxLength">The maximum number of characters to measure.</param>
        /// <returns></returns>
        public Vector2 MeasureString(string text, int maxLength)
        {
            Vector2 size = new Vector2(0, _tileHeight);
            int limit = Math.Min(maxLength, text.Length);

            for (int i = 0; i < limit; i++)
            {
                char c = text[i];

                //if the character does not exist, attempt add it.
                if (c >= _ids.Length || _lookup[c] == false)
                    AddCharacter(c);

                ushort id = _ids[c];
                size.X += _charData[id].Width;
            }

            return size;
        }

        /// <summary>Returns the size of a string of text, in pixels, if it was drawn with this font. 
        /// <remarks>Does not currently support line breaks.</remarks></summary>
        /// <param name="text">The string of text to measure.</param>
        /// <param name="maxLength">The maximum number of characters to measure.</param>
        /// <returns></returns>
        public Rectangle MeasureString(string text, int startIndex, int length)
        {
            Rectangle result = new Rectangle(0, 0, 0, _tileHeight);

            float size = 0;
            int start = Math.Max(0, startIndex);
            int end = Math.Min(start + length, text.Length);

            for (int i = 0; i < end; i++)
            {
                char c = text[i];

                // If the character does not exist, attempt add it.
                if (c >= _ids.Length || _lookup[c] == false)
                    AddCharacter(c);

                if (i == start)
                    result.X = (int)size;

                ushort id = _ids[c];
                size += _charData[id].Width;
            }

            result.Right = (int)size;

            return result;
        }

        /// <summary>Returns the index of the character which is closest to the provided local position/offset. Also returns a value equal to the length if the end
        /// of the string was closer.</summary>
        /// <param name="text">The string to text to test against.</param>
        /// <param name="localPosition">The local position, relative to the string's position.</param>
        /// <returns></returns>
        public int NearestCharacter(string text, Vector2 localPosition)
        {
            Vector2 size = new Vector2(0, _tileHeight);
            float lowestDist = float.MaxValue;
            float dist = float.MaxValue;
            int result = 0;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                //if the character does not exist, attempt add it.
                if (c >= _ids.Length || _lookup[c] == false)
                    AddCharacter(c);

                ushort id = _ids[c];

                dist = Vector2.Distance(localPosition, size);
                if (dist < lowestDist)
                {
                    lowestDist = dist;
                    result = i;
                }

                size.X += _charData[id].Width;
            }

            // Test end of string
            dist = Vector2.Distance(localPosition, size);
            if (dist < lowestDist)
                result = text.Length;

            return result;
        }

        private int ToPixels(float designUnits)
        {
            return (int)Math.Ceiling(_size * designUnits / _metrics.DesignUnitsPerEm);
        }

        protected override void OnDispose()
        {
            _staticReferences--;

            if (_staticReferences == 0)
                _collection.Dispose();
        }

        /// <summary>Gets the underlying font atlas texture.</summary>
        public ITexture2D UnderlyingTexture => _surface;

        /// <summary>Gets the name of the font that the sprite font uses.</summary>
        public string FontName => _family.FamilyNames.GetString(0);

        /// <summary>Gets the font size.</summary>
        public int FontSize => _size;

        internal TextFormat Format => _format;
    }
}
