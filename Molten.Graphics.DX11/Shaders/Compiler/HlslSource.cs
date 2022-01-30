using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Buffer = Silk.NET.Direct3D.Compilers.Buffer;

namespace Molten.Graphics
{
    internal unsafe class HlslSource : EngineObject
    {
        static Regex _includeCommas = new Regex("(?:#include) (?:\")([.+])(?:\")");
        static Regex _includeBrackets = new Regex("(?:#include) (?:<)([.+])(?:>)");

        string _src;
        IDxcBlobEncoding* _blob;
        Buffer _buffer;

        internal HlslSource(HlslCompiler compiler, string filename, ref string src)
        {
            Filename = filename;
            Dependencies = new List<HlslSource>();
            _src = src;

            ParseDependencies(compiler);
        }

        private void ParseDependencies(HlslCompiler compiler)
        {
            // See for info: https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-appendix-pre-include

            // Parse #include <file> - Only checks INCLUDE path and "in paths specified by the /I compiler option, in the order in which they are listed."
            Match m = _includeBrackets.Match(_src);
            while (m.Success)
            {
                string fn = m.Value.Trim().Replace("<", "").Replace(">", "");

                m = m.NextMatch();
            }

            // Parse #Include "file" - Above + local source file directory
            m = _includeCommas.Match(_src);
            while (m.Success)
            {
                string fn = m.Value.Trim().Replace("\"", "");

                m = m.NextMatch();
            }
        }

        public override string ToString()
        {
            return _src;
        }

        protected override void OnDispose()
        {
            SilkUtil.ReleasePtr(ref _blob);
        }

        internal Buffer BuildFinalSource(HlslCompiler compiler)
        {
            if (_blob != null)
                return _buffer;

            string finalSrc = "";
            foreach (HlslSource src in Dependencies)
                finalSrc += src.Source;

            finalSrc += _src;

            NumBytes = (uint)(sizeof(char) * finalSrc.Length);
            void* ptrSource = (void*)SilkMarshal.StringToPtr(_src, NativeStringEncoding.LPWStr);
            compiler.Utils->CreateBlob(ptrSource, NumBytes, DXC.CPUtf16, ref _blob);

            _buffer = new Buffer()
            {
                Ptr = _blob->GetBufferPointer(),
                Size = _blob->GetBufferSize(),
                Encoding = 0
            };

            return _buffer;
        }

        /// <summary>
        /// Gets the filename that the current <see cref="HlslSource"/> represents.
        /// </summary>
        public string Filename { get; private set; }

        public uint NumBytes { get; private set; }

        /// <summary>
        /// Gets a reference to the HLSL source code string
        /// </summary>
        public ref string Source => ref _src;

        internal List<HlslSource> Dependencies { get; }
    }
}
