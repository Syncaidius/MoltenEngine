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

        /// <summary>Reads an array and flips the byte order of all it's elements.</summary>
        /// <typeparam name="T">The type of element in the array.</typeparam>
        /// <param name="count">The number of elements to read.</param>
        /// <returns></returns>
        public override T[] ReadArray<T>(int count)
        {
            if (count == 0)
                return new T[0];

            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than 0");

            int remaining = count * Marshal.SizeOf<T>();
            T[] result = new T[count];
            int blockOffset = 0;

            while (remaining > 0)
            {
                int toRead = Math.Min(_arrayBuffer.Length, remaining);
                BaseStream.Read(_arrayBuffer, 0, toRead);

                Array.Reverse(_arrayBuffer, 0, toRead); // Now the bytes per-element are correct, but the elements themselves are in reversed order.
                Buffer.BlockCopy(_arrayBuffer, 0, result, blockOffset, toRead);

                blockOffset += toRead;
                remaining -= toRead;
            }

            // Reverse result array to flip elements back to correct ordering.
            Array.Reverse(result);

            return result;
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
