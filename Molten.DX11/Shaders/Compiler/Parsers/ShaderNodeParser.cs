using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    internal abstract class ShaderNodeParser
    {
        internal abstract string[] SupportedNodes { get; }

        internal abstract NodeParseResult Parse(HlslFoundation foundation, ShaderCompilerContext context, XmlNode node);
    }
}
