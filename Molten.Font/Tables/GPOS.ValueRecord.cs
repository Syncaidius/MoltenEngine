using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public partial class GPOS
    {
        public class ValueRecord
        {
            /// <summary>
            /// Horizontal adjustment for placement, in design units.
            /// </summary>
            public short XPlacement { get; private set; }

            /// <summary>
            /// Vertical adjustment for placement, in design units.
            /// </summary>
            public short YPlacement { get; private set; }

            /// <summary>
            /// Horizontal adjustment for advance, in design units — only used for horizontal layout.
            /// </summary>
            public short XAdvance { get; private set; }

            /// <summary>
            /// Vertical adjustment for advance, in design units — only used for vertical layout.
            /// </summary>
            public short YAdvance { get; private set; }

            /// <summary>
            /// Offset to Device table (non-variable font) / VariationIndex table (variable font) for horizontal placement, from beginning of positioning subtable (SimplePos, PairPos — may be NULL)
            /// </summary>
            public ushort XPlaDeviceOffset { get; private set; }

            /// <summary>
            /// Offset to Device table (non-variable font) / VariationIndex table (variable font) for vertical placement, from beginning of positioning subtable (SimplePos, PairPos — may be NULL)
            /// </summary>
            public ushort YPlaDeviceOffset { get; private set; }

            /// <summary>
            /// Offset to Device table (non-variable font) / VariationIndex table (variable font) for horizontal advance, from beginning of positioning subtable (SimplePos, PairPos — may be NULL)
            /// </summary>
            public ushort XAdvDeviceOffset { get; private set; }

            /// <summary>
            /// Offset to Device table (non-variable font) / VariationIndex table (variable font) for vertical advance, from beginning of positioning subtable (SimplePos, PairPos — may be NULL)
            /// </summary>
            public ushort YAdvDeviceOffset { get; private set; }

            /// <summary>
            /// Gets a flags which describes the information present in the current <see cref="ValueRecord"/>.
            /// </summary>
            public ValueFormat Format { get; private set; }

            internal ValueRecord(BinaryEndianAgnosticReader reader, ValueFormat format)
            {
                Format = format;

                if (HasFormat(ValueFormat.XPlacement))
                    XPlacement = reader.ReadInt16();

                if (HasFormat(ValueFormat.YPlacement))
                    YPlacement = reader.ReadInt16();

                if (HasFormat(ValueFormat.XAdvance))
                    XAdvance = reader.ReadInt16();

                if (HasFormat(ValueFormat.YAdvance))
                    YAdvance = reader.ReadInt16();

                if (HasFormat(ValueFormat.XPlacementDevice))
                    XPlaDeviceOffset = reader.ReadUInt16();

                if (HasFormat(ValueFormat.YPlacementDevice))
                    YPlaDeviceOffset = reader.ReadUInt16();

                if (HasFormat(ValueFormat.XAdvanceDevice))
                    XAdvDeviceOffset = reader.ReadUInt16();

                if (HasFormat(ValueFormat.YAdvanceDevice))
                    YAdvDeviceOffset = reader.ReadUInt16();
            }

            public bool HasFormat(ValueFormat formatValue)
            {
                return (Format & formatValue) == formatValue;
            }
        }

        [Flags]
        public enum ValueFormat
        {
            /// <summary>
            /// Includes horizontal adjustment for placement
            /// </summary>
            XPlacement = 1,

            /// <summary>
            /// Includes vertical adjustment for placement
            /// </summary>
            YPlacement = 1 << 1,

            /// <summary>
            /// Includes horizontal adjustment for advance
            /// </summary>
            XAdvance = 1 << 2,

            /// <summary>
            /// Includes vertical adjustment for advance
            /// </summary>
            YAdvance = 1 << 4,

            /// <summary>
            /// Includes Device table (non-variable font) / VariationIndex table (variable font) for horizontal placement
            /// </summary>
            XPlacementDevice = 1 << 5,

            /// <summary>
            /// Includes Device table (non-variable font) / VariationIndex table (variable font) for vertical placement
            /// </summary>
            YPlacementDevice = 1 << 6,

            /// <summary>
            /// Includes Device table (non-variable font) / VariationIndex table (variable font) for horizontal advance
            /// </summary>
            XAdvanceDevice = 1 << 7,

            /// <summary>
            /// Includes Device table (non-variable font) / VariationIndex table (variable font) for vertical advance
            /// </summary>
            YAdvanceDevice = 1 << 8,

            /// <summary>
            /// Reserved for future use (set to zero).
            /// </summary>
            Reserved = 1 << 9
        }
    }
}
