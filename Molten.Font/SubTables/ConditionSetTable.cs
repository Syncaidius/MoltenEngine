using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class ConditionSetTable : FontSubTable
    {
        public ConditionTable[] Tables { get; internal set; }

        internal override void Read(EnhancedBinaryReader reader, FontReaderContext context, FontTable parent)
        {
            ushort conditionCount = reader.ReadUInt16();
            uint[] conditionTableOffsets = reader.ReadArray<uint>(conditionCount);
            Tables = new ConditionTable[conditionCount];
            for (int i = 0; i < conditionCount; i++)
                Tables[i] = context.ReadSubTable<ConditionTable>(conditionTableOffsets[i]);
        }
    }

    public class ConditionTable : FontSubTable
    {
        /// <summary>
        /// Gets the format of the current <see cref="ConditionTable"/>.
        /// </summary>
        public ushort Format { get; internal set; }

        /// <summary>
        /// Gets the index (zero-based) for the variation axis within the 'fvar' table.
        /// </summary>
        public ushort AxisIndex { get; internal set; }

        /// <summary>
        /// Gets the minimum value of the font variation instances that satisfy this condition.
        /// </summary>
        public float FilterRangeMinValue { get; internal set; }

        /// <summary>
        /// Gets the maximum value of the font variation instances that satisfy this condition.
        /// </summary>
        public float FilterRangeMaxValue { get; internal set; }

        internal override void Read(EnhancedBinaryReader reader, FontReaderContext context, FontTable parent)
        {
            Format = reader.ReadUInt16();

            switch (Format)
            {
                case 1:
                    AxisIndex = reader.ReadUInt16();
                    FilterRangeMinValue = FontUtil.FromF2DOT14(reader.ReadUInt16());
                    FilterRangeMaxValue = FontUtil.FromF2DOT14(reader.ReadUInt16());
                    break;
            }
        }
    }
}
