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
        TextureAssetCube _texture;

        internal TextureCubeVariable(HlslShader shader) : base(shader) { }

        protected override PipelineShaderObject OnSetResource(object value)
        {
            if (value != null)
                _texture = value as TextureAssetCube;
            else
                _texture = null;

            return _texture;
        }
    }
}
