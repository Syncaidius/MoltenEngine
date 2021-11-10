using System;

namespace Molten.Font
{
    /// <summary>Glyf data table (glyf).<para/>
    /// This table contains information that describes the glyphs in the font in the TrueType outline format. 
    /// Information regarding the rasterizer (scaler) refers to the TrueType rasterizer. <para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/glyf </summary>
    [FontTableTag("glyf", "loca", "maxp")]
    public class Glyf : FontTable
    {
        public Glyph[] Glyphs { get; internal set; }

        internal override void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies)
        {
            long tableStartPos = reader.Position;
            Loca loca = dependencies.Get<Loca>();
            Maxp maxp = dependencies.Get<Maxp>();

            /* https://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6glyf.html
             * Glyph description:
             * Type 	Name 	Description
             * SHORT 	numberOfContours 	If the number of contours is >= zero, this is a single glyph. If it's negative, this is a composite glyph.
             * SHORT 	xMin 	Minimum x for coordinate data.
             * SHORT 	yMin 	Minimum y for coordinate data.
             * SHORT 	xMax 	Maximum x for coordinate data.
             * SHORT 	yMax 	Maximum y for coordinate data.
             */

            ushort numGlyphs = maxp.NumGlyphs;
            Glyphs = new Glyph[numGlyphs];

            for (ushort i = 0; i < numGlyphs; i++)
            {
                // Has the glyph already loaded earlier via a composite? Skip it!
                if (Glyphs[i] != null)
                    continue;
                else
                    ReadGlyph(reader, log, Glyphs, tableStartPos, loca.Offsets, i);
            }

            // Jump to the expected end of the table (last offset in the loca table).
            // This is for debugging purposes only.
            reader.Position = tableStartPos + loca.Offsets[numGlyphs];
        }

        private void ReadGlyph(EnhancedBinaryReader reader, Logger log, Glyph[] glyphs, long tableStartPos, uint[] locaOffsets, ushort id)
        {
            uint offset = locaOffsets[id];
            uint length = locaOffsets[id + 1] - offset;

            if (length == 0)
            {
                glyphs[id] = id == 0 ? Glyph.Empty : glyphs[0];
            }
            else
            {
                reader.Position = tableStartPos + offset;

                // If the number of contours is >= zero, this is a single glyph. If it's negative, this is a composite glyph.
                short numContours = reader.ReadInt16();
                Rectangle bounds = ReadBounds(reader);

                if (numContours >= 0)
                    glyphs[id] = ReadSimpleGlyph(reader, numContours, bounds);
                else
                    glyphs[id] = ReadCompositeGlyph(reader, log, glyphs, tableStartPos, locaOffsets, bounds, id);

            }
        }

        private Glyph ReadCompositeGlyph(EnhancedBinaryReader reader, Logger log, Glyph[] glyphs, long tableStartPos, uint[] locaOffsets, Rectangle bounds, ushort id)
        {
            CompositeGlyphFlags flags;
            Glyph compositeGlyph = null;

            do
            {
                flags = (CompositeGlyphFlags)reader.ReadUInt16();
                ushort glyphID = reader.ReadUInt16();

                // Check if we're referring to an un-loaded glyph.
                if (glyphs[glyphID] == null)
                {
                    long curPos = reader.Position;
                    ReadGlyph(reader, log, glyphs, tableStartPos, locaOffsets, glyphID);
                    reader.Position = curPos;
                }

                int arg1;
                int arg2;
                Matrix2F? scaleMatrix = null;
                Glyph componentGlyph = glyphs[glyphID].Clone();

                // Is arg1 and 2 a XY offset value?
                // Argument1 and argument2 can be either x and y offsets to be added to the glyph (the ARGS_ARE_XY_VALUES flag is set), 
                // or two point numbers (the ARGS_ARE_XY_VALUES flag is not set). 
                if (HasFlag(flags, CompositeGlyphFlags.ArgsAreXYValues))
                {
                    if (HasFlag(flags, CompositeGlyphFlags.Arg1And2AreWords))
                    {
                        arg1 = reader.ReadInt16(); // 1st short contains the value of x offset
                        arg2 = reader.ReadInt16(); // 2nd short contains the value of y offset
                    }
                    else
                    {
                        arg1 = reader.ReadSByte(); // 1st byte contains the value of x offset
                        arg2 = reader.ReadSByte(); // 2nd byte contains the value of y offset
                    }

                    // Read scale values
                    if (HasFlag(flags, CompositeGlyphFlags.WeHaveScale))
                    {
                        float scale = FontUtil.FromF2DOT14(reader.ReadInt16());
                        scaleMatrix = new Matrix2F()
                        {
                            M11 = scale,
                            M12 = 0,
                            M21 = 0,
                            M22 = scale,
                        };
                    }
                    else if (HasFlag(flags, CompositeGlyphFlags.WeHaveXAndYScale))
                    {
                        scaleMatrix = new Matrix2F()
                        {
                            M11 = FontUtil.FromF2DOT14(reader.ReadInt16()),
                            M12 = 0,
                            M21 = 0,
                            M22 = FontUtil.FromF2DOT14(reader.ReadInt16())
                        };
                    }
                    else if (HasFlag(flags, CompositeGlyphFlags.WeHaveATwoByTwo))
                    {
                        scaleMatrix = new Matrix2F()
                        {
                            M11 = FontUtil.FromF2DOT14(reader.ReadInt16()),
                            M12 = FontUtil.FromF2DOT14(reader.ReadInt16()),
                            M21 = FontUtil.FromF2DOT14(reader.ReadInt16()),
                            M22 = FontUtil.FromF2DOT14(reader.ReadInt16()),
                        };
                    }

                    if (HasFlag(flags, CompositeGlyphFlags.RoundXYToGrid))
                    {
                        // TODO round to grid. Does this go before or after scaling?
                    }

                    if (scaleMatrix != null)
                    {
                        /* MS Docs: If the SCALED_COMPONENT_OFFSET flag is set, then the x and y offset values are deemed to be 
                         * in the component glyph's coordinate system, and the scale transformation is applied to both values. */
                        if (HasFlag(flags, CompositeGlyphFlags.ScaledComponentOffset))
                        {
                            Vector2F p = new Vector2F(arg1, arg2);
                            p = Vector2F.TransformNormal(p, scaleMatrix.Value);
                        }

                        // ref: https://github.com/servo/libfreetype2/blob/master/freetype2/src/truetype/ttgload.c#L1124
                        FontUtil.TransformGlyph(componentGlyph, scaleMatrix.Value);
                        FontUtil.OffsetGlyph(componentGlyph, arg1, arg2);
                    }
                    else
                    {
                        FontUtil.OffsetGlyph(componentGlyph, arg1, arg2);
                    }
                }
                else
                {
                    // We have two point values instead.
                    if (HasFlag(flags, CompositeGlyphFlags.Arg1And2AreWords))
                    {
                        arg1 = reader.ReadUInt16(); // 1st short contains the index of matching point in compound being constructed
                        arg2 = reader.ReadUInt16(); // 2nd short contains index of matching point in component
                    }
                    else
                    {
                        arg1 = reader.ReadByte(); // 1st byte contains the index of matching point in compound being constructed
                        arg2 = reader.ReadByte(); // 2nd byte contains index of matching point in component
                    }

                    // TODO ????
                }

                /* The purpose of USE_MY_METRICS is to force the lsb and rsb to take on a desired value. 
                 * For example, an i-circumflex (U+00EF) is often composed of the circumflex and a dotless-i. 
                 * In order to force the composite to have the same metrics as the dotless-i, set USE_MY_METRICS for the dotless-i component of the composite. */
                if (HasFlag(flags, CompositeGlyphFlags.UseMyMetrics))
                {
                    // TODO apply the current glyph's metrics to the composite glyph
                    // See: https://github.com/servo/libfreetype2/blob/master/freetype2/src/truetype/ttgload.c#L1912
                }

                if (compositeGlyph == null)
                    compositeGlyph = componentGlyph;
                else
                    compositeGlyph.Append(componentGlyph);

            } while (HasFlag(flags, CompositeGlyphFlags.MoreComponents));

            // Read composite glyph instructions, if any.
            byte[] instructions;
            if (HasFlag(flags, CompositeGlyphFlags.WeHaveInstructions))
            {
                ushort numInstructions = reader.ReadUInt16();
                instructions = reader.ReadBytes(numInstructions);
            }

            return compositeGlyph;
        }

        private Glyph ReadSimpleGlyph(EnhancedBinaryReader reader, short numContours, Rectangle bounds)
        {
            ushort[] contourEndPoints = reader.ReadArray<ushort>(numContours);
            ushort instructionLength = reader.ReadUInt16();
            byte[] instructions = instructionLength > 0 ? reader.ReadBytes(instructionLength) : new byte[0];

            // TODO: revisit this and attempt to reduce garbage (3 arrays get dumped).
            ushort pointCount = (ushort)(contourEndPoints[numContours - 1] + 1);
            SimpleGlyphFlags[] flags = ReadFlags(reader, pointCount);
            short[] xCoords = ReadCoordinates(reader, pointCount, flags, SimpleGlyphFlags.XShortVector, SimpleGlyphFlags.XSameOrPositive);
            short[] yCoords = ReadCoordinates(reader, pointCount, flags, SimpleGlyphFlags.YShortVector, SimpleGlyphFlags.YSameOrPositive);

            GlyphPoint[] points = new GlyphPoint[pointCount];
            Vector2F pos = Vector2F.Zero;
            for (int i = 0; i < pointCount; i++)
            {
                pos.X += xCoords[i];
                pos.Y += yCoords[i];
                points[i] = new GlyphPoint(pos, HasFlag(flags[i], SimpleGlyphFlags.OnCurvePoint));
            }

            // Create glyph
            return new Glyph(bounds, contourEndPoints, points, instructions);
        }

        private SimpleGlyphFlags[] ReadFlags(EnhancedBinaryReader reader, ushort pointCount)
        {
            // Read flags - There will be more flags than the data provides. Expand on-the-fly
            SimpleGlyphFlags[] flags = new SimpleGlyphFlags[pointCount];
            int i = 0;
            int repeatCount = 0;
            SimpleGlyphFlags curFlag = SimpleGlyphFlags.None;

            while (i < pointCount)
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

        private short[] ReadCoordinates(EnhancedBinaryReader reader, ushort pointCount, SimpleGlyphFlags[] flags, SimpleGlyphFlags byteFlag, SimpleGlyphFlags sameFlag)
        {
            short[] coords = new short[pointCount];
            int i = 0;

            while (i < pointCount)
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

                i++;
            }

            return coords;
        }

        private bool HasFlag(CompositeGlyphFlags val, CompositeGlyphFlags flag)
        {
            return (val & flag) == flag;
        }

        private bool HasFlag(SimpleGlyphFlags val, SimpleGlyphFlags flag)
        {
            return (val & flag) == flag;
        }

        [Flags]
        public enum CompositeGlyphFlags : ushort
        {
            None = 0,

            /// <summary>
            /// Bit 0: If this is set, the arguments are 16-bit (uint16 or int16); otherwise, they are bytes (uint8 or int8).
            /// </summary>
            Arg1And2AreWords = 1,

            /// <summary>
            /// Bit 1: If this is set, the arguments are signed xy values; otherwise, they are unsigned point numbers.
            /// </summary>
            ArgsAreXYValues = 2,

            /// <summary>
            /// Bit 2: For the xy values if the preceding is true.
            /// </summary>
            RoundXYToGrid = 4,

            /// <summary>
            /// Bit 3: This indicates that there is a simple scale for the component. Otherwise, scale = 1.0.
            /// </summary>
            WeHaveScale = 8,

            /// <summary>
            /// Bits 4 reserved: set to 0.
            /// </summary>
            Reserved4 = 16,

            /// <summary>
            /// Bit 5: Indicates at least one more glyph after this one.
            /// </summary>
            MoreComponents = 32,

            /// <summary>
            /// Bit 6: The x direction will use a different scale from the y direction.
            /// </summary>
            WeHaveXAndYScale = 64,

            /// <summary>
            /// Bit 7: There is a 2 by 2 transformation that will be used to scale the component.
            /// </summary>
            WeHaveATwoByTwo = 128,

            /// <summary>
            /// Bit 8: Following the last component are instructions for the composite character.
            /// </summary>
            WeHaveInstructions = 256,

            /// <summary>
            /// Bit 9: If set, this forces the aw and lsb (and rsb) for the composite to be equal to those from this original glyph. This works for hinted and unhinted characters.
            /// </summary>
            UseMyMetrics = 512,

            /// <summary>
            /// Bit 10: If set, the components of the compound glyph overlap. Use of this flag is not required in OpenType — that is, 
            /// it is valid to have components overlap without having this flag set. <para/>
            /// It may affect behaviors in some platforms, however. (See Apple's specification for details regarding behavior in Apple platforms.)
            /// </summary>
            OverlapCompound = 1024,

            /// <summary>
            /// Bit 11: The composite is designed to have the component offset scaled.
            /// </summary>
            ScaledComponentOffset = 2048,

            /// <summary>
            /// Bit 12: The composite is designed not to have the component offset scaled.
            /// </summary>
            UnscaledComponentOffset = 4096,

            /// <summary>
            /// Bit 13 is reserved: set to 0.
            /// </summary>
            Reserved13 = 8192,

            /// <summary>
            /// Bit 14 is reserved: set to 0.
            /// </summary>
            Reserved14 = 16384,

            /// <summary>
            /// Bit 15 is reserved: set to 0.
            /// </summary>
            Reserved15 = 32768,
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

        private Rectangle ReadBounds(EnhancedBinaryReader reader)
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
