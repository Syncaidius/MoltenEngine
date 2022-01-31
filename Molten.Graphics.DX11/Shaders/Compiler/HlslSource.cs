using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Buffer = Silk.NET.Direct3D.Compilers.Buffer;

namespace Molten.Graphics
{
    internal unsafe class HlslSource : EngineObject
    {
        string _src;
        IDxcBlobEncoding* _blob;
        Buffer _buffer;

        internal HlslSource(string filename, ref string src, HlslSourceType type, Assembly assembly = null)
        {
            Filename = filename;
            Dependencies = new List<HlslSource>();
            SourceType = type;
            ParentAssembly = type == HlslSourceType.EmbeddedFile ? assembly : null; 

            string[] lines = src.Split('\n');
            LineCount = lines.Length;
            _src = $"#line 1 {filename}\n{src}\n#line {LineCount + 1} {filename}";
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
                finalSrc += src.SourceCode;

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
        public ref string SourceCode => ref _src;

        internal List<HlslSource> Dependencies { get; }

        /// <summary>
        /// Gets the type of the current <see cref="HlslSource"/>.
        /// </summary>
        internal HlslSourceType SourceType { get; }

        internal Assembly ParentAssembly { get; }

        public int LineCount { get; }
    }
}
