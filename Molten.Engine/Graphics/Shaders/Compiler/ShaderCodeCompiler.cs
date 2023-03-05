using System.Text.RegularExpressions;

namespace Molten.Graphics
{
    public abstract class ShaderCodeCompiler
    {
        Regex _regexHeader;

        protected ShaderCodeCompiler()
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

        /// <summary>
        /// Invoked when the current <see cref="ShaderCodeCompiler"/> should build a one or more pieces of HLSL bytecode.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="renderer"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public abstract List<HlslGraphicsObject> Build(ShaderCompilerContext context, RenderService renderer, in string header);

        /// <summary>
        /// Gets the type of shader that the current <see cref="ShaderCodeCompiler"/> is meant to build.
        /// </summary>
        public abstract ShaderCodeType ClassType { get; }
    }
}
