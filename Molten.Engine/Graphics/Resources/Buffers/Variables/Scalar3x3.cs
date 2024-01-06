namespace Molten.Graphics;

/// <summary>A shader matrix variable.</summary>
public unsafe class ScalarFloat3x3Variable : GraphicsConstantVariable
{
    Matrix3F _value;

    internal ScalarFloat3x3Variable(IConstantBuffer parent, string name) : 
        base(parent, name)
    {
        SizeOf = (uint)sizeof(Matrix3F);
    }

    public override unsafe void ValueFromPtr(void* ptr)
    {
        _value = *(Matrix3F*)ptr;
        _value.Transpose();
    }

    public override void Dispose() { }

    public override void Write(byte* pDest)
    {
        ((Matrix3F*)pDest)[0] = _value;
    }

    public override object Value
    {
        get => _value;

        set
        {
            _value = (Matrix3F)value;
            _value.Transpose();
            DirtyParent();
        }
    }
}
