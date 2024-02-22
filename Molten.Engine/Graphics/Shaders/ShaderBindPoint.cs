using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Molten.Graphics;

[StructLayout(LayoutKind.Explicit)]
public struct ShaderBindPoint<T> : IEquatable<ulong>, IEquatable<ShaderBindPoint>, IEquatable<ShaderBindPoint<T>>
{
    [FieldOffset(0)]
    public ulong BindMask;

    [FieldOffset(0)]
    public uint BindPoint;

    [FieldOffset(4)]
    public uint BindSpace;

    [FieldOffset(8)]
    public T Object;

    public ShaderBindPoint(uint bindPoint, uint bindSpace, T binding)
    {
        BindPoint = bindPoint;
        BindSpace = bindSpace;
        Object = binding;
    }

    public bool Equals(ShaderBindPoint other)
    {
        return BindMask == other.BindMask;
    }

    public bool Equals(ShaderBindPoint<T> other)
    {
        return BindMask == other.BindMask 
            && Object.Equals(other.Object);
    }

    public bool Equals(ulong other)
    {
        return BindMask == other;
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
        return HashCode.Combine(BindMask);
    }
}

[StructLayout(LayoutKind.Explicit)]
public struct ShaderBindPoint : IEquatable<ulong>, IEquatable<ShaderBindPoint>
{
    [FieldOffset(0)]
    public ulong BindMask;

    [FieldOffset(0)]
    public uint BindPoint;

    [FieldOffset(4)]
    public uint BindSpace;

    public ShaderBindPoint(uint bindPoint, uint bindSpace)
    {
        BindPoint = bindPoint;
        BindSpace = bindSpace;
    }

    public bool Equals(ShaderBindPoint other)
    {
        return BindMask == other.BindMask;
    }

    public bool Equals(ulong other)
    {
        return BindMask == other;
    }

    public override bool Equals(object obj) => obj switch
    {
        ShaderBindPoint other => Equals(other),
        ulong other => Equals(other),
        _ => false,
    };

    public override int GetHashCode()
    {
        return HashCode.Combine(BindMask);
    }
}
