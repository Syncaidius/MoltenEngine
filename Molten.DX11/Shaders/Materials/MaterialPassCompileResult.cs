using Molten.Graphics.Shaders;
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
            Results = new IDxcResult*[MaterialPass.ShaderTypes.Length];
            Reflections = new ShaderReflection[MaterialPass.ShaderTypes.Length];
        }

        internal IDxcResult*[] Results;

        internal ShaderReflection[] Reflections;

        internal ShaderIOStructure InputStructure;

        internal ShaderIOStructure OutputSructure;

        internal MaterialPass Pass { get; private set; }

        internal IDxcResult* VertexResult => Results[0];

        internal IDxcResult* HullResult => Results[1];

        internal IDxcResult* DomainResult => Results[2];

        internal IDxcResult* GeometryResult => Results[3];

        internal IDxcResult* PixelResult => Results[4];

        internal IDxcResult* VertexReflection => Reflections[0];

        internal IDxcResult* HullReflection => Reflections[1];

        internal IDxcResult* DomainReflection => Reflections[2];

        internal IDxcResult* GeometryReflection => Reflections[3];

        internal IDxcResult* PixelReflection => Reflections[4];

        internal List<string> Errors = new List<string>();

        internal List<string> Messages = new List<string>();
    }
}
