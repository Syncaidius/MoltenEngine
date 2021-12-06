using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public enum PipeStageType
    {
        None = 0,

        InputAssembler = 1,

        VertexShader = 2,

        GeometryShader = 3,

        HullShader = 4,

        DomainShader = 5,

        PixelShader = 6,

        ComputeShader = 7,

        OutputMerger = 8,

        StreamOutput = 9,

        MaterialBinder = 10,
    }
}
