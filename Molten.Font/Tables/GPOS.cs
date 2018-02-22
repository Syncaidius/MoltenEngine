using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Index-to-location table.<para/>
    /// <para>The indexToLoc table stores the offsets to the locations of the glyphs in the font, relative to the beginning of the glyphData table. In order to compute the length of the last glyph element, there is an extra entry after the last valid index.</para>
    /// <para>By definition, index zero points to the "missing character," which is the character that appears if a character is not found in the font. The missing character is commonly represented by a blank box or a space. If the font does not contain an outline for the missing character, then the first and second offsets should have the same value. This also applies to any other characters without an outline, such as the space character. If a glyph has no outline, then loca[n] = loca [n+1]. In the particular case of the last glyph(s), loca[n] will be equal the length of the glyph data ('glyf') table. The offsets must be in ascending order with loca[n] less-or-equal-to loca[n+1].</para>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/gpos </summary>
    public partial class GPOS : FontGTable<GPosLookupSubTable>
    {
        Dictionary<GPOSLookupType, LookupSubTable> _gposLookup = new Dictionary<GPOSLookupType, LookupSubTable>();
        

        public LookupSubTable GetLookupTable(GPOSLookupType type)
        {
            if (_gposLookup.TryGetValue(type, out LookupSubTable subTable))
                return subTable;
            else
                return null;
        }

        internal class GPOSParser : Parser
        {
            public override string TableTag => "GPOS";

            protected override Type[] GetLookupTypeIndex()
            {
                return new Type[]
                {

                };
            }

            protected override FontGTable<GPosLookupSubTable> CreateTable(BinaryEndianAgnosticReader reader, TableHeader header, Logger log, FontTableList dependenceies)
            {
                return new GPOS();
            }
        }
    }

    public enum GPOSLookupType
    {
        None = 0,

        SingleAdjustment = 1,

        PairAdjustment = 2,

        CursiveAttachment = 3,

        MarkToBaseAttachment = 4,

        MarkToLigatureAttachment = 5,

        MarkToMarkAttachment = 6,

        ContextPositioning = 7,

        ChainedContextPositioning = 8,

        ExtensionPositioning = 9,

        Reserved = 10,
    }

    public abstract class GPosLookupSubTable : LookupSubTable
    {
        public GPOSLookupType Type { get; protected set; }
    }

    public class SingleAdjustmentPositioningTable : GPosLookupSubTable
    {
        internal override void Read(BinaryEndianAgnosticReader reader, Logger log, long lookupStartPos, long subStartPos, ushort lookupType, LookupFlags flags, ushort markFilteringSet)
        {
            GPOSLookupType lt = (GPOSLookupType)lookupType;
            Type = lt;
        }
    }
}
