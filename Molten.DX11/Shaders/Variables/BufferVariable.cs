using SharpDX.Direct3D11;
using Molten.Graphics.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal class BufferVariable : ShaderResourceVariable
    {
        BufferSegment _bufferSegment;

        internal BufferVariable(HlslShader shader) : base(shader) { }

        protected override PipelineShaderObject OnSetResource(object value)
        {
            _bufferSegment = value as BufferSegment;
            return _bufferSegment;
        }
    }
}
