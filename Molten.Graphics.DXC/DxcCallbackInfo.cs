using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Dxc
{
    public unsafe delegate void* DxcBuildShaderCallback(HlslPass parent, ShaderType type, void* byteCode, nuint numBytes);

    public class DxcCallbackInfo
    {
        public DxcBuildShaderCallback BuildShader;
    }
}
