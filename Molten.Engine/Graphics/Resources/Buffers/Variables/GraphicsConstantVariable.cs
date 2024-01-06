namespace Molten.Graphics;

public unsafe abstract class GraphicsConstantVariable : ShaderVariable, IDisposable
{
    /// <summary>Gets the byte offset of the variable.</summary>
    public uint ByteOffset;

    /// <summary>The size of the variable's data in bytes.</summary>
    public uint SizeOf;

    protected GraphicsConstantVariable(IConstantBuffer parent, string name)
    {
        ParentBuffer = parent;
        Name = name;
    }

    public static bool AreEqual(GraphicsConstantVariable[] bufferA, GraphicsConstantVariable[] bufferB)
    {
        if (bufferA == null)
            throw new ArgumentNullException("a");

        if (bufferB == null)
            throw new ArgumentNullException("b");


        if (bufferA.Length != bufferB.Length)
            return false;

        for (int i = 0; i < bufferA.Length; i++)
        {
            GraphicsConstantVariable a = bufferA[i];
            GraphicsConstantVariable b = bufferB[i];

            // If any variable is different, then the constant buffers are not equal.
            if (a.GetType() != b.GetType() ||
                a.Name != b.Name ||
                a.ByteOffset != b.ByteOffset ||
                a.SizeOf != b.SizeOf)
            {
                return false;
            }
        }

        return true;
    }

    public abstract void ValueFromPtr(void* ptr);

    public abstract void Dispose();

    /// <summary>Marks the parent buffer as dirty.</summary>
    protected void DirtyParent()
    {
        ParentBuffer.IsDirty = true;
    }

    /// <summary>Called when the variable's value needs to be written to a buffer.</summary>
    /// <param name="pDest">A pointer within the parent <see cref="IConstantBuffer"/> to write the value of the current <see cref="GraphicsConstantVariable"/>.</param>
    public abstract void Write(byte* pDest);

    /// <summary>Gets the <see cref="IConstantBuffer"/> which owns the variable.</summary>
    internal IConstantBuffer ParentBuffer { get; private set; }
}
