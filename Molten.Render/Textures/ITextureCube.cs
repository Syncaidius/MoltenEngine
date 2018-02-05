using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface ITextureCube : ITexture
    {
        void Resize(int newWidth, int newHeight);

        /// <summary>Gets the height of the texture.</summary>
        int Height { get; }
    }
}
