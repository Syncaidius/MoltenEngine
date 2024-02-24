using System.Runtime.InteropServices;

namespace Molten.Graphics;

[StructLayout(LayoutKind.Explicit)]
public struct ShaderBindPoint<T> : IEquatable<ulong>, IEquatable<ShaderBindPoint>, IEquatable<ShaderBindPoint<T>>
{
    /// <summary>
    /// The mask which includes the bind point, space and type as a single value.
    /// </summary>
    [FieldOffset(0)]
    public ulong Mask;

    /// <summary>
    /// The bind point.
    /// </summary>
    [FieldOffset(0)]
    public uint BindPoint;

    /// <summary>
    /// The bind space for the current bind point.
    /// </summary>
    [FieldOffset(4)]
    public uint Space;

    /// <summary>
    /// The binding object.
    /// </summary>
    [FieldOffset(8)]
    public T Object;

    public ShaderBindPoint(uint bindPoint, uint bindSpace, T binding)
    {
        BindPoint = bindPoint;
        Space = bindSpace;
        Object = binding;
    }

    public bool Equals(ShaderBindPoint other)
    {
        return Mask == other.Mask;
    }

    public bool Equals(ShaderBindPoint<T> other)
    {
        return Mask == other.Mask 
            && Object.Equals(other.Object);
    }

    public bool Equals(ulong other)
    {
        return Mask == other;
    }

    public override bool Equals(object obj) => obj switch
    {
        ShaderBindPoint other => Equals(other),
        ShaderBindPoint<T> other => Equals(other),
        ulong other => Equals(other),
        _ => false,
    };

    public override int GetHashCode()
    {
        return HashCode.Combine(Mask);
    }

    public static bool operator ==(ShaderBindPoint<T> left, ShaderBindPoint<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ShaderBindPoint<T> left, ShaderBindPoint<T> right)
    {
        return !left.Equals(right);
    }

    public static bool operator ==(ShaderBindPoint<T> left, ShaderBindPoint right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ShaderBindPoint<T> left, ShaderBindPoint right)
    {
        return !left.Equals(right);
    }

    public static bool operator ==(ShaderBindPoint left, ShaderBindPoint<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ShaderBindPoint left, ShaderBindPoint<T> right)
    {
        return !left.Equals(right);
    }
}

[StructLayout(LayoutKind.Explicit)]
public struct ShaderBindPoint : IEquatable<ulong>, IEquatable<ShaderBindPoint>
{
    /// <summary>
    /// The mask which includes the bind point, space and type as a single value.
    /// </summary>
    [FieldOffset(0)]
    public ulong Mask;

    /// <summary>
    /// The bind point.
    /// </summary>
    [FieldOffset(0)]
    public uint Point;

    /// <summary>
    /// The bind space for the current bind point.
    /// </summary>
    [FieldOffset(4)]
    public uint Space;

    public ShaderBindPoint(uint bindPoint, uint bindSpace)
    {
        Point = bindPoint;
        Space = bindSpace;
    }

    public bool Equals(ShaderBindPoint other)
    {
        return Mask == other.Mask;
    }

    public bool Equals(ulong other)
    {
        return Mask == other;
    }

    public override bool Equals(object obj) => obj switch
    {
        ShaderBindPoint other => Equals(other),
        ulong other => Equals(other),
        _ => false,
    };

    public override int GetHashCode()
    {
        return Mask.GetHashCode();
    }

    public static bool operator ==(ShaderBindPoint left, ShaderBindPoint right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ShaderBindPoint left, ShaderBindPoint right)
    {
        return !left.Equals(right);
    }
}
