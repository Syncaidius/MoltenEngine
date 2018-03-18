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
    public class FlippedBinaryReader : EnhancedBinaryReader
    {
        byte[] _flipBuffer;
        int[] _decimalBuffer;

        public FlippedBinaryReader(Stream input) : base(input)
        {
            _flipBuffer = new byte[8];
            _decimalBuffer = new int[4];
        }

        public FlippedBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
        {
            _flipBuffer = new byte[8];
            _decimalBuffer = new int[4];
        }

        public FlippedBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
            _flipBuffer = new byte[8];
            _decimalBuffer = new int[4];
        }

        public FlippedBinaryReader(Stream input, bool leaveOpen) : base(input, leaveOpen)
        {
            _flipBuffer = new byte[8];
            _decimalBuffer = new int[4];
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

        public override float ReadSingle()
        {
            return BitConverter.ToSingle(ReadReverse(4), 4);
        }

        public override double ReadDouble()
        {
            return BitConverter.ToSingle(ReadReverse(8), 0);
        }

        public override ushort[] ReadArrayUInt16(int count)
        {
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than 0");

            ushort[] r = new ushort[count];
            for (int i = 0; i < count; i++)
                r[i] = ReadUInt16();

            return r;
        }

        /// <summary>
        /// Reads an array of <see cref="ushort"/> into a <see cref="uint"/> array.
        /// </summary>
        /// <param name="dest">The destination array.</param>
        /// <param name="count">The number of values to read.</param>
        public override void ReadArrayUInt16(uint[] dest, int count)
        {
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than 0");

            for (int i = 0; i < count; i++)
                dest[i] = ReadUInt16();
        }

        public override short[] ReadArrayInt16(int count)
        {
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than 0");

            short[] r = new short[count];
            for (int i = 0; i < count; i++)
                r[i] = ReadInt16();

            return r;
        }

        /// <summary>
        /// Reads an array of <see cref="short"/> into a <see cref="int"/> array.
        /// </summary>
        /// <param name="dest">The destination array.</param>
        /// <param name="count">The number of values to read.</param>
        public override void ReadArrayInt16(int[] dest, int count)
        {
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than 0");

            for (int i = 0; i < count; i++)
                dest[i] = ReadUInt16();
        }

        public override uint[] ReadArrayUInt32(int count)
        {
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than 0");

            uint[] r = new uint[count];
            for (int i = 0; i < count; i++)
                r[i] = ReadUInt32();

            return r;
        }

        public override int[] ReadArrayInt32(int count)
        {
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than 0");

            int[] r = new int[count];
            for (int i = 0; i < count; i++)
                r[i] = ReadInt32();

            return r;
        }

        public override sbyte[] ReadArraySByte(int count)
        {
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than 0");

            sbyte[] r = new sbyte[count];
            for (int i = 0; i < count; i++)
                r[i] = ReadSByte();

            return r;
        }

        public override decimal ReadDecimal()
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

        private byte[] ReadReverse(int count)
        {
            base.Read(_flipBuffer, 0, count);
            Array.Reverse(_flipBuffer);
            return _flipBuffer;
        }
    }
}
