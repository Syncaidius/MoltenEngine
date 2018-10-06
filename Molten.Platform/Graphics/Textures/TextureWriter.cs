using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    public abstract class TextureWriter : EngineObject
    {
        public abstract void WriteData(Stream stream, TextureData data, Logger log, string filename = null);
    }
}
