namespace Molten.Graphics;

/// <summary>A shader matrix variable.</summary>
public unsafe class ScalarFloat4x4Variable : GraphicsConstantVariable
{
    Matrix4F _value;

    internal ScalarFloat4x4Variable(IConstantBuffer parent, string name)
        : base(parent, name)
    {
        SizeOf = (uint)sizeof(Matrix4F);
    }

    public override unsafe void ValueFromPtr(void* ptr) { }

    public override void Dispose() { }

    public override void Write(byte* pDest)
    {
        ((Matrix4F*)pDest)[0] = _value;
    }

    public override object Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = (Matrix4F)value;
            _value.Transpose();
            DirtyParent();
        }
    }
}
