// Converted to C# by James Yarwood.
// MIT License.

//-------------------------------------------------------------------------------------
// BC6HBC7.cpp
//  
// Block-compression (BC) functionality for BC6H and BC7 (DirectX 11 texture compression)
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// http://go.microsoft.com/fwlink/?LinkId=248926
//-------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Textures
{
    internal partial class BC67
    {
        internal class CBits
        {
            internal byte[] m_uBits;
            internal CBits(uint sizeInBytes)
            {
                m_uBits = new byte[sizeInBytes];
            }

            public byte GetBit(ref uint uStartBit)
            {
                Debug.Assert(uStartBit < 128);
                uint uIndex = uStartBit >> 3;
                uint ret = (uint)(m_uBits[uIndex] >> (int)(uStartBit - (uIndex << 3))) & 0x01;
                uStartBit++;
                return (byte)ret;
            }

            public byte GetBits(ref uint uStartBit, uint uNumBits)
            {
                if (uNumBits == 0)
                    return 0;
                Debug.Assert(uStartBit + uNumBits <= 128 && uNumBits <= 8);
                byte ret;
                uint uIndex = uStartBit >> 3;
                uint uBase = uStartBit - (uIndex << 3);
                if (uBase + uNumBits > 8)
                {
                    uint uFirstIndexBits = 8 - uBase;
                    uint uNextIndexBits = uNumBits - uFirstIndexBits;
                    ret = (byte)((m_uBits[uIndex]) >> (int)uBase | ((m_uBits[uIndex + 1]) & ((1 << (int)uNextIndexBits) - 1)) << (int)uFirstIndexBits);
                }
                else
                {
                    ret = (byte)((m_uBits[uIndex] >> (int)uBase) & ((1 << (int)uNumBits) - 1));
                }
                Debug.Assert(ret < (1 << (int)uNumBits));
                uStartBit += uNumBits;
                return ret;
            }

            public void SetBit(ref uint uStartBit, byte uValue)
            {
                Debug.Assert(uStartBit < 128 && uValue < 2);
                uint uIndex = uStartBit >> 3;
                uint uBase = uStartBit - (uIndex << 3);
                m_uBits[uIndex] = (byte)(m_uBits[uIndex] & ~(1 << (int)uBase));
                m_uBits[uIndex] = (byte)(m_uBits[uIndex] | (uValue << (int)uBase));
                uStartBit++;
            }

            public void SetBits(ref uint uStartBit, uint uNumBits, byte uValue)
            {
                if (uNumBits == 0)
                    return;
                Debug.Assert(uStartBit + uNumBits <= 128 && uNumBits <= 8);
                Debug.Assert(uValue < (1 << (int)uNumBits));
                uint uIndex = uStartBit >> 3;
                uint uBase = uStartBit - (uIndex << 3);
                if (uBase + uNumBits > 8)
                {
                    uint uFirstIndexBits = 8 - uBase;
                    uint uNextIndexBits = uNumBits - uFirstIndexBits;
                    m_uBits[uIndex] &= (byte)(~((1 << (int)uFirstIndexBits) - 1) << (int)uBase);
                    m_uBits[uIndex] |= (byte)(uValue << (int)uBase);
                    m_uBits[uIndex + 1] &= (byte)(~((1 << (int)uNextIndexBits) - 1));
                    m_uBits[uIndex + 1] |= (byte)(uValue >> (int)uFirstIndexBits);
                }
                else
                {
                    m_uBits[uIndex] &= (byte)(~(((1 << (int)uNumBits) - 1) << (int)uBase));
                    m_uBits[uIndex] |= (byte)(uValue << (int)uBase);
                }
                uStartBit += uNumBits;
            }

            internal void Read(BinaryReader reader)
            {
                reader.Read(m_uBits, 0, m_uBits.Length);
            }

            internal void Write(BinaryWriter writer)
            {
                writer.Write(m_uBits);
            }
        }
    }
}
