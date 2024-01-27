namespace Molten.Graphics;

public class ShaderLayoutValidator
{
    static readonly ShaderType[] _validationIndex = [
            ShaderType.Vertex,
            ShaderType.Hull,
            ShaderType.Domain,
            ShaderType.Geometry,
            ShaderType.Pixel];

    public bool Validate(ShaderCompilerContext context,
        PassCompileResult pResult)
    {
        // Stage order reference: https://msdn.microsoft.com/en-us/library/windows/desktop/ff476882(v=vs.85).aspx
        bool valid = true;
        ShaderComposition prevStage = null;
        ShaderComposition curStage = null;

        for (int i = 0; i < _validationIndex.Length; i++)
        {
            // Nothing to compare yet, continue.
            if (prevStage == null)
            {
                prevStage = pResult.Pass[_validationIndex[i]];
                continue;
            }

            // No shader to compare. Go to next shader stage.
            curStage = pResult.Pass[_validationIndex[i]];
            if (curStage == null)
                continue;

            ShaderIOLayout output = prevStage.OutputLayout;
            ShaderIOLayout input = curStage.InputLayout;

            // If the input expects anything, check compatibility. Skip compat check if input does not expect anything (length 0).
            if (input.Metadata.Length > 0 && !output.IsCompatible(input))
            {
                ShaderType currentCompositionType =  curStage.Type;
                ShaderType previousCompositionType = prevStage.Type;

                context.AddError("Incompatible shader I/O structure.");
                context.AddError("====================================");
                context.AddError($"\tFilename: {pResult.Pass.Parent.Filename ?? "N/A"}");
                context.AddError($"\tOutput -- {previousCompositionType}:");

                if (output.Metadata.Length > 0)
                {
                    for (int o = 0; o < output.Metadata.Length; o++)
                    {
                        ref ShaderIOLayout.ElementMetadata meta = ref output.Metadata[o];
                        string name = meta.SystemValueType != ShaderSVType.Undefined ? $"SV_{meta.SystemValueType}" : meta.Name;
                        context.AddError($"\t\t[{o}] {name} -- index: {meta.SemanticIndex}");
                    }
                }
                else
                {
                    context.AddError("No output elements expected.");
                }

                context.AddError($"\tInput -- {currentCompositionType}:");
                for (int o = 0; o < input.Metadata.Length; o++)
                {
                    ref ShaderIOLayout.ElementMetadata meta = ref input.Metadata[o];
                    string name = meta.SystemValueType != ShaderSVType.Undefined ? $"SV_{meta.SystemValueType}" : meta.Name;
                    context.AddError($"\t\t[{o}] {name} -- index: {meta.SemanticIndex}");
                }

                valid = false;
            }

            prevStage = curStage;
        }

        return valid &&
            CheckTessellationShaders(context, pResult) &&
            CheckGeometryTessellationAdjacency(pResult);
    }

    private bool CheckTessellationShaders(
        ShaderCompilerContext context,
        PassCompileResult pResult)
    {
        bool valid = true;
        ShaderCodeResult hs = pResult[ShaderType.Hull];
        ShaderCodeResult ds = pResult[ShaderType.Domain];

        if (hs != null && ds == null)
        {
            context.AddError($"Shader pass '{pResult.Pass.Name}' Has a hull shader but no domain shader. Both or neither must be present.");
            valid = false;
        }
        else if (hs == null && ds != null)
        {
            context.AddError($"Shader pass '{pResult.Pass.Name}' Has a domain shader but no hull shader. Both or neither must be present.");
            valid = false;
        }

        return valid;
    }

    private bool CheckGeometryTessellationAdjacency(PassCompileResult pResult)
    {
        bool valid = true;
        ShaderCodeResult geometryRef = pResult[ShaderType.Geometry];
        ShaderCodeResult hullRef = pResult[ShaderType.Hull];
        ShaderCodeResult domainRef = pResult[ShaderType.Domain];

        if (geometryRef == null || hullRef == null || domainRef == null)
            return valid;

        /* Invalidate if tessellation is active with a geometry shader that expects adjacency data.
            * see: https://msdn.microsoft.com/en-us/library/windows/desktop/ff476340%28v=vs.85%29.aspx
            * quote: "A geometry shader that expects primitives with adjacency (for example, 6 vertices per triangle) is 
            * not valid when tessellation is active (this results in undefined behavior, which the debug layer will complain about)."*/
        HlslPass pass = pResult.Pass;
        valid = pass.GeometryPrimitive == GeometryHullTopology.LineAdjacency ||
            pass.GeometryPrimitive == GeometryHullTopology.TriangleAdjacency;

        return valid;
    }
}
