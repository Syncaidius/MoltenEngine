using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;

namespace Molten
{
    [Serializable]
    public struct DeviceID
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
}
