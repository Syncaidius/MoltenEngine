using SharpDX.Direct3D11;
using Molten.Graphics.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal class Texture2DVariable : ShaderResourceVariable
    {
        TextureAsset2D _texture;

        internal Texture2DVariable(HlslShader shader) : base(shader) { }

        protected override PipelineShaderObject OnSetResource(object value)
        {
            if (value != null)
            {
                _texture = value as TextureAsset2D;

                if (_texture != null)
                {
                    if (_texture is SwapChainSurface)
                        throw new InvalidOperationException("Texture must not be a swap chain render target.");
                }
            }
            else
            {
                _texture = null;
            }

            return _texture;
        }
    }
}
