namespace Molten.Graphics;

public class ShaderCompilerContext
{
    /// <summary>
    /// HLSL shader objects stored by entry-point name
    /// </summary>
    public Dictionary<string, ShaderCodeResult> Shaders { get; } 

    internal ShaderCompileResult Result { get; }

    internal IReadOnlyList<ShaderCompilerMessage> Messages => _messages;

    public bool HasErrors { get; private set; }

    public ShaderSource Source { get; set; }

    public ShaderCompileFlags Flags { get; set; }

    public ShaderStageType Type { get; set; }

    public ShaderCompiler Compiler { get; }

    public string EntryPoint { get; internal set; }

    List<ShaderCompilerMessage> _messages;
    Dictionary<Type, Dictionary<string, object>> _resources;

    public ShaderCompilerContext(ShaderCompiler compiler)
    {
        _messages = new List<ShaderCompilerMessage>();
        _resources = new Dictionary<Type, Dictionary<string, object>>();
        Shaders = new Dictionary<string, ShaderCodeResult>();
        Result = new ShaderCompileResult();
        Compiler = compiler;
    }

    public void AddResource<T>(string name, T resource) 
        where T : IGraphicsResource
    {
        if (!_resources.TryGetValue(typeof(T), out Dictionary<string, object> lookup))
        {
            lookup = new Dictionary<string, object>();
            _resources.Add(typeof(T), lookup);
        }

        lookup.Add(name, resource);
    }

    public bool TryGetResource<T>(string name, out T resource)
        where T : class, IGraphicsResource
    {
        if (!_resources.TryGetValue(typeof(T), out Dictionary<string, object> lookup))
        {
            lookup = new Dictionary<string, object>();
            _resources.Add(typeof(T), lookup);
        }

        object result = default(T);
        if(lookup.TryGetValue(name, out result))
        {
            resource = result as T;
            return true;
        }

        resource = default(T);
        return false;
    }

    public void AddMessage(string text, ShaderCompilerMessage.Kind type = ShaderCompilerMessage.Kind.Message)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        _messages.Add(new ShaderCompilerMessage()
        {
            Text = $"[{type}] {text}",
            MessageType = type,
        });

        if (type == ShaderCompilerMessage.Kind.Error)
            HasErrors = true;
    }

    public void AddError(string text)
    {
        AddMessage(text, ShaderCompilerMessage.Kind.Error);
    }

    public void AddDebug(string text)
    {
        AddMessage(text, ShaderCompilerMessage.Kind.Debug);
    }

    public void AddWarning(string text)
    {
        AddMessage(text, ShaderCompilerMessage.Kind.Warning);
    }
}
