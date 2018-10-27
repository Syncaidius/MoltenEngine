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

using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Textures
{
    // BC6H compression (16 bits per texel)
    internal class D3DX_BC6H : BC67.CBits
    {
        internal class Context : BCContext, IPoolable
        {
            internal INTColor[] aPalette = new INTColor[BC67.BC6H_MAX_INDICES];
            internal INTColor[] aPalette_mapColors = new INTColor[BC67.BC6H_MAX_INDICES];
            internal INTColor[][] aPalette_indices;

            internal INTColor[] aPixels = new INTColor[BC.NUM_PIXELS_PER_BLOCK];
            internal float[] afRoughMSE = new float[BC67.BC6H_MAX_SHAPES];
            internal byte[] auShape = new byte[BC67.BC6H_MAX_SHAPES];

            // RoughMSE
            internal uint[] auPixIdx = new uint[BC.NUM_PIXELS_PER_BLOCK];

            // EndPointsFit
            internal INTColor[] aBits = new INTColor[4];

            internal float[] aOrgErr = new float[BC67.BC6H_MAX_REGIONS];
            internal float[] aOptErr = new float[BC67.BC6H_MAX_REGIONS];
            internal BC67.INTEndPntPair[] aOrgEndPts = new BC67.INTEndPntPair[BC67.BC6H_MAX_REGIONS];
            internal BC67.INTEndPntPair[] aOptEndPts = new BC67.INTEndPntPair[BC67.BC6H_MAX_REGIONS];
            internal uint[] aOrgIdx = new uint[BC.NUM_PIXELS_PER_BLOCK];
            internal uint[] aOptIdx = new uint[BC.NUM_PIXELS_PER_BLOCK];

            internal EncodeParams EP = new EncodeParams();

            internal Context()
            {
                // build list of possibles
                aPalette_indices = new INTColor[BC67.BC6H_MAX_REGIONS][];
                for (int i = 0; i < aPalette_indices.Length; i++)
                    aPalette_indices[i] = new INTColor[BC67.BC6H_MAX_INDICES];
            }

            public void Clear()
            {
                EP.fBestErr = float.MaxValue;
                EP.uShape = 0;
                EP.uMode = 0;
            }
        }

        internal class EncodeParams
        {
            public float fBestErr;
            public bool bSigned;
            public byte uMode;
            public byte uShape;
            public Color4[] aHDRPixels;
            public BC67.INTEndPntPair[][] aUnqEndPts;
            public INTColor[] aIPixels;

            internal EncodeParams()
            {
                aUnqEndPts = new BC67.INTEndPntPair[BC67.BC6H_MAX_SHAPES][];
                for (int i = 0; i < aUnqEndPts.Length; i++)
                    aUnqEndPts[i] = new BC67.INTEndPntPair[BC67.BC6H_MAX_REGIONS];

                aIPixels = new INTColor[BC.NUM_PIXELS_PER_BLOCK];
                fBestErr = float.MaxValue;                
                uMode = 0;
                uShape = 0;
                aHDRPixels = new Color4[BC.NUM_PIXELS_PER_BLOCK];

            }

            internal void Set(Color4[] aOriginal, bool bSignedFormat)
            {
                bSigned = bSignedFormat;
                Array.Copy(aOriginal, 0, aHDRPixels, 0, aHDRPixels.Length);
                for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
                    aIPixels[i].Set(aOriginal[i], bSigned);
            }
        }

        class ModeDesc
        {
            public EField m_eField;
            public byte m_uBit;

            public ModeDesc(EField f, byte bit)
            {
                m_eField = f;
                m_uBit = bit;
            }
        };

        class ModeInfo
        {
            public byte uMode;
            public byte uPartitions;
            public bool bTransformed;
            public byte uIndexPrec;
            public Color[][] RGBAPrec;

            public ModeInfo(byte _uMode, byte _uPartitions, bool _bTransformed, byte _uIndexPrec, Color[][] _rgbaPrec)
            {
                uMode = _uMode;
                uPartitions = _uPartitions;
                bTransformed = _bTransformed;
                uIndexPrec = _uIndexPrec;
                RGBAPrec = _rgbaPrec;
            }
        };

        enum EField : byte
        {
            NA, // N/A
            M,  // Mode
            D,  // Shape
            RW,
            RX,
            RY,
            RZ,
            GW,
            GX,
            GY,
            GZ,
            BW,
            BX,
            BY,
            BZ,
        };

        static readonly ModeDesc[][] ms_aDesc = new ModeDesc[][] // [14][82]
        {
            new ModeDesc[]{   // Mode 1 (0x00) - 10 5 5 5
                new ModeDesc(EField.M, 0), new ModeDesc(EField.M, 1), new ModeDesc(EField.GY, 4), new ModeDesc(EField.BY, 4), new ModeDesc(EField.BZ, 4), new ModeDesc(EField.RW, 0), new ModeDesc(EField.RW, 1), new ModeDesc(EField.RW, 2), new ModeDesc(EField.RW, 3), new ModeDesc(EField.RW, 4),
                new ModeDesc(EField.RW, 5), new ModeDesc(EField.RW, 6), new ModeDesc(EField.RW, 7), new ModeDesc(EField.RW, 8), new ModeDesc(EField.RW, 9), new ModeDesc(EField.GW, 0), new ModeDesc(EField.GW, 1), new ModeDesc(EField.GW, 2), new ModeDesc(EField.GW, 3), new ModeDesc(EField.GW, 4),
                new ModeDesc(EField.GW, 5), new ModeDesc(EField.GW, 6), new ModeDesc(EField.GW, 7), new ModeDesc(EField.GW, 8), new ModeDesc(EField.GW, 9), new ModeDesc(EField.BW, 0), new ModeDesc(EField.BW, 1), new ModeDesc(EField.BW, 2), new ModeDesc(EField.BW, 3), new ModeDesc(EField.BW, 4),
                new ModeDesc(EField.BW, 5), new ModeDesc(EField.BW, 6), new ModeDesc(EField.BW, 7), new ModeDesc(EField.BW, 8), new ModeDesc(EField.BW, 9), new ModeDesc(EField.RX, 0), new ModeDesc(EField.RX, 1), new ModeDesc(EField.RX, 2), new ModeDesc(EField.RX, 3), new ModeDesc(EField.RX, 4),
                new ModeDesc(EField.GZ, 4), new ModeDesc(EField.GY, 0), new ModeDesc(EField.GY, 1), new ModeDesc(EField.GY, 2), new ModeDesc(EField.GY, 3), new ModeDesc(EField.GX, 0), new ModeDesc(EField.GX, 1), new ModeDesc(EField.GX, 2), new ModeDesc(EField.GX, 3), new ModeDesc(EField.GX, 4),
                new ModeDesc(EField.BZ, 0), new ModeDesc(EField.GZ, 0), new ModeDesc(EField.GZ, 1), new ModeDesc(EField.GZ, 2), new ModeDesc(EField.GZ, 3), new ModeDesc(EField.BX, 0), new ModeDesc(EField.BX, 1), new ModeDesc(EField.BX, 2), new ModeDesc(EField.BX, 3), new ModeDesc(EField.BX, 4),
                new ModeDesc(EField.BZ, 1), new ModeDesc(EField.BY, 0), new ModeDesc(EField.BY, 1), new ModeDesc(EField.BY, 2), new ModeDesc(EField.BY, 3), new ModeDesc(EField.RY, 0), new ModeDesc(EField.RY, 1), new ModeDesc(EField.RY, 2), new ModeDesc(EField.RY, 3), new ModeDesc(EField.RY, 4),
                new ModeDesc(EField.BZ, 2), new ModeDesc(EField.RZ, 0), new ModeDesc(EField.RZ, 1), new ModeDesc(EField.RZ, 2), new ModeDesc(EField.RZ, 3), new ModeDesc(EField.RZ, 4), new ModeDesc(EField.BZ, 3), new ModeDesc(EField. D, 0), new ModeDesc(EField. D, 1), new ModeDesc(EField. D, 2),
                new ModeDesc(EField. D, 3), new ModeDesc(EField. D, 4),
            },

            new ModeDesc[]{   // Mode 2 (0x01) - 7 6 6 6
                new ModeDesc(EField.M, 0), new ModeDesc(EField.M, 1), new ModeDesc(EField.GY, 5), new ModeDesc(EField.GZ, 4), new ModeDesc(EField.GZ, 5), new ModeDesc(EField.RW, 0), new ModeDesc(EField.RW, 1), new ModeDesc(EField.RW, 2), new ModeDesc(EField.RW, 3), new ModeDesc(EField.RW, 4),
                new ModeDesc(EField.RW, 5), new ModeDesc(EField.RW, 6), new ModeDesc(EField.BZ, 0), new ModeDesc(EField.BZ, 1), new ModeDesc(EField.BY, 4), new ModeDesc(EField.GW, 0), new ModeDesc(EField.GW, 1), new ModeDesc(EField.GW, 2), new ModeDesc(EField.GW, 3), new ModeDesc(EField.GW, 4),
                new ModeDesc(EField.GW, 5), new ModeDesc(EField.GW, 6), new ModeDesc(EField.BY, 5), new ModeDesc(EField.BZ, 2), new ModeDesc(EField.GY, 4), new ModeDesc(EField.BW, 0), new ModeDesc(EField.BW, 1), new ModeDesc(EField.BW, 2), new ModeDesc(EField.BW, 3), new ModeDesc(EField.BW, 4),
                new ModeDesc(EField.BW, 5), new ModeDesc(EField.BW, 6), new ModeDesc(EField.BZ, 3), new ModeDesc(EField.BZ, 5), new ModeDesc(EField.BZ, 4), new ModeDesc(EField.RX, 0), new ModeDesc(EField.RX, 1), new ModeDesc(EField.RX, 2), new ModeDesc(EField.RX, 3), new ModeDesc(EField.RX, 4),
                new ModeDesc(EField.RX, 5), new ModeDesc(EField.GY, 0), new ModeDesc(EField.GY, 1), new ModeDesc(EField.GY, 2), new ModeDesc(EField.GY, 3), new ModeDesc(EField.GX, 0), new ModeDesc(EField.GX, 1), new ModeDesc(EField.GX, 2), new ModeDesc(EField.GX, 3), new ModeDesc(EField.GX, 4),
                new ModeDesc(EField.GX, 5), new ModeDesc(EField.GZ, 0), new ModeDesc(EField.GZ, 1), new ModeDesc(EField.GZ, 2), new ModeDesc(EField.GZ, 3), new ModeDesc(EField.BX, 0), new ModeDesc(EField.BX, 1), new ModeDesc(EField.BX, 2), new ModeDesc(EField.BX, 3), new ModeDesc(EField.BX, 4),
                new ModeDesc(EField.BX, 5), new ModeDesc(EField.BY, 0), new ModeDesc(EField.BY, 1), new ModeDesc(EField.BY, 2), new ModeDesc(EField.BY, 3), new ModeDesc(EField.RY, 0), new ModeDesc(EField.RY, 1), new ModeDesc(EField.RY, 2), new ModeDesc(EField.RY, 3), new ModeDesc(EField.RY, 4),
                new ModeDesc(EField.RY, 5), new ModeDesc(EField.RZ, 0), new ModeDesc(EField.RZ, 1), new ModeDesc(EField.RZ, 2), new ModeDesc(EField.RZ, 3), new ModeDesc(EField.RZ, 4), new ModeDesc(EField.RZ, 5), new ModeDesc(EField. D, 0), new ModeDesc(EField. D, 1), new ModeDesc(EField. D, 2),
                new ModeDesc(EField. D, 3), new ModeDesc(EField. D, 4),
            },

            new ModeDesc[]{   // Mode 3 (EField.0x02) - 11 5 4 4
                new ModeDesc(EField.M, 0), new ModeDesc(EField.M, 1), new ModeDesc(EField.M, 2), new ModeDesc(EField.M, 3), new ModeDesc(EField.M, 4), new ModeDesc(EField.RW, 0), new ModeDesc(EField.RW, 1), new ModeDesc(EField.RW, 2), new ModeDesc(EField.RW, 3), new ModeDesc(EField.RW, 4),
                new ModeDesc(EField.RW, 5), new ModeDesc(EField.RW, 6), new ModeDesc(EField.RW, 7), new ModeDesc(EField.RW, 8), new ModeDesc(EField.RW, 9), new ModeDesc(EField.GW, 0), new ModeDesc(EField.GW, 1), new ModeDesc(EField.GW, 2), new ModeDesc(EField.GW, 3), new ModeDesc(EField.GW, 4),
                new ModeDesc(EField.GW, 5), new ModeDesc(EField.GW, 6), new ModeDesc(EField.GW, 7), new ModeDesc(EField.GW, 8), new ModeDesc(EField.GW, 9), new ModeDesc(EField.BW, 0), new ModeDesc(EField.BW, 1), new ModeDesc(EField.BW, 2), new ModeDesc(EField.BW, 3), new ModeDesc(EField.BW, 4),
                new ModeDesc(EField.BW, 5), new ModeDesc(EField.BW, 6), new ModeDesc(EField.BW, 7), new ModeDesc(EField.BW, 8), new ModeDesc(EField.BW, 9), new ModeDesc(EField.RX, 0), new ModeDesc(EField.RX, 1), new ModeDesc(EField.RX, 2), new ModeDesc(EField.RX, 3), new ModeDesc(EField.RX, 4),
                new ModeDesc(EField.RW,10), new ModeDesc(EField.GY, 0), new ModeDesc(EField.GY, 1), new ModeDesc(EField.GY, 2), new ModeDesc(EField.GY, 3), new ModeDesc(EField.GX, 0), new ModeDesc(EField.GX, 1), new ModeDesc(EField.GX, 2), new ModeDesc(EField.GX, 3), new ModeDesc(EField.GW,10),
                new ModeDesc(EField.BZ, 0), new ModeDesc(EField.GZ, 0), new ModeDesc(EField.GZ, 1), new ModeDesc(EField.GZ, 2), new ModeDesc(EField.GZ, 3), new ModeDesc(EField.BX, 0), new ModeDesc(EField.BX, 1), new ModeDesc(EField.BX, 2), new ModeDesc(EField.BX, 3), new ModeDesc(EField.BW,10),
                new ModeDesc(EField.BZ, 1), new ModeDesc(EField.BY, 0), new ModeDesc(EField.BY, 1), new ModeDesc(EField.BY, 2), new ModeDesc(EField.BY, 3), new ModeDesc(EField.RY, 0), new ModeDesc(EField.RY, 1), new ModeDesc(EField.RY, 2), new ModeDesc(EField.RY, 3), new ModeDesc(EField.RY, 4),
                new ModeDesc(EField.BZ, 2), new ModeDesc(EField.RZ, 0), new ModeDesc(EField.RZ, 1), new ModeDesc(EField.RZ, 2), new ModeDesc(EField.RZ, 3), new ModeDesc(EField.RZ, 4), new ModeDesc(EField.BZ, 3), new ModeDesc(EField. D, 0), new ModeDesc(EField. D, 1), new ModeDesc(EField. D, 2),
                new ModeDesc(EField. D, 3), new ModeDesc(EField. D, 4),
            },

            new ModeDesc[]{   // Mode 4 (0x06) - 11 4 5 4
                new ModeDesc(EField.M, 0), new ModeDesc(EField.M, 1), new ModeDesc(EField.M, 2), new ModeDesc(EField.M, 3), new ModeDesc(EField.M, 4), new ModeDesc(EField.RW, 0), new ModeDesc(EField.RW, 1), new ModeDesc(EField.RW, 2), new ModeDesc(EField.RW, 3), new ModeDesc(EField.RW, 4),
                new ModeDesc(EField.RW, 5), new ModeDesc(EField.RW, 6), new ModeDesc(EField.RW, 7), new ModeDesc(EField.RW, 8), new ModeDesc(EField.RW, 9), new ModeDesc(EField.GW, 0), new ModeDesc(EField.GW, 1), new ModeDesc(EField.GW, 2), new ModeDesc(EField.GW, 3), new ModeDesc(EField.GW, 4),
                new ModeDesc(EField.GW, 5), new ModeDesc(EField.GW, 6), new ModeDesc(EField.GW, 7), new ModeDesc(EField.GW, 8), new ModeDesc(EField.GW, 9), new ModeDesc(EField.BW, 0), new ModeDesc(EField.BW, 1), new ModeDesc(EField.BW, 2), new ModeDesc(EField.BW, 3), new ModeDesc(EField.BW, 4),
                new ModeDesc(EField.BW, 5), new ModeDesc(EField.BW, 6), new ModeDesc(EField.BW, 7), new ModeDesc(EField.BW, 8), new ModeDesc(EField.BW, 9), new ModeDesc(EField.RX, 0), new ModeDesc(EField.RX, 1), new ModeDesc(EField.RX, 2), new ModeDesc(EField.RX, 3), new ModeDesc(EField.RW,10),
                new ModeDesc(EField.GZ, 4), new ModeDesc(EField.GY, 0), new ModeDesc(EField.GY, 1), new ModeDesc(EField.GY, 2), new ModeDesc(EField.GY, 3), new ModeDesc(EField.GX, 0), new ModeDesc(EField.GX, 1), new ModeDesc(EField.GX, 2), new ModeDesc(EField.GX, 3), new ModeDesc(EField.GX, 4),
                new ModeDesc(EField.GW,10), new ModeDesc(EField.GZ, 0), new ModeDesc(EField.GZ, 1), new ModeDesc(EField.GZ, 2), new ModeDesc(EField.GZ, 3), new ModeDesc(EField.BX, 0), new ModeDesc(EField.BX, 1), new ModeDesc(EField.BX, 2), new ModeDesc(EField.BX, 3), new ModeDesc(EField.BW,10),
                new ModeDesc(EField.BZ, 1), new ModeDesc(EField.BY, 0), new ModeDesc(EField.BY, 1), new ModeDesc(EField.BY, 2), new ModeDesc(EField.BY, 3), new ModeDesc(EField.RY, 0), new ModeDesc(EField.RY, 1), new ModeDesc(EField.RY, 2), new ModeDesc(EField.RY, 3), new ModeDesc(EField.BZ, 0),
                new ModeDesc(EField.BZ, 2), new ModeDesc(EField.RZ, 0), new ModeDesc(EField.RZ, 1), new ModeDesc(EField.RZ, 2), new ModeDesc(EField.RZ, 3), new ModeDesc(EField.GY, 4), new ModeDesc(EField.BZ, 3), new ModeDesc(EField. D, 0), new ModeDesc(EField. D, 1), new ModeDesc(EField. D, 2),
                new ModeDesc(EField. D, 3), new ModeDesc(EField. D, 4),
            },

            new ModeDesc[]{   // Mode 5 (0x0a) - 11 4 4 5
                new ModeDesc(EField.M, 0), new ModeDesc(EField.M, 1), new ModeDesc(EField.M, 2), new ModeDesc(EField.M, 3), new ModeDesc(EField.M, 4), new ModeDesc(EField.RW, 0), new ModeDesc(EField.RW, 1), new ModeDesc(EField.RW, 2), new ModeDesc(EField.RW, 3), new ModeDesc(EField.RW, 4),
                new ModeDesc(EField.RW, 5), new ModeDesc(EField.RW, 6), new ModeDesc(EField.RW, 7), new ModeDesc(EField.RW, 8), new ModeDesc(EField.RW, 9), new ModeDesc(EField.GW, 0), new ModeDesc(EField.GW, 1), new ModeDesc(EField.GW, 2), new ModeDesc(EField.GW, 3), new ModeDesc(EField.GW, 4),
                new ModeDesc(EField.GW, 5), new ModeDesc(EField.GW, 6), new ModeDesc(EField.GW, 7), new ModeDesc(EField.GW, 8), new ModeDesc(EField.GW, 9), new ModeDesc(EField.BW, 0), new ModeDesc(EField.BW, 1), new ModeDesc(EField.BW, 2), new ModeDesc(EField.BW, 3), new ModeDesc(EField.BW, 4),
                new ModeDesc(EField.BW, 5), new ModeDesc(EField.BW, 6), new ModeDesc(EField.BW, 7), new ModeDesc(EField.BW, 8), new ModeDesc(EField.BW, 9), new ModeDesc(EField.RX, 0), new ModeDesc(EField.RX, 1), new ModeDesc(EField.RX, 2), new ModeDesc(EField.RX, 3), new ModeDesc(EField.RW,10),
                new ModeDesc(EField.BY, 4), new ModeDesc(EField.GY, 0), new ModeDesc(EField.GY, 1), new ModeDesc(EField.GY, 2), new ModeDesc(EField.GY, 3), new ModeDesc(EField.GX, 0), new ModeDesc(EField.GX, 1), new ModeDesc(EField.GX, 2), new ModeDesc(EField.GX, 3), new ModeDesc(EField.GW,10),
                new ModeDesc(EField.BZ, 0), new ModeDesc(EField.GZ, 0), new ModeDesc(EField.GZ, 1), new ModeDesc(EField.GZ, 2), new ModeDesc(EField.GZ, 3), new ModeDesc(EField.BX, 0), new ModeDesc(EField.BX, 1), new ModeDesc(EField.BX, 2), new ModeDesc(EField.BX, 3), new ModeDesc(EField.BX, 4),
                new ModeDesc(EField.BW,10), new ModeDesc(EField.BY, 0), new ModeDesc(EField.BY, 1), new ModeDesc(EField.BY, 2), new ModeDesc(EField.BY, 3), new ModeDesc(EField.RY, 0), new ModeDesc(EField.RY, 1), new ModeDesc(EField.RY, 2), new ModeDesc(EField.RY, 3), new ModeDesc(EField.BZ, 1),
                new ModeDesc(EField.BZ, 2), new ModeDesc(EField.RZ, 0), new ModeDesc(EField.RZ, 1), new ModeDesc(EField.RZ, 2), new ModeDesc(EField.RZ, 3), new ModeDesc(EField.BZ, 4), new ModeDesc(EField.BZ, 3), new ModeDesc(EField. D, 0), new ModeDesc(EField. D, 1), new ModeDesc(EField. D, 2),
                new ModeDesc(EField. D, 3), new ModeDesc(EField. D, 4),
            },

            new ModeDesc[]{   // Mode 6 (EField.0x0e) - 9 5 5 5
                new ModeDesc(EField.M, 0), new ModeDesc(EField.M, 1), new ModeDesc(EField.M, 2), new ModeDesc(EField.M, 3), new ModeDesc(EField.M, 4), new ModeDesc(EField.RW, 0), new ModeDesc(EField.RW, 1), new ModeDesc(EField.RW, 2), new ModeDesc(EField.RW, 3), new ModeDesc(EField.RW, 4),
                new ModeDesc(EField.RW, 5), new ModeDesc(EField.RW, 6), new ModeDesc(EField.RW, 7), new ModeDesc(EField.RW, 8), new ModeDesc(EField.BY, 4), new ModeDesc(EField.GW, 0), new ModeDesc(EField.GW, 1), new ModeDesc(EField.GW, 2), new ModeDesc(EField.GW, 3), new ModeDesc(EField.GW, 4),
                new ModeDesc(EField.GW, 5), new ModeDesc(EField.GW, 6), new ModeDesc(EField.GW, 7), new ModeDesc(EField.GW, 8), new ModeDesc(EField.GY, 4), new ModeDesc(EField.BW, 0), new ModeDesc(EField.BW, 1), new ModeDesc(EField.BW, 2), new ModeDesc(EField.BW, 3), new ModeDesc(EField.BW, 4),
                new ModeDesc(EField.BW, 5), new ModeDesc(EField.BW, 6), new ModeDesc(EField.BW, 7), new ModeDesc(EField.BW, 8), new ModeDesc(EField.BZ, 4), new ModeDesc(EField.RX, 0), new ModeDesc(EField.RX, 1), new ModeDesc(EField.RX, 2), new ModeDesc(EField.RX, 3), new ModeDesc(EField.RX, 4),
                new ModeDesc(EField.GZ, 4), new ModeDesc(EField.GY, 0), new ModeDesc(EField.GY, 1), new ModeDesc(EField.GY, 2), new ModeDesc(EField.GY, 3), new ModeDesc(EField.GX, 0), new ModeDesc(EField.GX, 1), new ModeDesc(EField.GX, 2), new ModeDesc(EField.GX, 3), new ModeDesc(EField.GX, 4),
                new ModeDesc(EField.BZ, 0), new ModeDesc(EField.GZ, 0), new ModeDesc(EField.GZ, 1), new ModeDesc(EField.GZ, 2), new ModeDesc(EField.GZ, 3), new ModeDesc(EField.BX, 0), new ModeDesc(EField.BX, 1), new ModeDesc(EField.BX, 2), new ModeDesc(EField.BX, 3), new ModeDesc(EField.BX, 4),
                new ModeDesc(EField.BZ, 1), new ModeDesc(EField.BY, 0), new ModeDesc(EField.BY, 1), new ModeDesc(EField.BY, 2), new ModeDesc(EField.BY, 3), new ModeDesc(EField.RY, 0), new ModeDesc(EField.RY, 1), new ModeDesc(EField.RY, 2), new ModeDesc(EField.RY, 3), new ModeDesc(EField.RY, 4),
                new ModeDesc(EField.BZ, 2), new ModeDesc(EField.RZ, 0), new ModeDesc(EField.RZ, 1), new ModeDesc(EField.RZ, 2), new ModeDesc(EField.RZ, 3), new ModeDesc(EField.RZ, 4), new ModeDesc(EField.BZ, 3), new ModeDesc(EField. D, 0), new ModeDesc(EField. D, 1), new ModeDesc(EField. D, 2),
                new ModeDesc(EField. D, 3), new ModeDesc(EField. D, 4),
            },

            new ModeDesc[]{   // Mode 7 (EField.0x12) - 8 6 5 5
                new ModeDesc(EField.M, 0), new ModeDesc(EField.M, 1), new ModeDesc(EField.M, 2), new ModeDesc(EField.M, 3), new ModeDesc(EField.M, 4), new ModeDesc(EField.RW, 0), new ModeDesc(EField.RW, 1), new ModeDesc(EField.RW, 2), new ModeDesc(EField.RW, 3), new ModeDesc(EField.RW, 4),
                new ModeDesc(EField.RW, 5), new ModeDesc(EField.RW, 6), new ModeDesc(EField.RW, 7), new ModeDesc(EField.GZ, 4), new ModeDesc(EField.BY, 4), new ModeDesc(EField.GW, 0), new ModeDesc(EField.GW, 1), new ModeDesc(EField.GW, 2), new ModeDesc(EField.GW, 3), new ModeDesc(EField.GW, 4),
                new ModeDesc(EField.GW, 5), new ModeDesc(EField.GW, 6), new ModeDesc(EField.GW, 7), new ModeDesc(EField.BZ, 2), new ModeDesc(EField.GY, 4), new ModeDesc(EField.BW, 0), new ModeDesc(EField.BW, 1), new ModeDesc(EField.BW, 2), new ModeDesc(EField.BW, 3), new ModeDesc(EField.BW, 4),
                new ModeDesc(EField.BW, 5), new ModeDesc(EField.BW, 6), new ModeDesc(EField.BW, 7), new ModeDesc(EField.BZ, 3), new ModeDesc(EField.BZ, 4), new ModeDesc(EField.RX, 0), new ModeDesc(EField.RX, 1), new ModeDesc(EField.RX, 2), new ModeDesc(EField.RX, 3), new ModeDesc(EField.RX, 4),
                new ModeDesc(EField.RX, 5), new ModeDesc(EField.GY, 0), new ModeDesc(EField.GY, 1), new ModeDesc(EField.GY, 2), new ModeDesc(EField.GY, 3), new ModeDesc(EField.GX, 0), new ModeDesc(EField.GX, 1), new ModeDesc(EField.GX, 2), new ModeDesc(EField.GX, 3), new ModeDesc(EField.GX, 4),
                new ModeDesc(EField.BZ, 0), new ModeDesc(EField.GZ, 0), new ModeDesc(EField.GZ, 1), new ModeDesc(EField.GZ, 2), new ModeDesc(EField.GZ, 3), new ModeDesc(EField.BX, 0), new ModeDesc(EField.BX, 1), new ModeDesc(EField.BX, 2), new ModeDesc(EField.BX, 3), new ModeDesc(EField.BX, 4),
                new ModeDesc(EField.BZ, 1), new ModeDesc(EField.BY, 0), new ModeDesc(EField.BY, 1), new ModeDesc(EField.BY, 2), new ModeDesc(EField.BY, 3), new ModeDesc(EField.RY, 0), new ModeDesc(EField.RY, 1), new ModeDesc(EField.RY, 2), new ModeDesc(EField.RY, 3), new ModeDesc(EField.RY, 4),
                new ModeDesc(EField.RY, 5), new ModeDesc(EField.RZ, 0), new ModeDesc(EField.RZ, 1), new ModeDesc(EField.RZ, 2), new ModeDesc(EField.RZ, 3), new ModeDesc(EField.RZ, 4), new ModeDesc(EField.RZ, 5), new ModeDesc(EField. D, 0), new ModeDesc(EField. D, 1), new ModeDesc(EField. D, 2),
                new ModeDesc(EField. D, 3), new ModeDesc(EField. D, 4),
            },

            new ModeDesc[]{   // Mode 8 (EField.0x16) - 8 5 6 5
                new ModeDesc(EField.M, 0), new ModeDesc(EField.M, 1), new ModeDesc(EField.M, 2), new ModeDesc(EField.M, 3), new ModeDesc(EField.M, 4), new ModeDesc(EField.RW, 0), new ModeDesc(EField.RW, 1), new ModeDesc(EField.RW, 2), new ModeDesc(EField.RW, 3), new ModeDesc(EField.RW, 4),
                new ModeDesc(EField.RW, 5), new ModeDesc(EField.RW, 6), new ModeDesc(EField.RW, 7), new ModeDesc(EField.BZ, 0), new ModeDesc(EField.BY, 4), new ModeDesc(EField.GW, 0), new ModeDesc(EField.GW, 1), new ModeDesc(EField.GW, 2), new ModeDesc(EField.GW, 3), new ModeDesc(EField.GW, 4),
                new ModeDesc(EField.GW, 5), new ModeDesc(EField.GW, 6), new ModeDesc(EField.GW, 7), new ModeDesc(EField.GY, 5), new ModeDesc(EField.GY, 4), new ModeDesc(EField.BW, 0), new ModeDesc(EField.BW, 1), new ModeDesc(EField.BW, 2), new ModeDesc(EField.BW, 3), new ModeDesc(EField.BW, 4),
                new ModeDesc(EField.BW, 5), new ModeDesc(EField.BW, 6), new ModeDesc(EField.BW, 7), new ModeDesc(EField.GZ, 5), new ModeDesc(EField.BZ, 4), new ModeDesc(EField.RX, 0), new ModeDesc(EField.RX, 1), new ModeDesc(EField.RX, 2), new ModeDesc(EField.RX, 3), new ModeDesc(EField.RX, 4),
                new ModeDesc(EField.GZ, 4), new ModeDesc(EField.GY, 0), new ModeDesc(EField.GY, 1), new ModeDesc(EField.GY, 2), new ModeDesc(EField.GY, 3), new ModeDesc(EField.GX, 0), new ModeDesc(EField.GX, 1), new ModeDesc(EField.GX, 2), new ModeDesc(EField.GX, 3), new ModeDesc(EField.GX, 4),
                new ModeDesc(EField.GX, 5), new ModeDesc(EField.GZ, 0), new ModeDesc(EField.GZ, 1), new ModeDesc(EField.GZ, 2), new ModeDesc(EField.GZ, 3), new ModeDesc(EField.BX, 0), new ModeDesc(EField.BX, 1), new ModeDesc(EField.BX, 2), new ModeDesc(EField.BX, 3), new ModeDesc(EField.BX, 4),
                new ModeDesc(EField.BZ, 1), new ModeDesc(EField.BY, 0), new ModeDesc(EField.BY, 1), new ModeDesc(EField.BY, 2), new ModeDesc(EField.BY, 3), new ModeDesc(EField.RY, 0), new ModeDesc(EField.RY, 1), new ModeDesc(EField.RY, 2), new ModeDesc(EField.RY, 3), new ModeDesc(EField.RY, 4),
                new ModeDesc(EField.BZ, 2), new ModeDesc(EField.RZ, 0), new ModeDesc(EField.RZ, 1), new ModeDesc(EField.RZ, 2), new ModeDesc(EField.RZ, 3), new ModeDesc(EField.RZ, 4), new ModeDesc(EField.BZ, 3), new ModeDesc(EField. D, 0), new ModeDesc(EField. D, 1), new ModeDesc(EField. D, 2),
                new ModeDesc(EField. D, 3), new ModeDesc(EField. D, 4),
            },

            new ModeDesc[]{   // Mode 9 (EField.0x1a) - 8 5 5 6
                new ModeDesc(EField.M, 0), new ModeDesc(EField.M, 1), new ModeDesc(EField.M, 2), new ModeDesc(EField.M, 3), new ModeDesc(EField.M, 4), new ModeDesc(EField.RW, 0), new ModeDesc(EField.RW, 1), new ModeDesc(EField.RW, 2), new ModeDesc(EField.RW, 3), new ModeDesc(EField.RW, 4),
                new ModeDesc(EField.RW, 5), new ModeDesc(EField.RW, 6), new ModeDesc(EField.RW, 7), new ModeDesc(EField.BZ, 1), new ModeDesc(EField.BY, 4), new ModeDesc(EField.GW, 0), new ModeDesc(EField.GW, 1), new ModeDesc(EField.GW, 2), new ModeDesc(EField.GW, 3), new ModeDesc(EField.GW, 4),
                new ModeDesc(EField.GW, 5), new ModeDesc(EField.GW, 6), new ModeDesc(EField.GW, 7), new ModeDesc(EField.BY, 5), new ModeDesc(EField.GY, 4), new ModeDesc(EField.BW, 0), new ModeDesc(EField.BW, 1), new ModeDesc(EField.BW, 2), new ModeDesc(EField.BW, 3), new ModeDesc(EField.BW, 4),
                new ModeDesc(EField.BW, 5), new ModeDesc(EField.BW, 6), new ModeDesc(EField.BW, 7), new ModeDesc(EField.BZ, 5), new ModeDesc(EField.BZ, 4), new ModeDesc(EField.RX, 0), new ModeDesc(EField.RX, 1), new ModeDesc(EField.RX, 2), new ModeDesc(EField.RX, 3), new ModeDesc(EField.RX, 4),
                new ModeDesc(EField.GZ, 4), new ModeDesc(EField.GY, 0), new ModeDesc(EField.GY, 1), new ModeDesc(EField.GY, 2), new ModeDesc(EField.GY, 3), new ModeDesc(EField.GX, 0), new ModeDesc(EField.GX, 1), new ModeDesc(EField.GX, 2), new ModeDesc(EField.GX, 3), new ModeDesc(EField.GX, 4),
                new ModeDesc(EField.BZ, 0), new ModeDesc(EField.GZ, 0), new ModeDesc(EField.GZ, 1), new ModeDesc(EField.GZ, 2), new ModeDesc(EField.GZ, 3), new ModeDesc(EField.BX, 0), new ModeDesc(EField.BX, 1), new ModeDesc(EField.BX, 2), new ModeDesc(EField.BX, 3), new ModeDesc(EField.BX, 4),
                new ModeDesc(EField.BX, 5), new ModeDesc(EField.BY, 0), new ModeDesc(EField.BY, 1), new ModeDesc(EField.BY, 2), new ModeDesc(EField.BY, 3), new ModeDesc(EField.RY, 0), new ModeDesc(EField.RY, 1), new ModeDesc(EField.RY, 2), new ModeDesc(EField.RY, 3), new ModeDesc(EField.RY, 4),
                new ModeDesc(EField.BZ, 2), new ModeDesc(EField.RZ, 0), new ModeDesc(EField.RZ, 1), new ModeDesc(EField.RZ, 2), new ModeDesc(EField.RZ, 3), new ModeDesc(EField.RZ, 4), new ModeDesc(EField.BZ, 3), new ModeDesc(EField. D, 0), new ModeDesc(EField. D, 1), new ModeDesc(EField. D, 2),
                new ModeDesc(EField. D, 3), new ModeDesc(EField. D, 4),
            },

            new ModeDesc[]{   // Mode 10 (EField.0x1e) - 6 6 6 6
                new ModeDesc(EField.M, 0), new ModeDesc(EField.M, 1), new ModeDesc(EField.M, 2), new ModeDesc(EField.M, 3), new ModeDesc(EField.M, 4), new ModeDesc(EField.RW, 0), new ModeDesc(EField.RW, 1), new ModeDesc(EField.RW, 2), new ModeDesc(EField.RW, 3), new ModeDesc(EField.RW, 4),
                new ModeDesc(EField.RW, 5), new ModeDesc(EField.GZ, 4), new ModeDesc(EField.BZ, 0), new ModeDesc(EField.BZ, 1), new ModeDesc(EField.BY, 4), new ModeDesc(EField.GW, 0), new ModeDesc(EField.GW, 1), new ModeDesc(EField.GW, 2), new ModeDesc(EField.GW, 3), new ModeDesc(EField.GW, 4),
                new ModeDesc(EField.GW, 5), new ModeDesc(EField.GY, 5), new ModeDesc(EField.BY, 5), new ModeDesc(EField.BZ, 2), new ModeDesc(EField.GY, 4), new ModeDesc(EField.BW, 0), new ModeDesc(EField.BW, 1), new ModeDesc(EField.BW, 2), new ModeDesc(EField.BW, 3), new ModeDesc(EField.BW, 4),
                new ModeDesc(EField.BW, 5), new ModeDesc(EField.GZ, 5), new ModeDesc(EField.BZ, 3), new ModeDesc(EField.BZ, 5), new ModeDesc(EField.BZ, 4), new ModeDesc(EField.RX, 0), new ModeDesc(EField.RX, 1), new ModeDesc(EField.RX, 2), new ModeDesc(EField.RX, 3), new ModeDesc(EField.RX, 4),
                new ModeDesc(EField.RX, 5), new ModeDesc(EField.GY, 0), new ModeDesc(EField.GY, 1), new ModeDesc(EField.GY, 2), new ModeDesc(EField.GY, 3), new ModeDesc(EField.GX, 0), new ModeDesc(EField.GX, 1), new ModeDesc(EField.GX, 2), new ModeDesc(EField.GX, 3), new ModeDesc(EField.GX, 4),
                new ModeDesc(EField.GX, 5), new ModeDesc(EField.GZ, 0), new ModeDesc(EField.GZ, 1), new ModeDesc(EField.GZ, 2), new ModeDesc(EField.GZ, 3), new ModeDesc(EField.BX, 0), new ModeDesc(EField.BX, 1), new ModeDesc(EField.BX, 2), new ModeDesc(EField.BX, 3), new ModeDesc(EField.BX, 4),
                new ModeDesc(EField.BX, 5), new ModeDesc(EField.BY, 0), new ModeDesc(EField.BY, 1), new ModeDesc(EField.BY, 2), new ModeDesc(EField.BY, 3), new ModeDesc(EField.RY, 0), new ModeDesc(EField.RY, 1), new ModeDesc(EField.RY, 2), new ModeDesc(EField.RY, 3), new ModeDesc(EField.RY, 4),
                new ModeDesc(EField.RY, 5), new ModeDesc(EField.RZ, 0), new ModeDesc(EField.RZ, 1), new ModeDesc(EField.RZ, 2), new ModeDesc(EField.RZ, 3), new ModeDesc(EField.RZ, 4), new ModeDesc(EField.RZ, 5), new ModeDesc(EField. D, 0), new ModeDesc(EField. D, 1), new ModeDesc(EField. D, 2),
                new ModeDesc(EField. D, 3), new ModeDesc(EField. D, 4),
            },

            new ModeDesc[]{   // Mode 11 (EField.0x03) - 10 10
                new ModeDesc(EField.M, 0), new ModeDesc(EField.M, 1), new ModeDesc(EField.M, 2), new ModeDesc(EField.M, 3), new ModeDesc(EField.M, 4), new ModeDesc(EField.RW, 0), new ModeDesc(EField.RW, 1), new ModeDesc(EField.RW, 2), new ModeDesc(EField.RW, 3), new ModeDesc(EField.RW, 4),
                new ModeDesc(EField.RW, 5), new ModeDesc(EField.RW, 6), new ModeDesc(EField.RW, 7), new ModeDesc(EField.RW, 8), new ModeDesc(EField.RW, 9), new ModeDesc(EField.GW, 0), new ModeDesc(EField.GW, 1), new ModeDesc(EField.GW, 2), new ModeDesc(EField.GW, 3), new ModeDesc(EField.GW, 4),
                new ModeDesc(EField.GW, 5), new ModeDesc(EField.GW, 6), new ModeDesc(EField.GW, 7), new ModeDesc(EField.GW, 8), new ModeDesc(EField.GW, 9), new ModeDesc(EField.BW, 0), new ModeDesc(EField.BW, 1), new ModeDesc(EField.BW, 2), new ModeDesc(EField.BW, 3), new ModeDesc(EField.BW, 4),
                new ModeDesc(EField.BW, 5), new ModeDesc(EField.BW, 6), new ModeDesc(EField.BW, 7), new ModeDesc(EField.BW, 8), new ModeDesc(EField.BW, 9), new ModeDesc(EField.RX, 0), new ModeDesc(EField.RX, 1), new ModeDesc(EField.RX, 2), new ModeDesc(EField.RX, 3), new ModeDesc(EField.RX, 4),
                new ModeDesc(EField.RX, 5), new ModeDesc(EField.RX, 6), new ModeDesc(EField.RX, 7), new ModeDesc(EField.RX, 8), new ModeDesc(EField.RX, 9), new ModeDesc(EField.GX, 0), new ModeDesc(EField.GX, 1), new ModeDesc(EField.GX, 2), new ModeDesc(EField.GX, 3), new ModeDesc(EField.GX, 4),
                new ModeDesc(EField.GX, 5), new ModeDesc(EField.GX, 6), new ModeDesc(EField.GX, 7), new ModeDesc(EField.GX, 8), new ModeDesc(EField.GX, 9), new ModeDesc(EField.BX, 0), new ModeDesc(EField.BX, 1), new ModeDesc(EField.BX, 2), new ModeDesc(EField.BX, 3), new ModeDesc(EField.BX, 4),
                new ModeDesc(EField.BX, 5), new ModeDesc(EField.BX, 6), new ModeDesc(EField.BX, 7), new ModeDesc(EField.BX, 8), new ModeDesc(EField.BX, 9), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0),
                new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0),
                new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0),
            },

            new ModeDesc[]{   // Mode 12 (EField.0x07) - 11 9
                new ModeDesc(EField.M, 0), new ModeDesc(EField.M, 1), new ModeDesc(EField.M, 2), new ModeDesc(EField.M, 3), new ModeDesc(EField.M, 4), new ModeDesc(EField.RW, 0), new ModeDesc(EField.RW, 1), new ModeDesc(EField.RW, 2), new ModeDesc(EField.RW, 3), new ModeDesc(EField.RW, 4),
                new ModeDesc(EField.RW, 5), new ModeDesc(EField.RW, 6), new ModeDesc(EField.RW, 7), new ModeDesc(EField.RW, 8), new ModeDesc(EField.RW, 9), new ModeDesc(EField.GW, 0), new ModeDesc(EField.GW, 1), new ModeDesc(EField.GW, 2), new ModeDesc(EField.GW, 3), new ModeDesc(EField.GW, 4),
                new ModeDesc(EField.GW, 5), new ModeDesc(EField.GW, 6), new ModeDesc(EField.GW, 7), new ModeDesc(EField.GW, 8), new ModeDesc(EField.GW, 9), new ModeDesc(EField.BW, 0), new ModeDesc(EField.BW, 1), new ModeDesc(EField.BW, 2), new ModeDesc(EField.BW, 3), new ModeDesc(EField.BW, 4),
                new ModeDesc(EField.BW, 5), new ModeDesc(EField.BW, 6), new ModeDesc(EField.BW, 7), new ModeDesc(EField.BW, 8), new ModeDesc(EField.BW, 9), new ModeDesc(EField.RX, 0), new ModeDesc(EField.RX, 1), new ModeDesc(EField.RX, 2), new ModeDesc(EField.RX, 3), new ModeDesc(EField.RX, 4),
                new ModeDesc(EField.RX, 5), new ModeDesc(EField.RX, 6), new ModeDesc(EField.RX, 7), new ModeDesc(EField.RX, 8), new ModeDesc(EField.RW,10), new ModeDesc(EField.GX, 0), new ModeDesc(EField.GX, 1), new ModeDesc(EField.GX, 2), new ModeDesc(EField.GX, 3), new ModeDesc(EField.GX, 4),
                new ModeDesc(EField.GX, 5), new ModeDesc(EField.GX, 6), new ModeDesc(EField.GX, 7), new ModeDesc(EField.GX, 8), new ModeDesc(EField.GW,10), new ModeDesc(EField.BX, 0), new ModeDesc(EField.BX, 1), new ModeDesc(EField.BX, 2), new ModeDesc(EField.BX, 3), new ModeDesc(EField.BX, 4),
                new ModeDesc(EField.BX, 5), new ModeDesc(EField.BX, 6), new ModeDesc(EField.BX, 7), new ModeDesc(EField.BX, 8), new ModeDesc(EField.BW,10), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0),
                new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0),
                new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0),
            },

            new ModeDesc[]{   // Mode 13 (EField.0x0b) - 12 8
                new ModeDesc(EField.M, 0), new ModeDesc(EField.M, 1), new ModeDesc(EField.M, 2), new ModeDesc(EField.M, 3), new ModeDesc(EField.M, 4), new ModeDesc(EField.RW, 0), new ModeDesc(EField.RW, 1), new ModeDesc(EField.RW, 2), new ModeDesc(EField.RW, 3), new ModeDesc(EField.RW, 4),
                new ModeDesc(EField.RW, 5), new ModeDesc(EField.RW, 6), new ModeDesc(EField.RW, 7), new ModeDesc(EField.RW, 8), new ModeDesc(EField.RW, 9), new ModeDesc(EField.GW, 0), new ModeDesc(EField.GW, 1), new ModeDesc(EField.GW, 2), new ModeDesc(EField.GW, 3), new ModeDesc(EField.GW, 4),
                new ModeDesc(EField.GW, 5), new ModeDesc(EField.GW, 6), new ModeDesc(EField.GW, 7), new ModeDesc(EField.GW, 8), new ModeDesc(EField.GW, 9), new ModeDesc(EField.BW, 0), new ModeDesc(EField.BW, 1), new ModeDesc(EField.BW, 2), new ModeDesc(EField.BW, 3), new ModeDesc(EField.BW, 4),
                new ModeDesc(EField.BW, 5), new ModeDesc(EField.BW, 6), new ModeDesc(EField.BW, 7), new ModeDesc(EField.BW, 8), new ModeDesc(EField.BW, 9), new ModeDesc(EField.RX, 0), new ModeDesc(EField.RX, 1), new ModeDesc(EField.RX, 2), new ModeDesc(EField.RX, 3), new ModeDesc(EField.RX, 4),
                new ModeDesc(EField.RX, 5), new ModeDesc(EField.RX, 6), new ModeDesc(EField.RX, 7), new ModeDesc(EField.RW,11), new ModeDesc(EField.RW,10), new ModeDesc(EField.GX, 0), new ModeDesc(EField.GX, 1), new ModeDesc(EField.GX, 2), new ModeDesc(EField.GX, 3), new ModeDesc(EField.GX, 4),
                new ModeDesc(EField.GX, 5), new ModeDesc(EField.GX, 6), new ModeDesc(EField.GX, 7), new ModeDesc(EField.GW,11), new ModeDesc(EField.GW,10), new ModeDesc(EField.BX, 0), new ModeDesc(EField.BX, 1), new ModeDesc(EField.BX, 2), new ModeDesc(EField.BX, 3), new ModeDesc(EField.BX, 4),
                new ModeDesc(EField.BX, 5), new ModeDesc(EField.BX, 6), new ModeDesc(EField.BX, 7), new ModeDesc(EField.BW,11), new ModeDesc(EField.BW,10), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0),
                new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0),
                new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0),
            },

            new ModeDesc[]{   // Mode 14 (EField.0x0f) - 16 4
                new ModeDesc(EField.M, 0), new ModeDesc(EField.M, 1), new ModeDesc(EField.M, 2), new ModeDesc(EField.M, 3), new ModeDesc(EField.M, 4), new ModeDesc(EField.RW, 0), new ModeDesc(EField.RW, 1), new ModeDesc(EField.RW, 2), new ModeDesc(EField.RW, 3), new ModeDesc(EField.RW, 4),
                new ModeDesc(EField.RW, 5), new ModeDesc(EField.RW, 6), new ModeDesc(EField.RW, 7), new ModeDesc(EField.RW, 8), new ModeDesc(EField.RW, 9), new ModeDesc(EField.GW, 0), new ModeDesc(EField.GW, 1), new ModeDesc(EField.GW, 2), new ModeDesc(EField.GW, 3), new ModeDesc(EField.GW, 4),
                new ModeDesc(EField.GW, 5), new ModeDesc(EField.GW, 6), new ModeDesc(EField.GW, 7), new ModeDesc(EField.GW, 8), new ModeDesc(EField.GW, 9), new ModeDesc(EField.BW, 0), new ModeDesc(EField.BW, 1), new ModeDesc(EField.BW, 2), new ModeDesc(EField.BW, 3), new ModeDesc(EField.BW, 4),
                new ModeDesc(EField.BW, 5), new ModeDesc(EField.BW, 6), new ModeDesc(EField.BW, 7), new ModeDesc(EField.BW, 8), new ModeDesc(EField.BW, 9), new ModeDesc(EField.RX, 0), new ModeDesc(EField.RX, 1), new ModeDesc(EField.RX, 2), new ModeDesc(EField.RX, 3), new ModeDesc(EField.RW,15),
                new ModeDesc(EField.RW,14), new ModeDesc(EField.RW,13), new ModeDesc(EField.RW,12), new ModeDesc(EField.RW,11), new ModeDesc(EField.RW,10), new ModeDesc(EField.GX, 0), new ModeDesc(EField.GX, 1), new ModeDesc(EField.GX, 2), new ModeDesc(EField.GX, 3), new ModeDesc(EField.GW,15),
                new ModeDesc(EField.GW,14), new ModeDesc(EField.GW,13), new ModeDesc(EField.GW,12), new ModeDesc(EField.GW,11), new ModeDesc(EField.GW,10), new ModeDesc(EField.BX, 0), new ModeDesc(EField.BX, 1), new ModeDesc(EField.BX, 2), new ModeDesc(EField.BX, 3), new ModeDesc(EField.BW,15),
                new ModeDesc(EField.BW,14), new ModeDesc(EField.BW,13), new ModeDesc(EField.BW,12), new ModeDesc(EField.BW,11), new ModeDesc(EField.BW,10), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0),
                new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0),
                new ModeDesc(EField.NA, 0), new ModeDesc(EField.NA, 0),
            },
        };

        static readonly ModeInfo[] ms_aInfo =
        {
            new ModeInfo(0x00, 1, true,  3, new Color[][]{ new Color[]{ new Color(10,10,10,0), new Color( 5, 5, 5,0) }, new Color[]{ new Color(5,5,5,0), new Color(5,5,5,0)} }), // Mode 1
            new ModeInfo(0x01, 1, true,  3, new Color[][] { new Color[]{ new Color( 7, 7, 7,0), new Color( 6, 6, 6,0) }, new Color[]{ new Color(6,6,6,0), new Color(6,6,6,0) } } ), // Mode 2
            new ModeInfo(0x02, 1, true,  3, new Color[][] { new Color[]{ new Color(11,11,11,0), new Color( 5, 4, 4,0) }, new Color[]{ new Color(5,4,4,0), new Color(5,4,4,0) } } ), // Mode 3
            new ModeInfo(0x06, 1, true,  3, new Color[][] { new Color[]{ new Color(11,11,11,0), new Color( 4, 5, 4,0) }, new Color[]{ new Color(4,5,4,0), new Color(4,5,4,0) } } ), // Mode 4
            new ModeInfo(0x0a, 1, true,  3, new Color[][] { new Color[]{ new Color(11,11,11,0), new Color( 4, 4, 5,0) }, new Color[]{ new Color(4,4,5,0), new Color(4,4,5,0) } }), // Mode 5
            new ModeInfo(0x0e, 1, true,  3, new Color[][] { new Color[]{ new Color( 9, 9, 9,0), new Color( 5, 5, 5,0) }, new Color[]{ new Color(5,5,5,0), new Color(5,5,5,0) } }), // Mode 6
            new ModeInfo(0x12, 1, true,  3, new Color[][] { new Color[]{ new Color( 8, 8, 8,0), new Color( 6, 5, 5,0) }, new Color[]{ new Color(6,5,5,0), new Color(6,5,5,0) } }), // Mode 7
            new ModeInfo(0x16, 1, true,  3, new Color[][] { new Color[]{ new Color( 8, 8, 8,0), new Color( 5, 6, 5,0) }, new Color[]{ new Color(5,6,5,0), new Color(5,6,5,0) } }), // Mode 8
            new ModeInfo(0x1a, 1, true,  3, new Color[][] { new Color[]{ new Color( 8, 8, 8,0), new Color( 5, 5, 6,0) }, new Color[]{ new Color(5,5,6,0), new Color(5,5,6,0) } }), // Mode 9
            new ModeInfo(0x1e, 1, false, 3, new Color[][] { new Color[]{ new Color( 6, 6, 6,0), new Color( 6, 6, 6,0) }, new Color[]{ new Color(6,6,6,0), new Color(6,6,6,0) } }), // Mode 10
            new ModeInfo(0x03, 0, false, 4, new Color[][] { new Color[]{ new Color(10,10,10,0), new Color(10,10,10,0) }, new Color[]{ new Color(0,0,0,0), new Color(0,0,0,0) } }), // Mode 11
            new ModeInfo(0x07, 0, true,  4, new Color[][] { new Color[]{ new Color(11,11,11,0), new Color( 9, 9, 9,0) }, new Color[]{ new Color(0,0,0,0), new Color(0,0,0,0) } }), // Mode 12
            new ModeInfo(0x0b, 0, true,  4, new Color[][] { new Color[]{ new Color(12,12,12,0), new Color( 8, 8, 8,0) }, new Color[]{ new Color(0,0,0,0), new Color(0,0,0,0) } }), // Mode 13
            new ModeInfo(0x0f, 0, true,  4, new Color[][] { new Color[]{ new Color(16,16,16,0), new Color( 4, 4, 4,0) }, new Color[]{ new Color(0,0,0,0), new Color(0,0,0,0) } }), // Mode 14
        };

        static readonly int[] ms_aModeToInfo =
        {
             0, // Mode 1   - 0x00
             1, // Mode 2   - 0x01
             2, // Mode 3   - 0x02
            10, // Mode 11  - 0x03
            -1, // Invalid  - 0x04
            -1, // Invalid  - 0x05
             3, // Mode 4   - 0x06
            11, // Mode 12  - 0x07
            -1, // Invalid  - 0x08
            -1, // Invalid  - 0x09
             4, // Mode 5   - 0x0a
            12, // Mode 13  - 0x0b
            -1, // Invalid  - 0x0c
            -1, // Invalid  - 0x0d
             5, // Mode 6   - 0x0e
            13, // Mode 14  - 0x0f
            -1, // Invalid  - 0x10
            -1, // Invalid  - 0x11
             6, // Mode 7   - 0x12
            -1, // Reserved - 0x13
            -1, // Invalid  - 0x14
            -1, // Invalid  - 0x15
             7, // Mode 8   - 0x16
            -1, // Reserved - 0x17
            -1, // Invalid  - 0x18
            -1, // Invalid  - 0x19
             8, // Mode 9   - 0x1a
            -1, // Reserved - 0x1b
            -1, // Invalid  - 0x1c
            -1, // Invalid  - 0x1d
             9, // Mode 10  - 0x1e
            -1, // Resreved - 0x1f
        };

        internal D3DX_BC6H() : base(16) { }

        internal Color4[] Decode(bool bSigned, Logger log)
        {
            Color4[] pOut = new Color4[BC.NUM_PIXELS_PER_BLOCK];

            uint uStartBit = 0;
            byte uMode = GetBits(ref uStartBit, 2);
            if (uMode != 0x00 && uMode != 0x01)
                uMode = (byte)((GetBits(ref uStartBit, 3) << 2) | uMode);

            Debug.Assert(uMode < 32);

            if (ms_aModeToInfo[uMode] >= 0)
            {
                Debug.Assert(ms_aModeToInfo[uMode] < ms_aInfo.Length);
                ModeDesc[] desc = ms_aDesc[ms_aModeToInfo[uMode]];

                Debug.Assert(ms_aModeToInfo[uMode] < ms_aDesc.Length);
                ModeInfo info = ms_aInfo[ms_aModeToInfo[uMode]];

                BC67.INTEndPntPair[] aEndPts = new BC67.INTEndPntPair[BC67.BC6H_MAX_REGIONS];
                uint uShape = 0;

                // Read header
                uint uHeaderBits = info.uPartitions > 0 ? 82U : 65U;
                while (uStartBit < uHeaderBits)
                {
                    uint uCurBit = uStartBit;
                    if (GetBit(ref uStartBit) > 0)
                    {
                        switch (desc[uCurBit].m_eField)
                        {
                            case EField.D: uShape |= (uint)(1 << desc[uCurBit].m_uBit); break;
                            case EField.RW: aEndPts[0].A.r |= 1 << desc[uCurBit].m_uBit; break;
                            case EField.RX: aEndPts[0].B.r |= 1 << desc[uCurBit].m_uBit; break;
                            case EField.RY: aEndPts[1].A.r |= 1 << desc[uCurBit].m_uBit; break;
                            case EField.RZ: aEndPts[1].B.r |= 1 << desc[uCurBit].m_uBit; break;
                            case EField.GW: aEndPts[0].A.g |= 1 << desc[uCurBit].m_uBit; break;
                            case EField.GX: aEndPts[0].B.g |= 1 << desc[uCurBit].m_uBit; break;
                            case EField.GY: aEndPts[1].A.g |= 1 << desc[uCurBit].m_uBit; break;
                            case EField.GZ: aEndPts[1].B.g |= 1 << desc[uCurBit].m_uBit; break;
                            case EField.BW: aEndPts[0].A.b |= 1 << desc[uCurBit].m_uBit; break;
                            case EField.BX: aEndPts[0].B.b |= 1 << desc[uCurBit].m_uBit; break;
                            case EField.BY: aEndPts[1].A.b |= 1 << desc[uCurBit].m_uBit; break;
                            case EField.BZ: aEndPts[1].B.b |= 1 << desc[uCurBit].m_uBit; break;
                            default:
                                {
                                    log.WriteError("BC6H: Invalid header bits encountered during decoding\n");
                                    BC67.FillWithErrorColors(pOut);
                                    return pOut;
                                }
                        }
                    }
                }

                Debug.Assert(uShape < 64);

                // Sign extend necessary end points
                if (bSigned)
                    aEndPts[0].A.SignExtend(info.RGBAPrec[0][0]);

                if (bSigned || info.bTransformed)
                {
                    Debug.Assert(info.uPartitions < BC67.BC6H_MAX_REGIONS);
                    for (uint p = 0; p <= info.uPartitions; ++p)
                    {
                        if (p != 0)
                        {
                            aEndPts[p].A.SignExtend(info.RGBAPrec[p][0]);
                        }
                        aEndPts[p].B.SignExtend(info.RGBAPrec[p][1]);
                    }
                }

                // Inverse transform the end points
                if (info.bTransformed)
                    BC67.TransformInverse(aEndPts, info.RGBAPrec[0][0], bSigned);

                // Read indices
                for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
                {
                    uint uNumBits = BC67.IsFixUpOffset(info.uPartitions, uShape, i) ? info.uIndexPrec - 1U : info.uIndexPrec;
                    if (uStartBit + uNumBits > 128)
                    {
                        log.WriteError("BC6H: Invalid block encountered during decoding\n");
                        BC67.FillWithErrorColors(pOut);
                        return pOut;
                    }
                    byte uIndex = GetBits(ref uStartBit, uNumBits);

                    if (uIndex >= ((info.uPartitions > 0) ? 8 : 16))
                    {
                        log.WriteError("BC6H: Invalid index encountered during decoding\n");
                        BC67.FillWithErrorColors(pOut);
                        return pOut;
                    }

                    uint uRegion = BC67.g_aPartitionTable[info.uPartitions][uShape][i];
                    Debug.Assert(uRegion < BC67.BC6H_MAX_REGIONS);

                    // Unquantize endpoints and interpolate
                    int r1 = Unquantize(aEndPts[uRegion].A.r, info.RGBAPrec[0][0].R, bSigned);
                    int g1 = Unquantize(aEndPts[uRegion].A.g, info.RGBAPrec[0][0].G, bSigned);
                    int b1 = Unquantize(aEndPts[uRegion].A.b, info.RGBAPrec[0][0].B, bSigned);
                    int r2 = Unquantize(aEndPts[uRegion].B.r, info.RGBAPrec[0][0].R, bSigned);
                    int g2 = Unquantize(aEndPts[uRegion].B.g, info.RGBAPrec[0][0].G, bSigned);
                    int b2 = Unquantize(aEndPts[uRegion].B.b, info.RGBAPrec[0][0].B, bSigned);
                    int[] aWeights = info.uPartitions > 0 ? BC67.g_aWeights3 : BC67.g_aWeights4;
                    INTColor fc = new INTColor()
                    {
                        r = FinishUnquantize((r1 * (BC67.BC67_WEIGHT_MAX - aWeights[uIndex]) + r2 * aWeights[uIndex] + BC67.BC67_WEIGHT_ROUND) >> BC67.BC67_WEIGHT_SHIFT, bSigned),
                        g = FinishUnquantize((g1 * (BC67.BC67_WEIGHT_MAX - aWeights[uIndex]) + g2 * aWeights[uIndex] + BC67.BC67_WEIGHT_ROUND) >> BC67.BC67_WEIGHT_SHIFT, bSigned),
                        b = FinishUnquantize((b1 * (BC67.BC67_WEIGHT_MAX - aWeights[uIndex]) + b2 * aWeights[uIndex] + BC67.BC67_WEIGHT_ROUND) >> BC67.BC67_WEIGHT_SHIFT, bSigned),
                    };


                    Half[] rgb = fc.ToF16(bSigned);
                    pOut[i].R = rgb[0]; // XMConvertHalfToFloat(rgb[0]);
                    pOut[i].G = rgb[1];
                    pOut[i].B = rgb[2];
                    pOut[i].A = 1.0f;
                }
            }
            else
            {
                string warnstr = "BC6H: Invalid mode encountered during decoding\n";
                switch (uMode)
                {
                    case 0x13: warnstr = "BC6H: Reserved mode 10011 encountered during decoding\n"; break;
                    case 0x17: warnstr = "BC6H: Reserved mode 10111 encountered during decoding\n"; break;
                    case 0x1B: warnstr = "BC6H: Reserved mode 11011 encountered during decoding\n"; break;
                    case 0x1F: warnstr = "BC6H: Reserved mode 11111 encountered during decoding\n"; break;
                }

                log.WriteWarning(warnstr);

                // Per the BC6H format spec, we must return opaque black
                for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
                    pOut[i] = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            }

            return pOut;
        }

        internal void Encode(bool bSigned, Color4[] pIn, Context context)
        {
            EncodeParams EP = context.EP;
            EP.Set(pIn, bSigned);

            //int arraySize = Marshal.SizeOf<ModeInfo>() * ms_aInfo.Length;
            for (EP.uMode = 0; EP.uMode < ms_aInfo.Length && EP.fBestErr > 0; ++EP.uMode)
            {
                byte uShapes = (byte)(ms_aInfo[EP.uMode].uPartitions > 0 ? 32 : 1);
                // Number of rough cases to look at. reasonable values of this are 1, uShapes/4, and uShapes
                // uShapes/4 gets nearly all the cases; you can increase that a bit (say by 3 or 4) if you really want to squeeze the last bit out
                int uItems = Math.Max(1, uShapes >> 2);


                // pick the best uItems shapes and refine these.
                for (EP.uShape = 0; EP.uShape < uShapes; ++EP.uShape)
                {
                    uint uShape = EP.uShape;
                    context.afRoughMSE[uShape] = RoughMSE(EP, context);
                    context.auShape[uShape] = (byte)uShape;
                }

                // Bubble up the first uItems items
                for (uint i = 0; i < uItems; i++)
                {
                    for (uint j = i + 1; j < uShapes; j++)
                    {
                        if (context.afRoughMSE[i] > context.afRoughMSE[j])
                        {
                            BC67.Swap(ref context.afRoughMSE[i], ref context.afRoughMSE[j]);
                            BC67.Swap(ref context.auShape[i], ref context.auShape[j]);
                        }
                    }
                }

                for (uint i = 0; i < uItems && EP.fBestErr > 0; i++)
                {
                    EP.uShape = context.auShape[i];
                    Refine(EP, context);
                }
            }
        }

        private static int Quantize(int iValue, int prec, bool bSigned)
        {
            Debug.Assert(prec > 1);   // didn't bother to make it work for 1
            int q = 0;
            int s = 0;
            if (bSigned)
            {
                Debug.Assert(iValue >= -BC67.F16MAX && iValue <= BC67.F16MAX);
                if (iValue < 0)
                {
                    s = 1;
                    iValue = -iValue;
                }
                q = (prec >= 16) ? iValue : (iValue << (prec - 1)) / (BC67.F16MAX + 1);
                if (s > 0)
                    q = -q;
                Debug.Assert(q > -(1 << (prec - 1)) && q < (1 << (prec - 1)));
            }
            else
            {
                Debug.Assert(iValue >= 0 && iValue <= BC67.F16MAX);
                q = (prec >= 15) ? iValue : (iValue << prec) / (BC67.F16MAX + 1);
                Debug.Assert(q >= 0 && q < (1 << prec));
            }

            return q;
        }

        private static int Unquantize(int comp, byte uBitsPerComp, bool bSigned)
        {
            int unq = 0, s = 0;
            if (bSigned)
            {
                if (uBitsPerComp >= 16)
                {
                    unq = comp;
                }
                else
                {
                    if (comp < 0)
                    {
                        s = 1;
                        comp = -comp;
                    }

                    if (comp == 0)
                        unq = 0;
                    else if (comp >= ((1 << (uBitsPerComp - 1)) - 1))
                        unq = 0x7FFF;
                    else
                        unq = ((comp << 15) + 0x4000) >> (uBitsPerComp - 1);

                    if (s > 0)
                        unq = -unq;
                }
            }
            else
            {
                if (uBitsPerComp >= 15) unq = comp;
                else if (comp == 0) unq = 0;
                else if (comp == ((1 << uBitsPerComp) - 1)) unq = 0xFFFF;
                else unq = ((comp << 16) + 0x8000) >> uBitsPerComp;
            }

            return unq;
        }

        private static int FinishUnquantize(int comp, bool bSigned)
        {
            if (bSigned)
                return (comp < 0) ? -(((-comp) * 31) >> 5) : (comp * 31) >> 5;  // scale the magnitude by 31/32
            else
                return (comp * 31) >> 6;                                        // scale the magnitude by 31/64
        }

        private static bool EndPointsFit(EncodeParams pEP, BC67.INTEndPntPair[] aEndPts, Context context)
        {
            bool bTransformed = ms_aInfo[pEP.uMode].bTransformed;
            bool bIsSigned = pEP.bSigned;
            INTColor[] aBits = context.aBits;

            Color Prec0 = ms_aInfo[pEP.uMode].RGBAPrec[0][0];
            Color Prec1 = ms_aInfo[pEP.uMode].RGBAPrec[0][1];
            Color Prec2 = ms_aInfo[pEP.uMode].RGBAPrec[1][0];
            Color Prec3 = ms_aInfo[pEP.uMode].RGBAPrec[1][1];

            aBits[0].r = BC67.NBits(aEndPts[0].A.r, bIsSigned);
            aBits[0].g = BC67.NBits(aEndPts[0].A.g, bIsSigned);
            aBits[0].b = BC67.NBits(aEndPts[0].A.b, bIsSigned);
            aBits[1].r = BC67.NBits(aEndPts[0].B.r, bTransformed || bIsSigned);
            aBits[1].g = BC67.NBits(aEndPts[0].B.g, bTransformed || bIsSigned);
            aBits[1].b = BC67.NBits(aEndPts[0].B.b, bTransformed || bIsSigned);
            if (aBits[0].r > Prec0.R || aBits[1].r > Prec1.R ||
                aBits[0].g > Prec0.G || aBits[1].g > Prec1.G ||
                aBits[0].b > Prec0.B || aBits[1].b > Prec1.B)
                return false;

            if (ms_aInfo[pEP.uMode].uPartitions > 0)
            {
                aBits[2].r = BC67.NBits(aEndPts[1].A.r, bTransformed || bIsSigned);
                aBits[2].g = BC67.NBits(aEndPts[1].A.g, bTransformed || bIsSigned);
                aBits[2].b = BC67.NBits(aEndPts[1].A.b, bTransformed || bIsSigned);
                aBits[3].r = BC67.NBits(aEndPts[1].B.r, bTransformed || bIsSigned);
                aBits[3].g = BC67.NBits(aEndPts[1].B.g, bTransformed || bIsSigned);
                aBits[3].b = BC67.NBits(aEndPts[1].B.b, bTransformed || bIsSigned);

                if (aBits[2].r > Prec2.R || aBits[3].r > Prec3.R ||
                    aBits[2].g > Prec2.G || aBits[3].g > Prec3.G ||
                    aBits[2].b > Prec2.B || aBits[3].b > Prec3.B)
                    return false;
            }

            return true;
        }

        void GeneratePaletteQuantized(EncodeParams pEP, BC67.INTEndPntPair endPts, INTColor[] aPalette)
        {
            int uIndexPrec = ms_aInfo[pEP.uMode].uIndexPrec;
            int uNumIndices = 1 << uIndexPrec;
            Debug.Assert(uNumIndices > 0);
            Color Prec = ms_aInfo[pEP.uMode].RGBAPrec[0][0];

            // scale endpoints
            BC67.INTEndPntPair unqEndPts = new BC67.INTEndPntPair();
            unqEndPts.A.r = Unquantize(endPts.A.r, Prec.R, pEP.bSigned);
            unqEndPts.A.g = Unquantize(endPts.A.g, Prec.G, pEP.bSigned);
            unqEndPts.A.b = Unquantize(endPts.A.b, Prec.B, pEP.bSigned);
            unqEndPts.B.r = Unquantize(endPts.B.r, Prec.R, pEP.bSigned);
            unqEndPts.B.g = Unquantize(endPts.B.g, Prec.G, pEP.bSigned);
            unqEndPts.B.b = Unquantize(endPts.B.b, Prec.B, pEP.bSigned);

            // interpolate
            int[] aWeights = null;
            switch (uIndexPrec)
            {
                case 3: aWeights = BC67.g_aWeights3; Debug.Assert(uNumIndices <= 8); break;
                case 4: aWeights = BC67.g_aWeights4; Debug.Assert(uNumIndices <= 16); break;
                default:
                    Debug.Assert(false);
                    for (uint i = 0; i < uNumIndices; ++i)
                        aPalette[i] = new INTColor(0, 0, 0);
                    return;
            }

            int weight;
            for (uint i = 0; i < uNumIndices; ++i)
            {
                weight = aWeights[i];
                aPalette[i] = new INTColor()
                {
                    r = FinishUnquantize(
                    (unqEndPts.A.r * (BC67.BC67_WEIGHT_MAX - weight) + unqEndPts.B.r * weight + BC67.BC67_WEIGHT_ROUND) >> BC67.BC67_WEIGHT_SHIFT,
                    pEP.bSigned),
                    g = FinishUnquantize(
                    (unqEndPts.A.g * (BC67.BC67_WEIGHT_MAX - weight) + unqEndPts.B.g * weight + BC67.BC67_WEIGHT_ROUND) >> BC67.BC67_WEIGHT_SHIFT,
                    pEP.bSigned),
                    b = FinishUnquantize(
                    (unqEndPts.A.b * (BC67.BC67_WEIGHT_MAX - weight) + unqEndPts.B.b * weight + BC67.BC67_WEIGHT_ROUND) >> BC67.BC67_WEIGHT_SHIFT,
                    pEP.bSigned)
                };
            }
        }

        private unsafe float MapColorsQuantized(EncodeParams pEP, INTColor[] aColors, uint np, BC67.INTEndPntPair endPts, Context context)
        {
            byte uIndexPrec = ms_aInfo[pEP.uMode].uIndexPrec;
            byte uNumIndices = (byte)(1U << uIndexPrec);
            GeneratePaletteQuantized(pEP, endPts, context.aPalette);

            float fTotErr = 0;
            for (uint i = 0; i < np; ++i)
            {
                INTColor aColor = aColors[i];
                Vector3F vcolors = *(Vector3F*)&aColor; // new Vector3F(aColor.r, aColor.g, aColor.b);

                // Compute ErrorMetricRGB
                INTColor aPal = context.aPalette[0];
                Vector3F tpal = *(Vector3F*)&aPal; // new Vector3F(aPal.r, aPal.g, aPal.b); // XMLoadSInt4(reinterpret_cast <const XMINT4*> (&));
                tpal = vcolors - tpal;                                          // XMVectorSubtract(vcolors, tpal);
                float fBestErr = Vector3F.Dot(tpal, tpal);  // XMVectorGetX(XMVector3Dot(tpal, tpal));

                for (int j = 1; j < uNumIndices && fBestErr > 0; ++j)
                {
                    // Compute ErrorMetricRGB
                    aPal = context.aPalette[j];
                    tpal = *(Vector3F*)&aPal; // new Vector3F(aPal.r, aPal.g, aPal.b);      // XMLoadSInt4(reinterpret_cast <const XMINT4*> (&));
                    tpal = vcolors - tpal;                                      // XMVectorSubtract(vcolors, tpal);
                    float fErr = Vector3F.Dot(tpal, tpal);  // XMVectorGetX(XMVector3Dot(tpal, tpal));
                    if (fErr > fBestErr) break;                                 // error increased, so we're done searching
                    if (fErr < fBestErr) fBestErr = fErr;
                }
                fTotErr += fBestErr;
            }
            return fTotErr;
        }

        float PerturbOne(EncodeParams pEP, INTColor[] aColors, uint np, byte ch,
            BC67.INTEndPntPair oldEndPts, ref BC67.INTEndPntPair newEndPts, float fOldErr, int do_b, Context context)
        {
            byte uPrec = 0;
            switch (ch)
            {
                case 0: uPrec = ms_aInfo[pEP.uMode].RGBAPrec[0][0].R; break;
                case 1: uPrec = ms_aInfo[pEP.uMode].RGBAPrec[0][0].G; break;
                case 2: uPrec = ms_aInfo[pEP.uMode].RGBAPrec[0][0].B; break;
                default:
                    Debug.Assert(false);
                    newEndPts = oldEndPts;
                    return float.MaxValue;
            }

            BC67.INTEndPntPair tmpEndPts;
            float fMinErr = fOldErr;
            int beststep = 0;

            // copy real endpoints so we can perturb them
            tmpEndPts = newEndPts = oldEndPts;

            // do a logarithmic search for the best error for this endpoint (which)
            for (int step = 1 << (uPrec - 1); step > 0; step >>= 1)
            {
                bool bImproved = false;
                for (int sign = -1; sign <= 1; sign += 2)
                {
                    if (do_b == 0)
                    {
                        tmpEndPts.A[ch] = newEndPts.A[ch] + sign * step;
                        if (tmpEndPts.A[ch] < 0 || tmpEndPts.A[ch] >= (1 << uPrec))
                            continue;
                    }
                    else
                    {
                        tmpEndPts.B[ch] = newEndPts.B[ch] + sign * step;
                        if (tmpEndPts.B[ch] < 0 || tmpEndPts.B[ch] >= (1 << uPrec))
                            continue;
                    }

                    float fErr = MapColorsQuantized(pEP, aColors, np, tmpEndPts, context);

                    if (fErr < fMinErr)
                    {
                        bImproved = true;
                        fMinErr = fErr;
                        beststep = sign * step;
                    }
                }
                // if this was an improvement, move the endpoint and continue search from there
                if (bImproved)
                {
                    if (do_b == 0)
                        newEndPts.A[ch] += beststep;
                    else
                        newEndPts.B[ch] += beststep;
                }
            }
            return fMinErr;
        }

        private void OptimizeOne(EncodeParams pEP, INTColor[] aColors, uint np, float aOrgErr,
            BC67.INTEndPntPair aOrgEndPts, ref BC67.INTEndPntPair aOptEndPts, Context context)
        {
            float aOptErr = aOrgErr;
            aOptEndPts.A = aOrgEndPts.A;
            aOptEndPts.B = aOrgEndPts.B;

            BC67.INTEndPntPair new_a = new BC67.INTEndPntPair();
            BC67.INTEndPntPair new_b = new BC67.INTEndPntPair();
            BC67.INTEndPntPair newEndPts = new BC67.INTEndPntPair();
            int do_b;

            // now optimize each channel separately
            for (byte ch = 0; ch < BC67.BC6H_NUM_CHANNELS; ++ch)
            {
                // figure out which endpoint when perturbed gives the most improvement and start there
                // if we just alternate, we can easily end up in a local minima
                float fErr0 = PerturbOne(pEP, aColors, np, ch, aOptEndPts, ref new_a, aOptErr, 0, context);  // perturb endpt A
                float fErr1 = PerturbOne(pEP, aColors, np, ch, aOptEndPts, ref new_b, aOptErr, 1, context);  // perturb endpt B

                if (fErr0 < fErr1)
                {
                    if (fErr0 >= aOptErr) continue;
                    aOptEndPts.A[ch] = new_a.A[ch];
                    aOptErr = fErr0;
                    do_b = 1;       // do B next
                }
                else
                {
                    if (fErr1 >= aOptErr) continue;
                    aOptEndPts.B[ch] = new_b.B[ch];
                    aOptErr = fErr1;
                    do_b = 0;       // do A next
                }

                // now alternate endpoints and keep trying until there is no improvement
                for (; ; )
                {
                    float fErr = PerturbOne(pEP, aColors, np, ch, aOptEndPts, ref newEndPts, aOptErr, do_b, context);
                    if (fErr >= aOptErr)
                        break;
                    if (do_b == 0)
                        aOptEndPts.A[ch] = newEndPts.A[ch];
                    else
                        aOptEndPts.B[ch] = newEndPts.B[ch];
                    aOptErr = fErr;
                    do_b = 1 - do_b;    // now move the other endpoint
                }
            }
        }

        private void OptimizeEndPoints(EncodeParams pEP, float[] aOrgErr, BC67.INTEndPntPair[] aOrgEndPts, BC67.INTEndPntPair[] aOptEndPts, Context context)
        {
            byte uPartitions = ms_aInfo[pEP.uMode].uPartitions;
            Debug.Assert(uPartitions < BC67.BC6H_MAX_REGIONS);

            for (uint p = 0; p <= uPartitions; ++p)
            {
                // collect the pixels in the region
                uint np = 0;
                for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
                {
                    if (BC67.g_aPartitionTable[p][pEP.uShape][i] == p)
                        context.aPixels[np++] = pEP.aIPixels[i];
                }

                OptimizeOne(pEP, context.aPixels, np, aOrgErr[p], aOrgEndPts[p], ref aOptEndPts[p], context);
            }
        }

        // Swap endpoints as needed to ensure that the indices at fix up have a 0 high-order bit
        private void SwapIndices(EncodeParams pEP, BC67.INTEndPntPair[] aEndPts, uint[] aIndices)
        {
            uint uPartitions = ms_aInfo[pEP.uMode].uPartitions;
            uint uNumIndices = 1U << ms_aInfo[pEP.uMode].uIndexPrec;
            uint uHighIndexBit = uNumIndices >> 1;

            Debug.Assert(uPartitions < BC67.BC6H_MAX_REGIONS && pEP.uShape < BC67.BC6H_MAX_SHAPES);

            for (uint p = 0; p <= uPartitions; ++p)
            {
                uint i = BC67.g_aFixUp[uPartitions][pEP.uShape][p];
                Debug.Assert(BC67.g_aPartitionTable[uPartitions][pEP.uShape][i] == p);
                if ((aIndices[i] & uHighIndexBit) == uHighIndexBit)
                {
                    // high bit is set, swap the aEndPts and indices for this region
                    aEndPts[p].SwapPoints();

                    for (uint j = 0; j < BC.NUM_PIXELS_PER_BLOCK; ++j)
                        if (BC67.g_aPartitionTable[uPartitions][pEP.uShape][j] == p)
                            aIndices[j] = uNumIndices - 1 - aIndices[j];
                }
            }
        }

        // assign indices given a tile, shape, and quantized endpoints, return toterr for each region
        private void AssignIndices(EncodeParams pEP, BC67.INTEndPntPair[] aEndPts, uint[] aIndices, float[] aTotErr, Context context)
        {
            byte uPartitions = ms_aInfo[pEP.uMode].uPartitions;
            byte uNumIndices = (byte)(1u << ms_aInfo[pEP.uMode].uIndexPrec);

            Debug.Assert(uPartitions < BC67.BC6H_MAX_REGIONS && pEP.uShape < BC67.BC6H_MAX_SHAPES);

            for (uint p = 0; p <= uPartitions; ++p)
            {
                GeneratePaletteQuantized(pEP, aEndPts[p], context.aPalette_indices[p]);
                aTotErr[p] = 0;
            }

            for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
            {
                byte uRegion = BC67.g_aPartitionTable[uPartitions][pEP.uShape][i];
                Debug.Assert(uRegion < BC67.BC6H_MAX_REGIONS);
                float fBestErr = BC67.Norm(pEP.aIPixels[i], context.aPalette_indices[uRegion][0]);
                aIndices[i] = 0;

                for (byte j = 1; j < uNumIndices && fBestErr > 0; ++j)
                {
                    float fErr = BC67.Norm(pEP.aIPixels[i], context.aPalette_indices[uRegion][j]);
                    if (fErr > fBestErr) break; // error increased, so we're done searching
                    if (fErr < fBestErr)
                    {
                        fBestErr = fErr;
                        aIndices[i] = j;
                    }
                }
                aTotErr[uRegion] += fBestErr;
            }
        }

        private void QuantizeEndPts(EncodeParams pEP, BC67.INTEndPntPair[] aQntEndPts)
        {
            BC67.INTEndPntPair[] aUnqEndPts = pEP.aUnqEndPts[pEP.uShape];
            Color Prec = ms_aInfo[pEP.uMode].RGBAPrec[0][0];
            byte uPartitions = ms_aInfo[pEP.uMode].uPartitions;
            Debug.Assert(uPartitions < BC67.BC6H_MAX_REGIONS);

            for (uint p = 0; p <= uPartitions; ++p)
            {
                aQntEndPts[p].A.r = Quantize(aUnqEndPts[p].A.r, Prec.R, pEP.bSigned);
                aQntEndPts[p].A.g = Quantize(aUnqEndPts[p].A.g, Prec.G, pEP.bSigned);
                aQntEndPts[p].A.b = Quantize(aUnqEndPts[p].A.b, Prec.B, pEP.bSigned);
                aQntEndPts[p].B.r = Quantize(aUnqEndPts[p].B.r, Prec.R, pEP.bSigned);
                aQntEndPts[p].B.g = Quantize(aUnqEndPts[p].B.g, Prec.G, pEP.bSigned);
                aQntEndPts[p].B.b = Quantize(aUnqEndPts[p].B.b, Prec.B, pEP.bSigned);
            }
        }

        private void EmitBlock(EncodeParams pEP, BC67.INTEndPntPair[] aEndPts, uint[] aIndices)
        {
            byte uRealMode = ms_aInfo[pEP.uMode].uMode;
            byte uPartitions = ms_aInfo[pEP.uMode].uPartitions;
            byte uIndexPrec = ms_aInfo[pEP.uMode].uIndexPrec;
            uint uHeaderBits = uPartitions > 0 ? 82U : 65U;
            ModeDesc[] desc = ms_aDesc[pEP.uMode];
            uint uStartBit = 0;

            while (uStartBit < uHeaderBits)
            {
                switch (desc[uStartBit].m_eField)
                {
                    case EField.M: SetBit(ref uStartBit, (byte)((uRealMode >> desc[uStartBit].m_uBit) & 0x01)); break;
                    case EField.D: SetBit(ref uStartBit, (byte)((pEP.uShape >> desc[uStartBit].m_uBit) & 0x01)); break;
                    case EField.RW: SetBit(ref uStartBit, (byte)((aEndPts[0].A.r >> desc[uStartBit].m_uBit) & 0x01)); break;
                    case EField.RX: SetBit(ref uStartBit, (byte)((aEndPts[0].B.r >> desc[uStartBit].m_uBit) & 0x01)); break;
                    case EField.RY: SetBit(ref uStartBit, (byte)((aEndPts[1].A.r >> desc[uStartBit].m_uBit) & 0x01)); break;
                    case EField.RZ: SetBit(ref uStartBit, (byte)((aEndPts[1].B.r >> desc[uStartBit].m_uBit) & 0x01)); break;
                    case EField.GW: SetBit(ref uStartBit, (byte)((aEndPts[0].A.g >> desc[uStartBit].m_uBit) & 0x01)); break;
                    case EField.GX: SetBit(ref uStartBit, (byte)((aEndPts[0].B.g >> desc[uStartBit].m_uBit) & 0x01)); break;
                    case EField.GY: SetBit(ref uStartBit, (byte)((aEndPts[1].A.g >> desc[uStartBit].m_uBit) & 0x01)); break;
                    case EField.GZ: SetBit(ref uStartBit, (byte)((aEndPts[1].B.g >> desc[uStartBit].m_uBit) & 0x01)); break;
                    case EField.BW: SetBit(ref uStartBit, (byte)((aEndPts[0].A.b >> desc[uStartBit].m_uBit) & 0x01)); break;
                    case EField.BX: SetBit(ref uStartBit, (byte)((aEndPts[0].B.b >> desc[uStartBit].m_uBit) & 0x01)); break;
                    case EField.BY: SetBit(ref uStartBit, (byte)((aEndPts[1].A.b >> desc[uStartBit].m_uBit) & 0x01)); break;
                    case EField.BZ: SetBit(ref uStartBit, (byte)((aEndPts[1].B.b >> desc[uStartBit].m_uBit) & 0x01)); break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            }

            for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
            {
                if (BC67.IsFixUpOffset(ms_aInfo[pEP.uMode].uPartitions, pEP.uShape, i))
                    SetBits(ref uStartBit, uIndexPrec - 1U, (byte)(aIndices[i]));
                else
                    SetBits(ref uStartBit, uIndexPrec, (byte)(aIndices[i]));
            }
            Debug.Assert(uStartBit == 128);
        }

        private void Refine(EncodeParams pEP, Context cxt)
        {
            byte uPartitions = ms_aInfo[pEP.uMode].uPartitions;
            Debug.Assert(uPartitions < BC67.BC6H_MAX_REGIONS);

            bool bTransformed = ms_aInfo[pEP.uMode].bTransformed;

            QuantizeEndPts(pEP, cxt.aOrgEndPts);
            AssignIndices(pEP, cxt.aOrgEndPts, cxt.aOrgIdx, cxt.aOrgErr, cxt);
            SwapIndices(pEP, cxt.aOrgEndPts, cxt.aOrgIdx);

            if (bTransformed)
                BC67.TransformForward(cxt.aOrgEndPts);

            if (EndPointsFit(pEP, cxt.aOrgEndPts, cxt))
            {
                if (bTransformed)
                    BC67.TransformInverse(cxt.aOrgEndPts, ms_aInfo[pEP.uMode].RGBAPrec[0][0], pEP.bSigned);

                OptimizeEndPoints(pEP, cxt.aOrgErr, cxt.aOrgEndPts, cxt.aOptEndPts, cxt);
                AssignIndices(pEP, cxt.aOptEndPts, cxt.aOptIdx, cxt.aOptErr, cxt);
                SwapIndices(pEP, cxt.aOptEndPts, cxt.aOptIdx);

                float fOrgTotErr = 0.0f, fOptTotErr = 0.0f;
                for (uint p = 0; p <= uPartitions; ++p)
                {
                    fOrgTotErr += cxt.aOrgErr[p];
                    fOptTotErr += cxt.aOptErr[p];
                }

                if (bTransformed)
                    BC67.TransformForward(cxt.aOptEndPts);

                if (EndPointsFit(pEP, cxt.aOptEndPts, cxt) && fOptTotErr < fOrgTotErr && fOptTotErr < pEP.fBestErr)
                {
                    pEP.fBestErr = fOptTotErr;
                    EmitBlock(pEP, cxt.aOptEndPts, cxt.aOptIdx);
                }
                else if (fOrgTotErr < pEP.fBestErr)
                {
                    // either it stopped fitting when we optimized it, or there was no improvement
                    // so go back to the unoptimized endpoints which we know will fit
                    if (bTransformed)
                        BC67.TransformForward(cxt.aOrgEndPts);

                    pEP.fBestErr = fOrgTotErr;
                    EmitBlock(pEP, cxt.aOrgEndPts, cxt.aOrgIdx);
                }
            }
        }

        private void GeneratePaletteUnquantized(EncodeParams pEP, uint uRegion, INTColor[] aPalette)
        {
            Debug.Assert(uRegion < BC67.BC6H_MAX_REGIONS && pEP.uShape < BC67.BC6H_MAX_SHAPES);
            BC67.INTEndPntPair endPts = pEP.aUnqEndPts[pEP.uShape][uRegion];
            byte uIndexPrec = ms_aInfo[pEP.uMode].uIndexPrec;
            byte uNumIndices = (byte)(1u << uIndexPrec);
            Debug.Assert(uNumIndices > 0);

            int[] aWeights = null;
            switch (uIndexPrec)
            {
                case 3: aWeights = BC67.g_aWeights3; Debug.Assert(uNumIndices <= 8); break;
                case 4: aWeights = BC67.g_aWeights4; Debug.Assert(uNumIndices <= 16); break;
                default:
                    Debug.Assert(false);
                    for (uint i = 0; i < uNumIndices; ++i)
                        aPalette[i] = new INTColor(0, 0, 0);
                    return;
            }

            for (uint i = 0; i < uNumIndices; ++i)
            {
                aPalette[i].r = (endPts.A.r * (BC67.BC67_WEIGHT_MAX - aWeights[i]) + endPts.B.r * aWeights[i] + BC67.BC67_WEIGHT_ROUND) >> BC67.BC67_WEIGHT_SHIFT;
                aPalette[i].g = (endPts.A.g * (BC67.BC67_WEIGHT_MAX - aWeights[i]) + endPts.B.g * aWeights[i] + BC67.BC67_WEIGHT_ROUND) >> BC67.BC67_WEIGHT_SHIFT;
                aPalette[i].b = (endPts.A.b * (BC67.BC67_WEIGHT_MAX - aWeights[i]) + endPts.B.b * aWeights[i] + BC67.BC67_WEIGHT_ROUND) >> BC67.BC67_WEIGHT_SHIFT;
            }
        }

        private float MapColors(EncodeParams pEP, uint uRegion, uint np, uint[] auIndex, Context context)
        {
            byte uIndexPrec = ms_aInfo[pEP.uMode].uIndexPrec;
            byte uNumIndices = (byte)(1U << uIndexPrec);
            GeneratePaletteUnquantized(pEP, uRegion, context.aPalette_mapColors);

            float fTotalErr = 0.0f;
            for (uint i = 0; i < np; ++i)
            {
                float fBestErr = BC67.Norm(pEP.aIPixels[auIndex[i]], context.aPalette_mapColors[0]);
                for (byte j = 1; j < uNumIndices && fBestErr > 0.0f; ++j)
                {
                    float fErr = BC67.Norm(pEP.aIPixels[auIndex[i]], context.aPalette_mapColors[j]);
                    if (fErr > fBestErr) break;      // error increased, so we're done searching
                    if (fErr < fBestErr) fBestErr = fErr;
                }
                fTotalErr += fBestErr;
            }

            return fTotalErr;
        }

        private float RoughMSE(EncodeParams pEP, Context context)
        {
            Debug.Assert(pEP.uShape < BC67.BC6H_MAX_SHAPES);

            BC67.INTEndPntPair[] aEndPts = pEP.aUnqEndPts[pEP.uShape];

            byte uPartitions = ms_aInfo[pEP.uMode].uPartitions;
            Debug.Assert(uPartitions < BC67.BC6H_MAX_REGIONS);

            float fError = 0.0f;
            for (uint p = 0; p <= uPartitions; ++p)
            {
                uint np = 0;
                for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
                {
                    if (BC67.g_aPartitionTable[uPartitions][pEP.uShape][i] == p)
                        context.auPixIdx[np++] = i;
                }

                // handle simple cases
                Debug.Assert(np > 0);
                if (np == 1)
                {
                    aEndPts[p].A = pEP.aIPixels[context.auPixIdx[0]];
                    aEndPts[p].B = pEP.aIPixels[context.auPixIdx[0]];
                    continue;
                }
                else if (np == 2)
                {
                    aEndPts[p].A = pEP.aIPixels[context.auPixIdx[0]];
                    aEndPts[p].B = pEP.aIPixels[context.auPixIdx[1]];
                    continue;
                }

                Color4 epA,  epB;
                BC67.OptimizeRGB(pEP.aHDRPixels, out epA, out epB, 4, np, context.auPixIdx, context);
                aEndPts[p].A.Set(epA, pEP.bSigned);
                aEndPts[p].B.Set(epB, pEP.bSigned);
                if (pEP.bSigned)
                {
                    aEndPts[p].A.Clamp(-BC67.F16MAX, BC67.F16MAX);
                    aEndPts[p].B.Clamp(-BC67.F16MAX, BC67.F16MAX);
                }
                else
                {
                    aEndPts[p].A.Clamp(0, BC67.F16MAX);
                    aEndPts[p].B.Clamp(0, BC67.F16MAX);
                }

                fError += MapColors(pEP, p, np, context.auPixIdx, context);
            }

            return fError;
        }

    }
}
