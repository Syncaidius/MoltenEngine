using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>A small container class for tracking the tables that must be present for a <see cref="FontFile"/> to earn the attached <see cref="FontFlags"/>.</summary>
    internal class FontRequirements
    {
        public FontFlags Flags;
        public string[] Tables;

        internal FontRequirements(FontFlags flags, params string[] tables)
        {
            Tables = tables;
            Flags = flags;
        }
    }

    /// <summary>
    /// A helper class for validating <see cref="FontFile"/> table structures and producing a set of <see cref="FontFlags"/>.
    /// </summary>
    internal static class FontValidator
    {
        public readonly static FontRequirements[] _requirements = new FontRequirements[]
        {
            new FontRequirements(FontFlags.Valid,  "cmap", "head", "hhea", "hmtx", "maxp", "name", "OS/2", "post" ),
            new FontRequirements(FontFlags.TrueType, "glyf", "loca" ),
            new FontRequirements(FontFlags.CFF, "CFF"),
            new FontRequirements(FontFlags.CFF2, "CFF2"),
            new FontRequirements(FontFlags.SVG, "SVG"),
            new FontRequirements(FontFlags.DigitallySigned, "DSIG"),
            new FontRequirements(FontFlags.Kerning, "kern"),
        };

        internal static FontFlags Validate(FontTableList tables)
        {
            // The first set of required tables is always required. We cannot produce a usable FontFile instance without these.
            FontFlags flags = ValidateRequiredTables(_requirements[0], tables);
            if (flags == FontFlags.Valid)
            {
                for (int i = 1; i < _requirements.Length; i++)
                    flags |= ValidateRequiredTables(_requirements[i], tables);
            }

            return flags;
        }

        private static FontFlags ValidateRequiredTables(FontRequirements req, FontTableList tables)
        {
            for (int i = 0; i < req.Tables.Length; i++)
            {
                if (tables.Get(req.Tables[i]) == null)
                    return FontFlags.Invalid;
            }

            return req.Flags;
        }
    }
}
