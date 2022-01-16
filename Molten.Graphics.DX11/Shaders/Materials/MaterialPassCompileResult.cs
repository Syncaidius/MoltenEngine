using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D.Compilers;

namespace Molten.Graphics
{
    internal unsafe class MaterialPassCompileResult
    {
        internal MaterialPassCompileResult(MaterialPass pass)
        {
            Pass = pass;
            Results = new HlslCompileResult[MaterialPass.ShaderTypes.Length];
        }

        internal HlslCompileResult[] Results;

        internal ShaderIOStructure InputStructure;

        internal ShaderIOStructure OutputSructure;

        internal MaterialPass Pass { get; private set; }

        internal HlslCompileResult VertexResult => Results[0];

        internal HlslCompileResult HullResult => Results[1];

        internal HlslCompileResult DomainResult => Results[2];

        internal HlslCompileResult GeometryResult => Results[3];

        internal HlslCompileResult PixelResult => Results[4];

        internal List<string> Errors = new List<string>();

        internal List<string> Messages = new List<string>();
    }
}
