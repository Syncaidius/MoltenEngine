using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface ISwapChainSurface : IRenderSurface
    {
        void Present();

        /// <summary>Gets or sets the color that the swap chain surface is cleared before being rendered to for presentation.</summary>
        Color PresentClearColor { get; set; }
    }
}
