using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    /// <summary>A reader which automatically flips the bits of read types if the current system's endian architecture does not match the stream's endian format.</summary>
    public class BinaryEndianAgnosticReader : BinaryReader
    {
        bool _flipBytes;
        byte[] _flipBuffer;

        public BinaryEndianAgnosticReader(Stream stream, bool dataIsLittleEndian = true) : base(stream)
        {
            _flipBytes = BitConverter.IsLittleEndian != dataIsLittleEndian;
            _flipBuffer = new byte[8];
        }

        public override short ReadInt16()
        {
            return BitConverter.ToInt16(ReadReverse(2), 6);
        }

        public override int ReadInt32()
        {
            return BitConverter.ToInt32(ReadReverse(4), 4);
        }

        public override long ReadInt64()
        {
            return BitConverter.ToInt64(ReadReverse(8), 0);
        }

        public override ushort ReadUInt16()
        {
            return BitConverter.ToUInt16(ReadReverse(2), 6);
        }

        public override uint ReadUInt32()
        {
            return BitConverter.ToUInt32(ReadReverse(4), 4);
        }

        public override ulong ReadUInt64()
        {
            return BitConverter.ToUInt64(ReadReverse(8), 0);
        }

        private byte[] ReadReverse(int count)
        {
            base.Read(_flipBuffer, 0, count);
            Array.Reverse(_flipBuffer);
            return _flipBuffer;
        }
    }
}
