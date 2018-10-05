using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    public abstract class TextureWriter : EngineObject
    {
        protected string _error;

        public TextureWriter() { }

        public abstract void WriteData(Stream stream, TextureData data);

        /// <summary>Gets the error message, if any. This explains what went wrong if the file failed to load.</summary>
        public string Error { get { return _error; } }
    }
}
