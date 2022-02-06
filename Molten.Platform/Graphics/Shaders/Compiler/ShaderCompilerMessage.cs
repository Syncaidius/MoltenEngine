using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class ShaderCompilerMessage
    {
        public enum Kind
        {
            Message = 0,

            Error = 1,

            Warning = 2,

            Debug = 3,
        }

        public string Text;

        public Kind MessageType;
    }
}
