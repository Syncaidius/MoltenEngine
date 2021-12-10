using Molten.Graphics.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal class Texture2DVariable : ShaderResourceVariable
    {
        Texture2DDX11 _texture;

        internal Texture2DVariable(HlslShader shader) : base(shader) { }

        protected override PipeBindableResource OnSetResource(object value)
        {
            if (value != null)
            {
                _texture = value as Texture2DDX11;

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
