using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class ScriptList
    {
        public ScriptRecord[] Records { get; internal set; }

        internal ScriptList(BinaryEndianAgnosticReader reader, Logger log, long startPos)
        {
            reader.Position = startPos;
            ushort scriptCount = reader.ReadUInt16();
            Records = new ScriptRecord[scriptCount];

            ushort[] scriptTableOffsets = new ushort[scriptCount];
            for(int i = 0; i < scriptCount; i++)
            {
                Records[i] = new ScriptRecord()
                {
                    Tag = FontUtil.ReadTag(reader),
                };

                scriptTableOffsets[i] = reader.ReadUInt16();
            }

            for (int i = 0; i < scriptCount; i++)
                Records[i].Table = new ScriptTable(reader, startPos + scriptTableOffsets[i]);
        }
    }

    public class ScriptRecord
    {
        public string Tag { get; internal set; }

        public ScriptTable Table { get; internal set; }
    }

    public class ScriptTable
    {
        public LangSysRecord[] Records { get; internal set; }

        internal ScriptTable(BinaryEndianAgnosticReader reader, long startPos)
        {
            ushort defaultLangSys = reader.ReadUInt16();
            ushort langSysCount = reader.ReadUInt16();

            ushort[] langSysOffsets = new ushort[langSysCount];
            for (int i = 0; i < langSysCount; i++)
            {
                Records[i] = new LangSysRecord()
                {
                    Tag = FontUtil.ReadTag(reader),
                };

                langSysOffsets[i] = reader.ReadUInt16();
            }

            for (int i = 0; i < langSysCount; i++)
                Records[i].Table = new LangSysTable(reader, startPos + langSysOffsets[i]);
        }
    }

    public class LangSysRecord
    {
        public string Tag { get; internal set; }

        public LangSysTable Table { get; internal set; }
    }

    public class LangSysTable
    {
        internal LangSysTable(BinaryEndianAgnosticReader reader, long startPos)
        {

        }
    }
}
