using System.Runtime.InteropServices;

namespace Molten.Graphics;

public struct ShaderBind<T> : IEquatable<ulong>, IEquatable<ShaderBindInfo>, IEquatable<ShaderBind<T>>
{
    public ShaderBindInfo Info;

    public T Object;

    public ShaderBind(uint bindPoint, uint bindSpace, T binding)
    {
        Info.BindPoint = bindPoint;
        Info.BindSpace = bindSpace;
        Object = binding;
    }

    public bool Equals(ShaderBindInfo other)
    {
        return Info.Mask == other.Mask;
    }

    public bool Equals(ShaderBind<T> other)
    {
        return Info.Mask == other.Info.Mask 
            && Object.Equals(other.Object);
    }

    public bool Equals(ulong other)
    {
        return Info.Mask == other;
    }

    public override bool Equals(object obj) => obj switch
    {
        ShaderBindInfo other => Equals(other),
        ShaderBind<T> other => Equals(other),
        ulong other => Equals(other),
        _ => false,
    };

    public override int GetHashCode()
    {
        return HashCode.Combine(Info.Mask);
    }

    public static bool operator ==(ShaderBind<T> left, ShaderBind<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ShaderBind<T> left, ShaderBind<T> right)
    {
        return !left.Equals(right);
    }

    public static bool operator ==(ShaderBind<T> left, ShaderBindInfo right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ShaderBind<T> left, ShaderBindInfo right)
    {
        return !left.Equals(right);
    }

    public static bool operator ==(ShaderBindInfo left, ShaderBind<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ShaderBindInfo left, ShaderBind<T> right)
    {
        return !left.Equals(right);
    }
}

[StructLayout(LayoutKind.Explicit)]
public struct ShaderBindInfo : IEquatable<ulong>, IEquatable<ShaderBindInfo>
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
    public uint BindSpace;

    public ShaderBindInfo(uint bindPoint, uint bindSpace)
    {
        BindPoint = bindPoint;
        BindSpace = bindSpace;
    }

    public bool Equals(ShaderBindInfo other)
    {
        return Mask == other.Mask;
    }

    public bool Equals(ulong other)
    {
        return Mask == other;
    }

    public override bool Equals(object obj) => obj switch
    {
        ShaderBindInfo other => Equals(other),
        ulong other => Equals(other),
        _ => false,
    };

    public override int GetHashCode()
    {
        return Mask.GetHashCode();
    }

    public static bool operator ==(ShaderBindInfo left, ShaderBindInfo right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ShaderBindInfo left, ShaderBindInfo right)
    {
        return !left.Equals(right);
    }
}
