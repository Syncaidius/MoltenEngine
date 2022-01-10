using Molten.Graphics.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal class RWTexture1DVariable : RWVariable
    {
        Texture1DDX11 _texture;

        internal RWTexture1DVariable(HlslShader shader) : base(shader) { }

        protected override PipeBindableResource OnSetUnorderedResource(object value)
        {
            _texture = value as Texture1DDX11;

            if (_texture != null)
            {
                if ((_texture.Flags & TextureFlags.AllowUAV) != TextureFlags.AllowUAV)
                    throw new InvalidOperationException("A texture cannot be passed to a RWTexture2D resource constant without .AllowUAV flags.");
            }

            return _texture;
        }
    }
}
