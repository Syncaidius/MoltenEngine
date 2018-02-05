using SharpDX.D3DCompiler;
using Molten.Graphics.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class MaterialPassCompileResult
    {
        internal MaterialPassCompileResult(MaterialPass pass)
        {
            Pass = pass;
            Results = new CompilationResult[MaterialPass.ShaderTypes.Length];
            Reflections = new ShaderReflection[MaterialPass.ShaderTypes.Length];
        }

        internal CompilationResult[] Results;

        internal ShaderReflection[] Reflections;

        internal ShaderIOStructure InputStructure;

        internal ShaderIOStructure OutputSructure;

        internal MaterialPass Pass { get; private set; }

        internal CompilationResult VertexResult => Results[0];

        internal CompilationResult HullResult => Results[1];

        internal CompilationResult DomainResult => Results[2];

        internal CompilationResult GeometryResult => Results[3];

        internal CompilationResult PixelResult => Results[4];

        internal ShaderReflection VertexReflection => Reflections[0];

        internal ShaderReflection HullReflection => Reflections[1];

        internal ShaderReflection DomainReflection => Reflections[2];

        internal ShaderReflection GeometryReflection => Reflections[3];

        internal ShaderReflection PixelReflection => Reflections[4];

        internal List<string> Errors = new List<string>();

        internal List<string> Warnings = new List<string>();

        internal bool HasCommonConstants = false;

        internal bool HasObjectConstants = false;
    }
}
