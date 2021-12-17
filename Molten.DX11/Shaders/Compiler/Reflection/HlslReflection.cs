using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// Encapsulates HLSL shader-related reflection information.
    /// </summary>
    internal unsafe class HlslReflection : IDisposable
    {
        internal readonly ID3D11ShaderReflection* Ptr;

        internal readonly ShaderDesc* Desc;

        internal readonly HlslInputBindDescription[] BindDescs;

        internal HlslReflection(ID3D11ShaderReflection* reflection)
        {
            Ptr = reflection;
            Ptr->GetDesc(Desc);
            BindDescs = new HlslInputBindDescription[Desc->BoundResources];

            for (uint r = 0; r < Desc->BoundResources; r++)
            {
                ShaderInputBindDesc* bDesc = null;
                Ptr->GetResourceBindingDesc(r, bDesc);
                BindDescs[r] = new HlslInputBindDescription(bDesc);
            }
        }

        public void Dispose()
        {
            Ptr->Release();
        }
    }
}
