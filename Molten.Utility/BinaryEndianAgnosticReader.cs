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
    public class BinaryEndianAgnosticReader : BinaryReader
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

        /// <summary>Reads a pascal string from the underlying stream. Pascal streams store their length in the first byte, followed by the string data, up to a maximum length of 255 bytes.</summary>
        /// <returns></returns>
        public string ReadPascalString()
        {
            byte size = base.ReadByte();
            byte[] body = base.ReadBytes(size);
            return Encoding.ASCII.GetString(body);
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

        public ushort[] ReadArrayUInt16(int count)
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
        public void ReadArrayUInt16(uint[] dest, int count)
        {
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than 0");

            for (int i = 0; i < count; i++)
                dest[i] = ReadUInt16();
        }

        public short[] ReadArrayInt16(int count)
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
        public void ReadArrayInt16(int[] dest, int count)
        {
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than 0");

            for (int i = 0; i < count; i++)
                dest[i] = ReadUInt16();
        }

        public uint[] ReadArrayUInt32(int count)
        {
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than 0");

            uint[] r = new uint[count];
            for (int i = 0; i < count; i++)
                r[i] = ReadUInt32();

            return r;
        }

        public int[] ReadArrayInt32(int count)
        {
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than 0");

            int[] r = new int[count];
            for (int i = 0; i < count; i++)
                r[i] = ReadInt32();

            return r;
        }

        public sbyte[] ReadArraySByte(int count)
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

        /// <summary>Gets or sets the position of the underlying <see cref="Stream"/>.</summary>
        public long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }
    }
}
