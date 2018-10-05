using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public enum ShaderType
    {
        Unknown = 0,

        PixelShader = 1,

        VertexShader = 2,

        GeometryShader = 3,

        DomainShader = 4,

        ComputeShader = 5,

        HullShader = 6,
    }
}
