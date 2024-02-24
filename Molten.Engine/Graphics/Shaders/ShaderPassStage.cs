namespace Molten.Graphics;

public unsafe class ShaderPassStage : GraphicsObject
{
    public ShaderBindManager Bindings { get; }

    public ShaderIOLayout InputLayout;

    public ShaderIOLayout OutputLayout;

    public string EntryPoint { get; internal set; }

    public ShaderStageType Type { get; internal set; }

    void* _ptrShader;

    internal ShaderPassStage(ShaderPass parentPass, ShaderStageType type) : 
        base(parentPass.Device)
    {
        Bindings = new ShaderBindManager(parentPass.Parent, parentPass.Bindings);
        Pass = parentPass;
        Type = type;
    }


    protected override void OnGraphicsRelease() { }

    /// <summary>
    /// Gets a pointer to the native shader object or bytecode.
    /// </summary>
    public void* PtrShader
    {
        get => _ptrShader;
        internal set => _ptrShader = value;
    }

    /// <summary>
    /// Gets the parent <see cref="ShaderPass"/> that the current <see cref="ShaderPassStage"/> belongs to.
    /// </summary>
    public ShaderPass Pass { get; }
}
