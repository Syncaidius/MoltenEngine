using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Dxc
{
    public class DXCShaderIOStructure : ShaderIOStructure
    {
        public DXCShaderIOStructure(uint elementCount) : base(elementCount) { }

        public DXCShaderIOStructure(ShaderCodeResult result, ShaderType sType, ShaderIOStructureType type) :
            base(result, sType, type)
        { }

        protected override void BuildVertexElement(ShaderCodeResult result, ShaderIOStructureType type, ShaderParameterInfo pInfo, GraphicsFormat format, int index)
        {
            //throw new NotImplementedException();
        }

        protected override void Initialize(uint vertexElementCount)
        {
            //throw new NotImplementedException();
        }

        protected override void OnDispose()
        {
            //throw new NotImplementedException();
        }
    }
}
