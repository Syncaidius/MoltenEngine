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

        /// <summary>
        /// Changes the current texture's dimensions and format.
        /// </summary>
        /// <param name="newWidth">The new width.</param>
        /// <param name="newHeight">The new height.</param>
        /// <param name="newMipMapCount">The new mip-map level count.</param>
        /// <param name="newArraySize">The new array size. Anything greater than 1 will convert the texture into a texture array. Texture arrays can be treated as standard 2D texture.</param>
        /// <param name="newFormat">The new graphics format.</param>
        void Resize(int newWidth, int newHeight, int newMipMapCount, int newArraySize, GraphicsFormat newFormat);

        /// <summary>
        /// Changes the current texture's width and height.
        /// </summary>
        /// <param name="newWidth">The new width.</param>
        /// <param name="newHeight">The new height.</param>
        void Resize(int newWidth, int newHeight);

        /// <summary>Gets the height of the texture.</summary>
        int Height { get; }
    }
}
