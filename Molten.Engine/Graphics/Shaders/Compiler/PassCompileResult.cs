namespace Molten.Graphics;

public unsafe class PassCompileResult
{
    public PassCompileResult(ShaderPass pass)
    {
        Pass = pass;
        Results = new Dictionary<ShaderStageType, ShaderCodeResult>();
    }

    public ShaderCodeResult this[ShaderStageType type]
    {
        get
        {
            if (Results.TryGetValue(type, out ShaderCodeResult result))
                return result;
            else
                return null;
        }

        set => Results[type] = value;
    }

    public Dictionary<ShaderStageType, ShaderCodeResult> Results { get; }

    public ShaderPass Pass { get; }
}
