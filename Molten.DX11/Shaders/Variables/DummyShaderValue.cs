using SharpDX.Direct3D11;
using Molten.Graphics.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace Molten.Graphics
{
    /// <summary>
    /// Represents a dummy shader value which is not linked to any shader constant buffers or resources.
    /// </summary>
    internal class DummyShaderValue : IShaderValue
    {
        internal DummyShaderValue() { }

        public string Name { get; set; }

        public object Value { get; set; }
    }
}
