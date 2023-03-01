using System.Text.RegularExpressions;

namespace Molten.Graphics
{
    public abstract class ShaderClassCompiler
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

        public abstract List<HlslElement> Parse(ShaderCompilerContext context, RenderService renderer, in string header);

        public abstract ShaderClassType ClassType { get; }
    }
}
