namespace Molten.Graphics;
public struct ShaderBindPoint<T>
{
    public uint BindPoint;

    public uint BindSpace;

    public T Binding;

    public ShaderBindPoint(uint bindPoint, uint bindSpace, T binding)
    {
        BindPoint = bindPoint;
        BindSpace = bindSpace;
        Binding = binding;
    }
}
