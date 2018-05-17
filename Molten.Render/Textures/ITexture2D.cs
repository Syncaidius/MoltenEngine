using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface ITexture2D : ITexture
    {
        void Resize(int newWidth, int newHeight, int newMipMapCount);

        void Resize(int newWidth, int newHeight, int newMipMapCount, int newArraySize, GraphicsFormat newFormat);

        /// <summary>Gets the height of the texture.</summary>
        int Height { get; }
    }
}
