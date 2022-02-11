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
    internal unsafe class HlslSource : ShaderSource<Buffer>
    {
        IDxcBlobEncoding* _blob;
        Buffer _buffer;

        internal HlslSource(string filename, ref string src, ShaderCompileFlags type, int originalLineCount,
            Assembly assembly = null, string nameSpace = null) :
            base(filename, ref src, type, originalLineCount, assembly, nameSpace)
        {
            
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
        internal Buffer BuildSource(FxcCompiler compiler)
        {
            if (_blob != null)
                return _buffer;

            NumBytes = (uint)(sizeof(char) * SourceCode.Length);
            void* ptrSource = (void*)SilkMarshal.StringToPtr(SourceCode, NativeStringEncoding.UTF8);
            compiler.Utils->CreateBlob(ptrSource, NumBytes, DXC.CPUtf16, ref _blob);

            _buffer = new Buffer()
            {
                Ptr = _blob->GetBufferPointer(),
                Size = _blob->GetBufferSize(),
                Encoding = 0
            };

            return _buffer;
        }        
    }
}
