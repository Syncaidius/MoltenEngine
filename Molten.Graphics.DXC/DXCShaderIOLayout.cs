using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Dxc
{
    public class DXCShaderIOLayout : ShaderIOLayout
    {
        public DXCShaderIOLayout(uint elementCount) : base(elementCount) { }

        public DXCShaderIOLayout(ShaderCodeResult result, ShaderType sType, ShaderIOLayoutType type) :
            base(result, sType, type)
        { }

        protected override void BuildVertexElement(ShaderCodeResult result, ShaderIOLayoutType type, ShaderParameterInfo pInfo, GraphicsFormat format, int index)
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
