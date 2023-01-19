using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class ShaderTypeInfo
    {
        public uint RowCount;

        public uint ColumnCount;

        public ShaderVariableType Type;

        public ShaderVariableClass Class;

        public uint Offset;

        public string Name;

        public uint Elements;
    }
}
