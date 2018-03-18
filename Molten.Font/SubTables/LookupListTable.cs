using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class LookupListTable : FontSubTable
    {
        List<LookupTable> _subTables = new List<LookupTable>();

        public IReadOnlyCollection<LookupTable> SubTables { get; internal set; }

        internal LookupListTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset, Type[] lookupTypeIndex, ushort extensionIndex) : 
            base(reader, log, parent, offset)
        {
            ushort lookupCount = reader.ReadUInt16();
            ushort[] lookupOffsets = reader.ReadArrayUInt16(lookupCount);
            log.WriteDebugLine($"Reading lookup list table at {Header.Offset} containing {lookupCount} lookup tables");

            List<LookupTable> subtables = new List<LookupTable>();
            SubTables = subtables.AsReadOnly();

            for (int i = 0; i < lookupCount; i++)
            {
                long lookupStartPos = Header.Offset + lookupOffsets[i];
                reader.Position = lookupStartPos;
                ushort lookupType = reader.ReadUInt16();
                LookupFlags flags = (LookupFlags)reader.ReadUInt16();
                ushort subTableCount = reader.ReadUInt16();
                log.WriteDebugLine($"Reading lookup table {i+1}/{lookupCount} at {lookupStartPos} containing {subTableCount} sub-tables");

                // Get the offset's for the lookup subtable's own subtables.
                uint[] subTableOffsets = new uint[subTableCount];
                reader.ReadArrayUInt16(subTableOffsets, subTableCount);
                ushort markFilteringSet = 0;
                if (HasFlag(flags, LookupFlags.UseMarkFilteringSet))
                    markFilteringSet = reader.ReadUInt16();

                for (int s = 0; s < subTableCount; s++)
                {
                    // Check for extension.
                    // MS Docs: This lookup provides a mechanism whereby any other lookup type's subtables are stored at a 32-bit offset location in the 'GPOS' table
                    if (lookupType == extensionIndex)
                    {
                        ushort posFormat = reader.ReadUInt16();
                        lookupType = reader.ReadUInt16(); // extensionLookupType.
                        subTableOffsets[s] = reader.ReadUInt32(); // overwrite the offset for this table with the 32-bit offset.

                        // ExtensionLookupType must be set to any lookup type other than the extension lookup type.
                        if (lookupType == extensionIndex)
                        {
                            log.WriteDebugLine($"Nested extension lookup table detected. Ignored.");
                            continue;
                        }
                    }

                    // Skip unsupported tables.
                    if (lookupType >= lookupTypeIndex.Length || lookupTypeIndex[lookupType] == null)
                    {
                        log.WriteDebugLine($"Unsupported lookup sub-table type: {lookupType}");
                        continue;
                    }

                    long subOffsetFromListStart = lookupOffsets[i] + subTableOffsets[s];
                    LookupTable subTable = Activator.CreateInstance(lookupTypeIndex[lookupType], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                        new object[] { reader, log, this, subOffsetFromListStart, lookupType, flags, markFilteringSet }, null) as LookupTable;
                    subtables.Add(subTable);
                }
            }
        }

        private bool HasFlag(LookupFlags value, LookupFlags flag)
        {
            return (value & flag) == flag;
        }
    }

    public abstract class LookupTable : FontSubTable
    {
        internal LookupTable(EnhancedBinaryReader reader,
            Logger log,
            IFontTable parent,
            long offset,
            ushort lookupType,
            LookupFlags flags,
            ushort markFilteringSet) :
            base(reader, log, parent, offset)
        { }
    }

    [Flags]
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
