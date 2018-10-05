using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    public abstract class TextureReader : IDisposable
    {
        protected TextureData.Slice[] LevelData;

        public abstract void Read(BinaryReader reader);

        public abstract TextureData GetData();

        public virtual void Dispose() { }
        
        /// <summary>Gets the error message, if any. This explains what went wrong if the file failed to load.</summary>
        public string Error { get; protected set; }
    }
}
