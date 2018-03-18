using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    /// <summary>A reader which automatically flips the bytes of read types if the current system's endian architecture does not match the stream's endian format.</summary>
    public class BinaryEndianAgnosticReader : EnhancedBinaryReader
    {
        bool _flipNeeded;
        byte[] _flipBuffer;
        int[] _decimalBuffer;

        /// <summary>Creates a new instance of <see cref="BinaryEndianAgnosticReader"/>.</summary>
        /// <param name="stream">The stream from which to read data.</param>
        /// <param name="dataIsLittleEndian">Set to false if the expected data should be big-endian. Default value is true (i.e. data is expected to be little-endian).</param>
        /// <param name="leaveOpen">If true, the underlying stream will be left open when the <see cref="BinaryEndianAgnosticReader"/> is disposed.</param>
        /// <param name="encoding">The encoding to use when reading data from the provided stream.</param>
        public BinaryEndianAgnosticReader(Stream stream, Encoding encoding, bool dataIsLittleEndian = true, bool leaveOpen = false) : base(stream, encoding, leaveOpen)
        {
            _flipNeeded = BitConverter.IsLittleEndian != dataIsLittleEndian;
            _flipBuffer = new byte[8];
            _decimalBuffer = new int[4];
        }

        /// <summary>Creates a new instance of <see cref="BinaryEndianAgnosticReader"/>.</summary>
        /// <param name="stream">The stream from which to read data.</param>
        /// <param name="dataIsLittleEndian">Set to false if the expected data should be big-endian. Default value is true (i.e. data is expected to be little-endian).</param>
        /// <param name="leaveOpen">If true, the provided stream will be left open when the <see cref="BinaryEndianAgnosticReader"/> is disposed.</param>
        public BinaryEndianAgnosticReader(Stream stream, bool dataIsLittleEndian = true, bool leaveOpen = false) : base(stream, new UTF8Encoding(), leaveOpen)
        {
            _flipNeeded = BitConverter.IsLittleEndian != dataIsLittleEndian;
            _flipBuffer = new byte[8];
            _decimalBuffer = new int[4];
        }

        public override short ReadInt16()
        {
            if (_flipNeeded)
                return BitConverter.ToInt16(ReadReverse(2), 6);
            else
                return base.ReadInt16();
        }

        public override int ReadInt32()
        {
            if (_flipNeeded)
                return BitConverter.ToInt32(ReadReverse(4), 4);
            else
                return base.ReadInt32();
        }

        public override long ReadInt64()
        {
            if (_flipNeeded)
                return BitConverter.ToInt64(ReadReverse(8), 0);
            else
                return base.ReadInt64();
        }

        public override ushort ReadUInt16()
        {
            if (_flipNeeded)
                return BitConverter.ToUInt16(ReadReverse(2), 6);
            else
                return base.ReadUInt16();
        }

        public override uint ReadUInt32()
        {
            if (_flipNeeded)
                return BitConverter.ToUInt32(ReadReverse(4), 4);
            else
                return base.ReadUInt32();
        }

        public override ulong ReadUInt64()
        {
            if (_flipNeeded)
                return BitConverter.ToUInt64(ReadReverse(8), 0);
            else
                return base.ReadUInt64();
        }

        public override float ReadSingle()
        {
            if (_flipNeeded)
                return BitConverter.ToSingle(ReadReverse(4), 4);
            else
                return base.ReadUInt64();
        }

        public override double ReadDouble()
        {
            if (_flipNeeded)
                return BitConverter.ToSingle(ReadReverse(8), 0);
            else
                return base.ReadDouble();
        }

        public override decimal ReadDecimal()
        {
            if (_flipNeeded)
            {
                _decimalBuffer = new int[4]
                {
                    BitConverter.ToInt32(ReadReverse(4), 4),
                    BitConverter.ToInt32(ReadReverse(4), 4),
                    BitConverter.ToInt32(ReadReverse(4), 4),
                    BitConverter.ToInt32(ReadReverse(4), 4),
                };
                Array.Reverse(_decimalBuffer);
                return new decimal(_decimalBuffer);
            }
            else
            {
                return base.ReadDecimal();
            }
        }

        private byte[] ReadReverse(int count)
        {
            base.Read(_flipBuffer, 0, count);
            Array.Reverse(_flipBuffer);
            return _flipBuffer;
        }
    }
}
