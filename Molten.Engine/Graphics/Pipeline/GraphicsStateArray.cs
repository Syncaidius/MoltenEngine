namespace Molten.Graphics;

public class GraphicsStateArray<T> 
{
    public T[] Items;

    internal GraphicsStateArray(uint capacity)
    {
        Items = new T[capacity];
    }

    public void Reset(T defaultValue = default)
    {
        for (int i = 0; i < Items.Length; i++)
            Items[i] = defaultValue;

        IsDirty = true;
    }

    public void CopyTo(GraphicsStateArray<T> target)
    {
        Array.Copy(Items, target.Items, Items.Length);
    }

    public int Length => Items.Length;

    public bool IsDirty { get; set; }

    public T this[int index]
    {
        get => Items[index];
        set
        {
            Items[index] = value;
            IsDirty = true;
        }
    }

    public T this[uint index]
    {
        get => Items[index];
        set
        {
            Items[index] = value;
            IsDirty = true;
        }
    }
}
