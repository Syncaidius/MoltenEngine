using Molten.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Typography.Contours;
using Typography.OpenFont;
using Typography.Rendering;

namespace Molten.Graphics
{
    public class SpriteFont2
    {
        const int DATA_INCREMENT = 10;
        const char DEFAULT_CHAR = '?';

        ITexture2D _sheet;
        Typeface _face;
        IRenderer _renderer;

        Rectangle[] _charData;
        ushort[] _ids;
        bool[] _lookup;
        ushort _nextID;

        int _fontSpriteSize;
        int _sheetPageSize;
        BinPacker _packer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="stream"></param>
        /// <param name="sheetPageSize">The size of a single font sheet page, in pixels. <see cref="SpriteFont"/> uses texture arrays to provide huge amounts of capacity for any amount of characters.</param>
        /// <param name="fontSpriteSize">The font size used to render characters on to the sprite sheet. Larger font sizes take up more space on a sheet page.</param>
        public SpriteFont2(IRenderer renderer, Stream stream, int sheetPageSize = 512, int fontSpriteSize = 18)
        {
            _sheetPageSize = sheetPageSize;
            _fontSpriteSize = fontSpriteSize;
            _renderer = renderer;

            _charData = new Rectangle[10];
            _ids = new ushort[10];
            _lookup = new bool[10];

            OpenFontReader reader = new OpenFontReader();
            _face = reader.Read(stream);
            _packer = new BinPacker(sheetPageSize, sheetPageSize);
            _sheet = renderer.Resources.CreateTexture2D(new Texture2DProperties()
            {
                Width = sheetPageSize,
                Height = sheetPageSize,
                ArraySize = 1,
                MipMapLevels = 1,
                Format = GraphicsFormat.R8G8B8A8_UNorm,
                Flags = TextureFlags.None,
            });

        }

        public Rectangle GetCharRect(char c)
        {
            //if the character does not exist, attempt add it.
            if (c >= _ids.Length || _lookup[c] == false)
                return AddCharacter(c);

            return _charData[_ids[c]];
        }

        private Rectangle AddCharacter(char c)
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

            // TODO do MSDF stuff here, call _sheet.SetData().
            // TODO return char's rectangle.
            // TODO texture will automatically be updated next time it is used on the GPU.

            MsdfGenParams msdfGenParams = new MsdfGenParams();
            var builder = new GlyphPathBuilder(_face);

            //build glyph
            ushort codePoint = _face.LookupIndex(c); // Glyph index.
            //get exact bounds of glyphs
            Glyph glyph = _face.GetGlyphByIndex(codePoint);
            Bounds bounds = glyph.Bounds;  //exact bounds

            // Build shape from contours
            builder.BuildFromGlyphIndex(codePoint, -1);
            var glyphToContour = new GlyphContourBuilder();
            builder.ReadShapes(glyphToContour);
            float scale = 1f / 32;
            msdfGenParams.shapeScale = scale;

            GlyphData gData = MsdfGlyphGen.CreateMsdfImage(_renderer, glyphToContour, msdfGenParams);
            CacheGlyphData g = new CacheGlyphData()
            {
                codePoint = codePoint,
                img = gData,
            };

            BinPackRect newRect = _packer.Insert(g.img.Width, g.img.Height);
            g.area = new Typography.Contours.Rectangle(newRect.X, newRect.Y,
                g.img.Width, g.img.Height);

            Rectangle rect = new Rectangle()
            {
                X = newRect.X,
                Y = newRect.Y,
                Width = g.img.Width,
                Height = g.img.Height,
            };

            ushort id = _nextID;
            _ids[c] = id;
            _lookup[c] = true;
            _nextID++;

            //store rectangle
            _charData[id] = rect;

            int bpp = Marshal.SizeOf<Color>();
            _sheet.SetData(rect, g.img.GetImageBuffer(), bpp, 0);
            return rect;
        }

        /// <summary>Gets the underlying <see cref="ITexture2D"/> which contains the font sheet.</summary>
        public ITexture2D SheetTexture => _sheet;
    }
}
