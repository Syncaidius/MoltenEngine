// Converted to C# by James Yarwood.
// MIT License.

//-------------------------------------------------------------------------------------
// BC4BC5.cpp
//  
// Block-compression (BC) functionality for BC4 and BC5 (DirectX 10 texture compression)
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// http://go.microsoft.com/fwlink/?LinkId=248926
//-------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Textures
{
    internal class BC4_UNORM
    {
        public byte red_0;
        public byte red_1;
        public ulong indices; // 6-byte value containing 16 * 3-bit indices (48 bits).

        internal float R(uint uOffset)
        {
            uint uIndex = GetIndex(uOffset);
            return DecodeFromIndex(uIndex);
        }

        internal float DecodeFromIndex(uint uIndex)
        {
            if (uIndex == 0)
                return red_0 / 255.0f;
            if (uIndex == 1)
                return red_1 / 255.0f;
            float fred_0 = red_0 / 255.0f;
            float fred_1 = red_1 / 255.0f;
            if (red_0 > red_1)
            {
                uIndex -= 1;
                return (fred_0 * (7 - uIndex) + fred_1 * uIndex) / 7.0f;
            }
            else
            {
                if (uIndex == 6)
                    return 0.0f;
                if (uIndex == 7)
                    return 1.0f;
                uIndex -= 1;
                return (fred_0 * (5 - uIndex) + fred_1 * uIndex) / 5.0f;
            }
        }

        internal uint GetIndex(uint uOffset)
        {
            return (uint)((indices >> (3 * (int)uOffset)) & 0x07);
        }

        internal void SetIndex(int uOffset, uint uIndex)
        {
            indices &= ~(0x07UL << (3 * uOffset));
            indices |= ((ulong)uIndex << (3 * uOffset));
        }

        internal void Read(BinaryReader reader)
        {
            red_0 = reader.ReadByte();
            red_1 = reader.ReadByte();

            indices = reader.ReadByte();
            indices += (ulong)reader.ReadByte() << 8;
            indices += (ulong)reader.ReadByte() << 16;
            indices += (ulong)reader.ReadByte() << 24;
            indices += (ulong)reader.ReadByte() << 32;
            indices += (ulong)reader.ReadByte() << 40;
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(red_0);
            writer.Write(red_1);
            byte[] data = BitConverter.GetBytes(indices);
            writer.Write(data, 0, 6);
        }
    }

    internal class BC4_SNORM
    {
        public sbyte red_0;
        public sbyte red_1;
        public ulong indices; // 6-byte value containing 16 * 3-bit indices (48 bits).

        internal float R(uint uOffset)
        {
            uint uIndex = GetIndex(uOffset);
            return DecodeFromIndex(uIndex);
        }

        internal float DecodeFromIndex(uint uIndex)
        {
            sbyte sred_0 = (sbyte)((red_0 == -128) ? -127 : red_0);
            sbyte sred_1 = (sbyte)((red_1 == -128) ? -127 : red_1);

            if (uIndex == 0)
                return sred_0 / 127.0f;
            if (uIndex == 1)
                return sred_1 / 127.0f;
            float fred_0 = sred_0 / 127.0f;
            float fred_1 = sred_1 / 127.0f;
            if (red_0 > red_1)
            {
                uIndex -= 1;
                return (fred_0 * (7 - uIndex) + fred_1 * uIndex) / 7.0f;
            }
            else
            {
                if (uIndex == 6)
                    return -1.0f;
                if (uIndex == 7)
                    return 1.0f;
                uIndex -= 1;
                return (fred_0 * (5 - uIndex) + fred_1 * uIndex) / 5.0f;
            }
        }

        internal uint GetIndex(uint uOffset)
        {
            return (uint)((indices >> (3 * (int)uOffset)) & 0x07);
        }

        internal void SetIndex(int uOffset, uint uIndex)
        {
            indices &= ~(0x07UL << (3 * uOffset));
            indices |= ((ulong)uIndex << (3 * uOffset));
        }

        internal void Read(BinaryReader reader)
        {
            red_0 = reader.ReadSByte();
            red_1 = reader.ReadSByte();

            indices = reader.ReadByte();
            indices += (ulong)reader.ReadByte() << 8;
            indices += (ulong)reader.ReadByte() << 16;
            indices += (ulong)reader.ReadByte() << 24;
            indices += (ulong)reader.ReadByte() << 32;
            indices += (ulong)reader.ReadByte() << 40;
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(red_0);
            writer.Write(red_1);
            byte[] data = BitConverter.GetBytes(indices);
            writer.Write(data, 0, 6);
        }
    }
}
