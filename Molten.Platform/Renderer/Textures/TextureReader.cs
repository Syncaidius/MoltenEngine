using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    public abstract class TextureReader : IDisposable
    {
        public abstract TextureData Read(BinaryReader reader, Logger log, string filename = null);

        public virtual void Dispose() { }
    }
}
