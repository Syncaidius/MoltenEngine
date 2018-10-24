using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Textures
{
    /// <summary>A base class for DDS block readers.</summary>
    internal abstract class BCBlockParser
    {
        public abstract GraphicsFormat ExpectedFormat { get; }

        internal abstract Color4[] Decode(BinaryReader imageReader);

        internal abstract void Encode(BinaryWriter writer, Color4[] uncompressed);
    }
}
