using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal interface ITextureChange
    {
        void Process(PipeDX11 pipe, TextureBase texture);
    }
}
