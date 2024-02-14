namespace Molten.Graphics;

/// <summary>
/// A helper class for storing references to common shader properties, or filling missing ones in with dummy properties.
/// </summary>
public abstract class CommonShaderProperties
{
    protected CommonShaderProperties(Shader shader) { }

    protected ShaderVariable MapValue(Shader shader, string name)
    {
        return shader[name] ?? new DummyShaderValue();
    }
}
