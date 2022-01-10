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

        internal ShaderVariableDesc* Description;

        internal ID3D11ShaderReflectionType* Type;

        internal ShaderVariableInfo(ID3D11ShaderReflectionVariable* variable)
        {
            Variable = variable;
            Variable->GetDesc(Description);
            Type = Variable->GetType();
            Type->GetDesc(TypeDesc);
        }

        protected override void OnDispose()
        {
            // TODO release pointers. They currently have no release method.
        }
    }
}
