//MIT - 2018 - James Yarwood - Modified for Molten Engine - https://github.com/Syncaidius/MoltenEngine
//MIT, 2015, Michael Popoloski's SharpFont
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    internal class TrueTypeInterpreter
    {
        FontFile _curFont;
        SharpFontInterpreter _interpreter;
        Prep _prep;
        Cvt _controlValTable;
        Fpgm _fpgm;

        public void SetTypeFace(FontFile font)
        {
            if (!font.HasFlag(FontFlags.Valid))
                throw new FontInterpreterException("The provided font is invalid. Ensure the font was loaded correctly.");

            // Collect everything we need to work with the font.
            _curFont = font;
            Maxp maximumProfile = _curFont.MaximumProfile;
            _prep = _curFont.Tables.Get<Prep>();
            _controlValTable = _curFont.Tables.Get<Cvt>();
            _fpgm = _curFont.Tables.Get<Fpgm>();

            _interpreter = new SharpFontInterpreter(
                maximumProfile.MaxStackElements,
                maximumProfile.MaxStorage,
                maximumProfile.MaxFunctionDefs,
                maximumProfile.MaxInstructionDefs,
                maximumProfile.MaxTwilightPoints);

            // The fpgm table optionally contains a program to run at initialization time 
            if (_fpgm != null)
                _interpreter.InitializeFunctionDefs(_fpgm.ByteCode);
        }

        public GlyphPoint[] HintGlyph(ushort glyphIndex, float glyphSizeInPixel)
        {

            Glyph glyph = _curFont.GetGlyphByIndex(glyphIndex);
            int horizontalAdv = _curFont.GetHAdvanceWidthFromGlyphIndex(glyphIndex);
            int hFrontSideBearing = _curFont.GetHFrontSideBearingFromGlyphIndex(glyphIndex);

            return HintGlyph(horizontalAdv,
                hFrontSideBearing,
                glyph.MinX,
                glyph.MaxY,
                glyph.Points,
                glyph.ContourEndPoints,
                glyph.Instructions,
                glyphSizeInPixel);
        }

        public GlyphPoint[] HintGlyph(
            int horizontalAdv,
            int hFrontSideBearing,
            int minX,
            int maxY,
            GlyphPoint[] glyphPoints,
            ushort[] contourEndPoints,
            byte[] instructions,
            float glyphSizeInPixel)
        {
            // TODO Get glyph for its matrix

            int verticalAdv = 0;
            int vFrontSideBearing = 0;
            var pp1 = new GlyphPoint((minX - hFrontSideBearing), 0, true);
            var pp2 = new GlyphPoint(pp1.X + horizontalAdv, 0, true);
            var pp3 = new GlyphPoint(0, maxY + vFrontSideBearing, true);
            var pp4 = new GlyphPoint(0, pp3.Y - verticalAdv, true);

            //2. use a clone version extend org with 4 elems
            int orgLen = glyphPoints.Length;
            GlyphPoint[] newGlyphPoints = FontUtil.CloneAndExtendArray(glyphPoints, 4);

            // Add phantom points. 
            // These are used to define the extents of the glyph and can be modified by hinting instructions.
            newGlyphPoints[orgLen] = pp1;
            newGlyphPoints[orgLen + 1] = pp2;
            newGlyphPoints[orgLen + 2] = pp3;
            newGlyphPoints[orgLen + 3] = pp4;

            // Scale all point to target pixel size
            float pxScale = _curFont.CalculateScaleToPixel(glyphSizeInPixel);
            for (int i = orgLen + 3; i >= 0; --i)
                newGlyphPoints[i].ApplyScale(pxScale);

            // Test : agg's vertical hint
            // Apply large scale on horizontal axis only 
            // Translate and then scale back
            float agg_x_scale = 1000;
            if (UseVerticalHinting)
                ApplyScaleOnlyOnXAxis(newGlyphPoints, agg_x_scale);


            _interpreter.SetControlValueTable(_controlValTable.Values,
                pxScale,
                glyphSizeInPixel,
                _prep.Instructions);


            _interpreter.HintGlyph(newGlyphPoints, contourEndPoints, instructions);

            // Scale back.
            if (UseVerticalHinting)
                ApplyScaleOnlyOnXAxis(newGlyphPoints, 1f / agg_x_scale);

            return newGlyphPoints;
        }

        public bool UseVerticalHinting { get; set; }

        static void ApplyScaleOnlyOnXAxis(GlyphPoint[] glyphPoints, float xscale)
        {
            //TODO: review performance here
            for (int i = glyphPoints.Length - 1; i >= 0; --i)
                glyphPoints[i].ApplyScaleOnlyOnXAxis(xscale);
        }
    }
}