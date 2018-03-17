using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class ShaderParseResult
    {
        public List<IShader> Shaders = new List<IShader>();

        public List<string> Errors = new List<string>();

        public List<string> Warnings = new List<string>();
    }
}
