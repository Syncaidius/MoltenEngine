using Molten.Graphics.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal class Texture1DVariable : ShaderResourceVariable
    {
        Texture1DDX11 _texture;

        internal Texture1DVariable(Material material) : base(material) { }

        protected override PipeBindableResource OnSetResource(object value)
        {
            _texture = value as Texture1DDX11;
            return _texture;
        }
    }
}
