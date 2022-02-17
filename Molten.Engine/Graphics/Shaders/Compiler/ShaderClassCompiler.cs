using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    public abstract class ShaderClassCompiler<R, S, CR>
        where R : RenderService
        where S : IShaderElement
        where CR : ShaderCompileResult
    {
        Regex _regexHeader;

        public ShaderClassCompiler()
        {
            string headerTagName = ClassType.ToString().ToLower();
            _regexHeader = new Regex($"<{headerTagName}>(.|\n)*?</{headerTagName}>");
        }

        internal List<string> GetHeaders(in string source)
        {
            List<string> headers = new List<string>();
            Match m = _regexHeader.Match(source);

            while (m.Success)
            {
                headers.Add(m.Value);
                m = m.NextMatch();
            }

            return headers;
        }

        public abstract List<IShaderElement> Parse(ShaderCompilerContext<R, S, CR> context, R renderer, in string header);

        public abstract ShaderClassType ClassType { get; }
    }
}
