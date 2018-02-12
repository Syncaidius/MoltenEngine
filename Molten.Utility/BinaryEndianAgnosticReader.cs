﻿using System;
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
        byte[] _flipBuffer16;

        public BinaryEndianAgnosticReader(Stream stream, bool dataIsLittleEndian = true) : base(stream)
        {
            _flipNeeded = BitConverter.IsLittleEndian != dataIsLittleEndian;
            _flipBuffer = new byte[8];
            _flipBuffer16 = new byte[16];
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
                throw new NotImplementedException();
            else
                return base.ReadDecimal();
        }

        private byte[] ReadReverse(int count)
        {
            base.Read(_flipBuffer, 0, count);
            Array.Reverse(_flipBuffer);
            return _flipBuffer;
        }

        private byte[] ReadReverse16(int count)
        {
            base.Read(_flipBuffer16, 0, count);
            Array.Reverse(_flipBuffer16);
            return _flipBuffer16;
        }
    }
}
