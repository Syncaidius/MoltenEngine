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
    public class EnhancedBinaryReader : BinaryReader
    {
        public EnhancedBinaryReader(Stream input) : base(input) { }

        public EnhancedBinaryReader(Stream input, Encoding encoding) : base(input, encoding) { }

        public EnhancedBinaryReader(Stream input, bool leaveOpen) : base(input, new UTF8Encoding(), leaveOpen) { }

        public EnhancedBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen) { }

        /// <summary>Reads a pascal string from the underlying stream. Pascal streams store their length in the first byte, followed by the string data, up to a maximum length of 255 bytes.</summary>
        /// <returns></returns>
        public string ReadPascalString()
        {
            byte size = base.ReadByte();
            byte[] body = base.ReadBytes(size);
            return Encoding.ASCII.GetString(body);
        }

        public virtual ushort[] ReadArrayUInt16(int count)
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
        public virtual void ReadArrayUInt16(uint[] dest, int count)
        {
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than 0");

            for (int i = 0; i < count; i++)
                dest[i] = ReadUInt16();
        }

        public virtual short[] ReadArrayInt16(int count)
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
        public virtual void ReadArrayInt16(int[] dest, int count)
        {
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than 0");

            for (int i = 0; i < count; i++)
                dest[i] = ReadUInt16();
        }

        public virtual uint[] ReadArrayUInt32(int count)
        {
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than 0");

            uint[] r = new uint[count];
            for (int i = 0; i < count; i++)
                r[i] = ReadUInt32();

            return r;
        }

        public virtual int[] ReadArrayInt32(int count)
        {
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than 0");

            int[] r = new int[count];
            for (int i = 0; i < count; i++)
                r[i] = ReadInt32();

            return r;
        }

        public virtual sbyte[] ReadArraySByte(int count)
        {
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than 0");

            sbyte[] r = new sbyte[count];
            for (int i = 0; i < count; i++)
                r[i] = ReadSByte();

            return r;
        }

        /// <summary>Gets or sets the position of the underlying <see cref="Stream"/>.</summary>
        public long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }
    }
}
