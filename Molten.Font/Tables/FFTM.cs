using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>FontForge Time-stamp table. See: https://fontforge.github.io/non-standard.html#FFTM</summary>
    public class FFTM : FontTable
    {
        public uint Version { get; internal set; }

        /// <summary>Gets the time-stamp of the FontForge sources used to create the font.</summary>
        public DateTime SourceTimeStamp { get; internal set; }

        /// <summary>Gets the creation date of the font. 
        /// This is not the creation date of the tt/ot file, but the date the sfd file was created. 
        /// (not always accurate).</summary>
        public DateTime CreationDate { get; internal set; }

        /// <summary>Gets the date that the font was last modified. 
        /// This is not the modification date of the file, but the time a glyph, etc. was last changed in the font database. 
        /// (not always accurate)</summary>
        public DateTime LastModified { get; internal set; }

        public class Parser : FontTableParser
        {
            public override string TableTag => "FFTM";

            public override FontTable Parse(BinaryEndianAgnosticReader reader, TableHeader header, Logger log)
            {
                return new FFTM()
                {
                    Version = reader.ReadUInt32(),
                    SourceTimeStamp = ReadHeadDate(reader),
                    CreationDate = ReadHeadDate(reader),
                    LastModified = ReadHeadDate(reader),
                };
            }
        }
    }

}
