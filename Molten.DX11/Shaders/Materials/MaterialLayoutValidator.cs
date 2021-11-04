using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class MaterialLayoutValidator
    {
        internal bool Validate(MaterialPassCompileResult pResult)
        {
            bool valid = true;
            MaterialPass pass = pResult.Pass;
            ShaderComposition[] stages = pass.Compositions;
            ShaderComposition previous = null;

            // Stage order reference: https://msdn.microsoft.com/en-us/library/windows/desktop/ff476882(v=vs.85).aspx
            previous = null;

            for (int i = 0; i < stages.Length; i++)
            {
                // Nothing to compare yet, continue.
                if (previous == null)
                {
                    previous = stages[i];
                    continue;
                }

                // No shader to compare. Go to next shader stage.
                if (string.IsNullOrWhiteSpace(stages[i].EntryPoint))
                    continue;

                ShaderIOStructure output = previous.OutputStructure;
                ShaderIOStructure input = stages[i].InputStructure;

                // If the input expects anything, check compatibility. Skip compat check if input does not expect anything (length 0).
                if (input.Elements.Length > 0 && !output.IsCompatible(input))
                {
                    Type currentCompositionType = stages[i].GetType().GenericTypeArguments[0];
                    Type previousCompositionType = previous.GetType().GenericTypeArguments[0];

                    pResult.Errors.Add("Incompatible material I/O structure.");
                    pResult.Errors.Add("====================================");
                    pResult.Errors.Add($"\tFilename: {pass.Material.Filename ?? "N/A"}");
                    pResult.Errors.Add($"\tOutput -- {previousCompositionType.Name}:");

                    if (output.Elements.Length > 0)
                    {
                        for (int o = 0; o < output.Elements.Length; o++)
                            pResult.Errors.Add($"\t\t[{o}] {output.Elements[o].SemanticName} -- index: {output.Elements[o].SemanticIndex}");
                    }
                    else
                    {
                        pResult.Errors.Add("No output elements expected.");
                    }

                    pResult.Errors.Add($"\tInput: {currentCompositionType.Name}:");
                    for (int o = 0; o < input.Elements.Length; o++)
                        pResult.Errors.Add($"\t\t[{o}] {input.Elements[o].SemanticName} -- index: {input.Elements[o].SemanticIndex}");

                    valid = false;
                }

                previous = stages[i];
            }

            return valid && 
                CheckTessellationShaders(pResult) && 
                CheckGeometryTessellationAdjacency(pResult);
        }

        private bool CheckTessellationShaders(MaterialPassCompileResult pResult)
        {
            bool valid = true;
            ShaderReflection hullRef = pResult.Reflections[MaterialPass.ID_HULL];
            ShaderReflection domainRef = pResult.Reflections[MaterialPass.ID_DOMAIN];

            if(hullRef != null && domainRef == null)
            {
                pResult.Errors.Add($"Material pass '{pResult.Pass.Name}' Has a hull shader but no domain shader. Both or neither must be present.");
                valid = false;
            }else if(hullRef == null && domainRef != null)
            {
                pResult.Errors.Add($"Material pass '{pResult.Pass.Name}' Has a domain shader but no hull shader. Both or neither must be present.");
                valid = false;
            }

            return valid;
        }

        private bool CheckGeometryTessellationAdjacency(MaterialPassCompileResult pResult)
        {
            bool valid = true;
            ShaderReflection geometryRef = pResult.Reflections[MaterialPass.ID_GEOMETRY];
            ShaderReflection hullRef = pResult.Reflections[MaterialPass.ID_HULL];
            ShaderReflection domainRef = pResult.Reflections[MaterialPass.ID_DOMAIN];

            if (geometryRef == null || hullRef == null || domainRef == null)
                return valid;

            /* Invalidate if tessellation is active with a geometry shader that expects adjacency data.
                * see: https://msdn.microsoft.com/en-us/library/windows/desktop/ff476340%28v=vs.85%29.aspx
                * quote: "A geometry shader that expects primitives with adjacency (for example, 6 vertices per triangle) is 
                * not valid when tessellation is active (this results in undefined behavior, which the debug layer will complain about)."*/
            valid = pResult.Pass.GeometryPrimitive == InputPrimitive.LineWithAdjacency || 
                pResult.Pass.GeometryPrimitive == InputPrimitive.TriangleWithAdjacency;

            return valid;
        }
    }
}
