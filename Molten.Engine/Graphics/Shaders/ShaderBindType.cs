namespace Molten.Graphics;
public enum ShaderBindType : ushort
{
    ConstantBuffer = 0,

    Resource = 1,

    UnorderedAccess = 2,

    Sampler = 3,
}
