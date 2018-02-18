using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Glyf data table (glyf).<para/>
    /// This table contains information that describes the glyphs in the font in the TrueType outline format. 
    /// Information regarding the rasterizer (scaler) refers to the TrueType rasterizer. <para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/glyf </summary>
    public class Glyf : FontTable
    {        
        public Glyph[] Glyphs { get; internal set; }

        internal class Parser : FontTableParser
        {
            static string[] _dependencies = new string[] { "loca", "maxp" };

            public override string TableTag => "glyf";

            public override string[] Dependencies => _dependencies;

            internal override FontTable Parse(BinaryEndianAgnosticReader reader, TableHeader header, Logger log, FontTableList dependencies)
            {
                long startOffset = reader.Position;
                Loca loca = dependencies.Get<Loca>();
                Maxp maxp = dependencies.Get<Maxp>();

                /* https://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6glyf.html
                 * Glyph descrion:
                 * Type 	Name 	Description
                 * SHORT 	numberOfContours 	If the number of contours is >= zero, this is a single glyph. If it's negative, this is a composite glyph.
                 * SHORT 	xMin 	Minimum x for coordinate data.
                 * SHORT 	yMin 	Minimum y for coordinate data.
                 * SHORT 	xMax 	Maximum x for coordinate data.
                 * SHORT 	yMax 	Maximum y for coordinate data.
                 */

                ushort numGlyphs = maxp.NumGlyphs;
                Glyph[] glyphs = new Glyph[numGlyphs];
                List<ushort> compositeGlyphs = new List<ushort>();

                for (ushort i = 0; i < numGlyphs; i++)
                {
                    uint offset = loca.Offsets[i];
                    uint length = loca.Offsets[i + 1] - offset;

                    if (length == 0)
                    {
                        if (i == 0)
                            glyphs[i] = Glyph.Empty;
                        else
                            glyphs[i] = glyphs[0]; // Use glyph 0 if the current glyph has no data.
                    }
                    else
                    {
                        reader.Position = startOffset + offset;

                        // If the number of contours is >= zero, this is a single glyph. If it's negative, this is a composite glyph.
                        short numContours = reader.ReadInt16();
                        Rectangle bounds = ReadBounds(reader);

                        if (numContours >= 0)
                            glyphs[i] = ReadSimpleGlyph(reader, numContours, bounds);
                        else
                            compositeGlyphs.Add(i);
                    }
                }

                // TODO: Resolve composite glyphs here.

                return new Glyf()
                {
                    Glyphs = glyphs,
                };
            }

            private Glyph ReadSimpleGlyph(BinaryEndianAgnosticReader reader, short numContours, Rectangle bounds)
            {
                ushort[] contourEndPoints = new ushort[numContours];
                for (int c = 0; c < numContours; c++)
                    contourEndPoints[c] = reader.ReadUInt16();

                ushort instructionLength = reader.ReadUInt16();
                byte[] instructions = instructionLength > 0? reader.ReadBytes(instructionLength) : new byte[0];

                // TODO: revisit this and attempt to reduce garbage (3 arrays get dumped).
                ushort pointCount = (ushort)(contourEndPoints[numContours - 1] + 1);
                SimpleGlyphFlags[] flags = ReadFlags(reader, pointCount);
                short[] xCoords = ReadCoordinates(reader, pointCount, flags, SimpleGlyphFlags.XShortVector, SimpleGlyphFlags.XSameOrPositive);
                short[] yCoords = ReadCoordinates(reader, pointCount, flags, SimpleGlyphFlags.YShortVector, SimpleGlyphFlags.YSameOrPositive);

                GlyphPoint[] points = new GlyphPoint[pointCount];
                for (int i = 0; i < pointCount; i++)
                    points[i] = new GlyphPoint(xCoords[i], yCoords[i], HasFlag(flags[i], SimpleGlyphFlags.OnCurvePoint));

                // Create glyph
                return new Glyph(bounds, contourEndPoints, points, instructions);
            }

            private SimpleGlyphFlags[] ReadFlags(BinaryEndianAgnosticReader reader, ushort pointCount)
            {
                // Read flags - There will be more flags than the data provides. Expand on-the-fly
                SimpleGlyphFlags[] flags = new SimpleGlyphFlags[pointCount];
                int i = 0;
                int repeatCount = 0;
                SimpleGlyphFlags curFlag = SimpleGlyphFlags.None;

                while(i < pointCount)
                {
                    if (repeatCount > 0)
                    {
                        repeatCount--;
                    }
                    else
                    {
                        curFlag = (SimpleGlyphFlags)reader.ReadByte();
                        if (HasFlag(curFlag, SimpleGlyphFlags.RepeatFlag))
                            repeatCount = reader.ReadByte();
                    }

                    flags[i++] = curFlag;
                }

                return flags;
            }

            private short[] ReadCoordinates(BinaryEndianAgnosticReader reader, ushort pointCount, SimpleGlyphFlags[] flags, SimpleGlyphFlags byteFlag, SimpleGlyphFlags sameFlag)
            {
                short[] coords = new short[pointCount];
                int i = 0;
                short prevCoord = 0;

                while(i < pointCount)
                {
                    // Check if the coordinate is 8-bit (1 byte)
                    if (HasFlag(flags[i], byteFlag))
                    {
                        byte val = reader.ReadByte();
                        coords[i] = (short)(HasFlag(flags[i], sameFlag) ? val : -val);
                    }
                    else
                    {
                        // If X_SHORT_VECTOR is not set and this bit is set, then the current x-coordinate is the same as the previous x-coordinate.
                        if (HasFlag(flags[i], sameFlag))
                            coords[i] = 0;
                        else // If X_SHORT_VECTOR is not set and this bit is also not set, the current x-coordinate is a signed 16-bit delta vector.
                            coords[i] = reader.ReadInt16();
                    }

                    prevCoord = coords[i];
                    i++;
                }

                return coords;
            }

            private bool HasFlag(SimpleGlyphFlags val, SimpleGlyphFlags flag)
            {
                return (val & flag) == flag;
            }

            [Flags]
            private enum SimpleGlyphFlags : byte
            {
                None = 0,

                /// <summary>
                /// Bit 0: If set, the point is on the curve; otherwise, it is off the curve.
                /// </summary>
                OnCurvePoint = 1,

                /// <summary>
                /// Bit 1: If set, the corresponding x-coordinate is 1 byte long. 
                /// If not set, it is two bytes long. For the sign of this value, see the description of the X_IS_SAME_OR_POSITIVE_X_SHORT_VECTOR flag.
                /// </summary>
                XShortVector = 2,

                /// <summary>
                /// Bit 2: If set, the corresponding y-coordinate is 1 byte long. 
                /// If not set, it is two bytes long. For the sign of this value, see the description of the Y_IS_SAME_OR_POSITIVE_Y_SHORT_VECTOR flag.
                /// </summary>
                YShortVector = 4,

                /// <summary>
                /// Bit 3: If set, the next byte (read as unsigned) specifies the number of additional times this flag is to be repeated — that is, 
                /// the number of additional logical flag entries inserted after this entry. 
                /// In this way, the number of flags listed can be smaller than the number of points in the glyph description.
                /// </summary>
                RepeatFlag = 8,

                /// <summary>
                /// Bit 4: This flag has two meanings, depending on how the X_SHORT_VECTOR flag is set.  <para/>
                /// If X_SHORT_VECTOR is set, this bit describes the sign of the value, with 1 equalling positive and 0 negative.  <para/>
                /// If X_SHORT_VECTOR is not set and this bit is set, then the current x-coordinate is the same as the previous x-coordinate. <para/>
                /// If X_SHORT_VECTOR is not set and this bit is also not set, the current x-coordinate is a signed 16-bit delta vector.
                /// </summary>
                XSameOrPositive = 16,

                /// <summary>
                /// Bit 5: This flag has two meanings, depending on how the Y_SHORT_VECTOR flag is set. <para/>
                /// If Y_SHORT_VECTOR is set, this bit describes the sign of the value, with 1 equalling positive and 0 negative.  <para/>
                /// If Y_SHORT_VECTOR is not set and this bit is set, then the current y-coordinate is the same as the previous y-coordinate.  <para/>
                /// If Y_SHORT_VECTOR is not set and this bit is also not set, the current y-coordinate is a signed 16-bit delta vector.
                /// </summary>
                YSameOrPositive = 32,

                /// <summary>
                /// Bit 6 is reserved: set to zero.
                /// </summary>
                Reserved6 = 64,

                /// <summary>
                /// Bit 7 is reserved: set to zero.
                /// </summary>
                Reserved7 = 128,
            }

            private Rectangle ReadBounds(BinaryEndianAgnosticReader reader)
            {
                return new Rectangle()
                {
                    X = reader.ReadInt16(),
                    Y = reader.ReadInt16(),
                    Right = reader.ReadInt16(),
                    Bottom = reader.ReadInt16(),
                };
            }
        }
    }

}
