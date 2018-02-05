using SharpDX.Direct3D11;
using Molten.Graphics.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal class RWTexture2DVariable : RWVariable
    {
        TextureAsset2D _texture;

        internal RWTexture2DVariable(HlslShader shader) : base(shader) { }

        protected override PipelineShaderObject OnSetUnorderedResource(object value)
        {
            _texture = value as TextureAsset2D;

            if (_texture != null)
            {
                if (_texture is SwapChainSurface)
                    throw new InvalidOperationException("Texture must not be a swap chain render target.");
                else if ((_texture.Flags & TextureFlags.AllowUAV) != TextureFlags.AllowUAV)
                    throw new InvalidOperationException("A texture cannot be passed to a RWTexture2D resource constant without .AllowUAV flags.");
            }

            return _texture;
        }
    }
}
