using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class TextureSet<T> : ITextureChange where T: struct
    {
        public int MipLevel;
        public T[] Data;
        public int StartIndex;
        public int Pitch;
        public int ArrayIndex;

        public int Count;
        public int Stride;

        public void Process(GraphicsPipe pipe, TextureBase texture)
        {
            texture.SetDataInternal<T>(pipe, Data, StartIndex, Count, Stride, MipLevel, ArrayIndex, Pitch);
        }
    }
}
