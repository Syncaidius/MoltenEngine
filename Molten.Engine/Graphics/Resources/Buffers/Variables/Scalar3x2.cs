namespace Molten.Graphics;

/// <summary>A shader matrix3x2 variable.</summary>
public unsafe class ScalarFloat3x2Variable : GraphicsConstantVariable
{
    Matrix3x2F _value;

    internal ScalarFloat3x2Variable(IConstantBuffer parent, string name)
        : base(parent, name)
    {
        SizeOf = sizeof(float) * (3 * 2);
    }
    public override unsafe void ValueFromPtr(void* ptr)
    {
        _value = *(Matrix3x2F*)ptr;
    }

    public override void Dispose() { }

    public override void Write(byte* pDest)
    {
        ((Matrix3x2F*)pDest)[0] = _value;
    }

    public override object Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = (Matrix3x2F)value;
            DirtyParent();
        }
    }
}
