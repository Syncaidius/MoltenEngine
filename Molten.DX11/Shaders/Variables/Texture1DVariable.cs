using SharpDX.Direct3D11;
using Molten.Graphics.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal class Texture1DVariable : ShaderResourceVariable
    {
        TextureAsset1D _texture;

        internal Texture1DVariable(Material material) : base(material) { }

        protected override PipelineShaderObject OnSetResource(object value)
        {
            _texture = value as TextureAsset1D;
            return _texture;
        }
    }
}
