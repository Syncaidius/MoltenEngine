using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface IComputeShader : IShader
    {
        /// <summary>Gets a shader texture sampler by its name. Returns null if no match was found.</summary>
        /// <param name="name">The name of the sampler to retrieve.</param>
        IShaderSampler GetSampler(string name);

        /// <summary>Gets the thread group size of the sub-effect that will be applied during dispatch. 
        /// This is defined by <see cref="numthreads"/> in a sub-effect's HLSL code.</summary>
        IntVector3 ThreadGroupSize { get; }
    }
}
