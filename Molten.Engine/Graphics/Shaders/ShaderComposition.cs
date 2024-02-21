namespace Molten.Graphics;

public unsafe class ShaderComposition : GraphicsObject
{
    /// <summary>A list of const buffers the shader stage requires to be bound.</summary>
    public List<uint> ConstBufferIds = new();

    /// <summary>A list of resources that must be bound to the shader stage.</summary>
    public List<uint> ResourceIds = new();

    /// <summary>A list of samplers that must be bound to the shader stage.</summary>
    public ShaderSampler[] Samplers = [];

    /// <summary>A list of static samplers that should be part of the root signature.</summary>
    public ShaderSampler[] StaticSamplers = [];

    public List<uint> UnorderedAccessIds = new();

    public ShaderIOLayout InputLayout;

    public ShaderIOLayout OutputLayout;

    public string EntryPoint { get; internal set; }

    public ShaderType Type { get; internal set; }

    void* _ptrShader;

    internal ShaderComposition(ShaderPass parentPass, ShaderType type) : 
        base(parentPass.Device)
    {
        Pass = parentPass;
        Type = type;
    }

    protected override void OnGraphicsRelease() { }

    public void* PtrShader
    {
        get => _ptrShader;
        internal set => _ptrShader = value;
    }

    public ShaderPass Pass { get; }
}
