using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface IRenderSurface
    {        
        /// <summary>Clears the provided <see cref="IRenderSurface2D"/> with the specified color.</summary>
        /// <param name="surface">The surface.</param>
        /// <param name="color">The color to use for clearing the surface.</param>
        void Clear(Color color);
    }
}
