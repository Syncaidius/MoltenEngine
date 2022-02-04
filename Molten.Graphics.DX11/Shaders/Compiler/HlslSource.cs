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

        internal HlslSource(string filename, ref string src, HlslSourceType type, int originalLineCount,
            Assembly assembly = null, string nameSpace = null)
        {
            Filename = filename;
            ParentNamespace = nameSpace;
            Dependencies = new List<HlslSource>();
            SourceType = type;
            ParentAssembly = assembly;
            FullFilename = filename;

            string[] lines = src.Split('\n');
            LineCount = lines.Length;
            _src = $"#line 1 \"{filename}\"\n{src}\n#line {originalLineCount} \"{filename}\"";

            if (type == HlslSourceType.EmbeddedFile)
            {
                if (assembly != null)
                {
                    if (string.IsNullOrWhiteSpace(nameSpace))
                        FullFilename = $"{filename}, {assembly}";
                    else
                        FullFilename = $"{nameSpace}.{filename}, {assembly}";
                }
            }
        }

        protected override void OnDispose()
        {
            SilkUtil.ReleasePtr(ref _blob);
        }

        /// <summary>
        /// (Re)builds the HLSL source code in the current <see cref="HlslSource"/> instance. 
        /// This generates a (new) <see cref="Buffer"/> object.
        /// </summary>
        /// <param name="compiler"></param>
        /// <returns></returns>
        internal Buffer BuildSource(HlslCompiler compiler)
        {
            if (_blob != null)
                SilkUtil.ReleasePtr(ref _blob);

            NumBytes = (uint)(sizeof(char) * SourceCode.Length);
            void* ptrSource = (void*)SilkMarshal.StringToPtr(_src, NativeStringEncoding.UTF8);
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

        internal string ParentNamespace { get; }

        internal string FullFilename { get; }
        
        public int LineCount { get; }
    }
}
