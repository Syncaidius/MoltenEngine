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
            Results = new FxcCompileResult[MaterialPass.ShaderTypes.Length];
        }

        internal FxcCompileResult[] Results;

        internal ShaderIOStructure InputStructure;

        internal ShaderIOStructure OutputSructure;

        internal MaterialPass Pass { get; private set; }

        internal FxcCompileResult VertexResult => Results[0];

        internal FxcCompileResult HullResult => Results[1];

        internal FxcCompileResult DomainResult => Results[2];

        internal FxcCompileResult GeometryResult => Results[3];

        internal FxcCompileResult PixelResult => Results[4];
    }
}
