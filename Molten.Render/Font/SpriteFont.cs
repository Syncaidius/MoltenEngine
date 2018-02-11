using Molten.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Typography.Contours;
using Typography.OpenFont;
using Typography.Rendering;

namespace Molten.Graphics
{
    public class SpriteFont
    {
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
        public SpriteFont(IRenderer renderer, Stream stream, int sheetPageSize = 512, int fontSpriteSize = 18)
        {
            _sheetPageSize = sheetPageSize;
            _fontSpriteSize = fontSpriteSize;
            _renderer = renderer;

            OpenFontReader reader = new OpenFontReader();
            _face = reader.Read(stream);
            _packer = new BinPacker(sheetPageSize, sheetPageSize);
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
            float scale = 1f / 64;
            msdfGenParams.shapeScale = scale;
            float s_xmin = bounds.XMin * scale;
            float s_xmax = bounds.XMax * scale;
            float s_ymin = bounds.YMin * scale;
            float s_ymax = bounds.YMax * scale;

            // TODO replace Bounds type with Rectangle
            Rectangle rect = new Rectangle()
            {
                X = bounds.XMin,
                Y = bounds.YMin,
                Width = bounds.XMax - bounds.XMin,
                Height = bounds.YMax - bounds.YMin,
            };

            GlyphData gData = MsdfGlyphGen.CreateMsdfImage(_renderer, glyphToContour, msdfGenParams);
            //atlasBuilder.AddGlyph(codePoint, glyphImg);

            CacheGlyphData g = new CacheGlyphData()
            {
                codePoint = codePoint,
                img = gData,
            };
            BinPackRect newRect = _packer.Insert(g.img.Width, g.img.Height);
            g.area = new Typography.Contours.Rectangle(newRect.X, newRect.Y,
                g.img.Width, g.img.Height);

            //_sheet.SetData()
            return rect;
        }

        /// <summary>Gets the underlying <see cref="ITexture2D"/> which contains the font sheet.</summary>
        public ITexture2D SheetTexture => _sheet;
    }
}
