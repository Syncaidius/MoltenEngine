using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface ISwapChainSurface : IRenderSurface
    {
        /// <summary>
        /// Called when the <see cref="ISwapChainSurface"/> is ready to be presented.
        /// </summary>
        void Present();

        /// <summary>
        /// Dispatches a callback to be invoked next time the <see cref="ISwapChainSurface"/> is presented on its parent render thread.
        /// </summary>
        /// <param name="callback"></param>
        void Dispatch(Action callback);
    }
}
