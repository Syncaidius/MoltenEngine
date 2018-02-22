using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class LookupTable<T> where T : LookupSubTable, new()
    {
        List<T> _subTables = new List<T>();

        public IReadOnlyCollection<T> SubTables { get; internal set; }

        internal LookupTable(BinaryEndianAgnosticReader reader, Logger log, Type[] lookupTypeIndex)
        {
            long lookupStartPos = reader.Position;
            ushort lookupCount = reader.ReadUInt16();
            ushort[] lookupOffsets = reader.ReadArrayUInt16(lookupCount);

            List<T> subtables = new List<T>();
            SubTables = subtables.AsReadOnly();

            for (int i = 0; i < lookupCount; i++)
            {
                reader.Position = lookupStartPos + lookupOffsets[i];
                ushort lookupType = reader.ReadUInt16();
                LookupFlags lookupFlags = (LookupFlags)reader.ReadUInt16();
                ushort subTableCount = reader.ReadUInt16();

                // Get the offset's for the lookup subtable's own subtables.
                ushort[] subTableOffsets = reader.ReadArrayUInt16(subTableCount);
                ushort markFilteringSet = reader.ReadUInt16();

                for (int s = 0; s < subTableCount; s++)
                {
                    long subStartPos = lookupStartPos + subTableOffsets[s];
                    reader.Position = subStartPos;
                    T subTable = Activator.CreateInstance(lookupTypeIndex[lookupType]) as T;
                    subTable.Read(reader, log, lookupStartPos, subStartPos, lookupType, lookupFlags, markFilteringSet);
                    subtables.Add(subTable);
                }
            }
        }
    }

    public abstract class LookupSubTable
    {
        internal abstract void Read(BinaryEndianAgnosticReader reader, 
            Logger log, 
            long lookupStartPos, 
            long subStartPos, 
            ushort lookupType,
            LookupFlags flags,
            ushort markFilteringSet);
    }

    public enum LookupFlags
    {
        None = 0,

        RightToLeft = 1,

        IgnoreBaseGlyphs = 1 << 1,

        IgnoreLigatures = 1 << 2,

        IgnoreMarks = 1 << 3,

        UseMarkFilteringSet = 1 << 4,

        Reserved = 1 << 5,

        MarkAttachmentType = 1 << 6,
    }
}
