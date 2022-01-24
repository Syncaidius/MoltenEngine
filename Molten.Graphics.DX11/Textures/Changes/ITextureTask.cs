using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal interface ITextureTask
    {
        /// <summary>
        /// Processes the texture change request. Returns true if the parent texture was altered.
        /// </summary>
        /// <param name="pipe"></param>
        /// <param name="texture"></param>
        /// <returns></returns>
        bool Process(PipeDX11 pipe, TextureBase texture);
    }
}
