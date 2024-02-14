namespace Molten.Graphics;

public sealed class ShaderCompileResult : EngineObject
{
    List<Shader> _shaders = new List<Shader>();
    Dictionary<string, Shader> _shadersByName = new Dictionary<string, Shader>();

    internal void AddShader(Shader shader)
    {
        _shaders.Add(shader);
        _shadersByName.Add(shader.Name.ToLower(), shader);
    }

    protected override void OnDispose(bool immediate) { }

    /// <summary>
    /// Gets a <see cref="Shader"/> of the specified name which was built successfully.
    /// </summary>
    /// <param name="shaderName">The name of the shader given to it it via its XML definition.</param>
    /// <returns></returns>
    public Shader this[string shaderName]
    {
        get
        {
            _shadersByName.TryGetValue(shaderName.ToLower(), out Shader shader);
            return shader;
        }
    }

    public Shader this[int index] => _shaders[index];
}
