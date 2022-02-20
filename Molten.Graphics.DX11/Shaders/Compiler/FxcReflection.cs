using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public unsafe class FxcReflection : IDisposable
    {
        internal ID3D11ShaderReflection* Ptr;

        internal ShaderDesc Desc;

        internal HlslInputBindDescription[] BindDescs;

        internal FxcReflection(ID3D11ShaderReflection* reflection)
        {
            Ptr = reflection;
            Ptr->GetDesc(ref Desc);

            BindDescs = new HlslInputBindDescription[Desc.BoundResources];

            for (uint rIndex = 0; rIndex < Desc.BoundResources; rIndex++)
                BindDescs[rIndex] = new HlslInputBindDescription(reflection, rIndex);
        }

        public void Dispose()
        {
            foreach (HlslInputBindDescription d in BindDescs)
                d.Dispose();

            SilkUtil.ReleasePtr(ref Ptr);
        }
    }
}
