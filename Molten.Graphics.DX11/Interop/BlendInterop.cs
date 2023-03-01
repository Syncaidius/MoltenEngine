using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal static class BlendInterop
    {
        public static Blend ToApi(this BlendType type)
        {
            if (type == BlendType.BlendFactorAlpha || type == BlendType.InvBlendFactorAlpha)
                throw new NotSupportedException("alpha blend-factor mode is not supported in dX11");

            // All other values match DX11.
            return (Blend)type;
        }
    }
}
