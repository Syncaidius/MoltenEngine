using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class ShaderVariableInfo : EngineObject
    {
        internal ID3D11ShaderReflectionVariable* Variable;

        internal ShaderTypeDesc* TypeDesc;

        internal ShaderVariableDesc* Desc;

        internal ID3D11ShaderReflectionType* Type;

        internal ShaderVariableInfo(ID3D11ShaderReflectionVariable* variable)
        {
            Variable = variable;

            Desc = EngineUtil.Alloc<ShaderVariableDesc>();
            Variable->GetDesc(Desc);

            Type = Variable->GetType();
            TypeDesc = EngineUtil.Alloc<ShaderTypeDesc>();
            Type->GetDesc(TypeDesc);
        }

        protected override void OnDispose()
        {
            GC.SuppressFinalize(this);

            EngineUtil.Free(ref TypeDesc);
            EngineUtil.Free(ref Desc);
        }
    }
}
