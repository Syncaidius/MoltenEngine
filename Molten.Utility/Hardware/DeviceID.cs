using Silk.NET.Core.Native;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Molten;

[Serializable]
public struct DeviceID : IEquatable<DeviceID>
{
    [DataMember]
    public ulong ID;

    public DeviceID(ulong id)
    {
        ID = id;
    }

    public override string ToString()
    {
        return ID.ToString();
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if(obj is DeviceID id)
            return ID == id.ID;

        return false;
    }

    public bool Equals(DeviceID other)
    {
        return ID == other.ID;
    }

    public static bool operator ==(DeviceID left, DeviceID right)
    {
        return left.ID == right.ID;
    }

    public static bool operator !=(DeviceID left, DeviceID right)
    {
        return left.ID != right.ID;
    }

    public static explicit operator ulong(DeviceID id)
    {
        return id.ID;
    }

    public static explicit operator Luid(DeviceID id)
    {
        return new Luid()
        {
            Low = (uint)(id.ID & 0xFFFFFFFF),
            High = (int)(id.ID >> 32)
        };
    }

    public static explicit operator DeviceID(Luid id)
    {
        return new DeviceID() { ID = (id.Low | (uint)(id.High << 32)) };
    }
}
