using SharpDX.Direct3D11;
using Molten.Graphics.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal class TextureCubeVariable : ShaderResourceVariable
    {
        TextureCubeDX11 _texture;

        internal TextureCubeVariable(HlslShader shader) : base(shader) { }

        protected override PipelineShaderObject OnSetResource(object value)
        {
            if (value != null)
                _texture = value as TextureCubeDX11;
            else
                _texture = null;

            return _texture;
        }
    }
}
