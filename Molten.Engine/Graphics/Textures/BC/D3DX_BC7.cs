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
using System.Diagnostics;

namespace Molten.Graphics.Textures
{
    // BC67 compression (16b bits per texel)
    internal class D3DX_BC7 : BC67.CBits
    {
        internal class Context : BCContext, IPoolable
        {

            public void ClearForPool()
            {

            }
        }

        class EncodeParams
        {
            internal byte uMode;
            internal BC67.LDREndPntPair[][] aEndPts; // [BC7_MAX_SHAPES][BC7_MAX_REGIONS];
            internal Color[] aLDRPixels;
            internal Color4[] aHDRPixels;

            internal EncodeParams(Color4[] aOriginal)
            {
                uMode = 0;
                aEndPts = new BC67.LDREndPntPair[BC67.BC7_MAX_SHAPES][];
                for (int i = 0; i < aEndPts.Length; i++)
                    aEndPts[i] = new BC67.LDREndPntPair[BC67.BC7_MAX_REGIONS];
                aLDRPixels = new Color[BC.NUM_PIXELS_PER_BLOCK];
                aHDRPixels = new Color4[BC.NUM_PIXELS_PER_BLOCK];
                Array.Copy(aOriginal, aHDRPixels, aHDRPixels.Length);
            }
        }

        class ModeInfo
        {
            public byte uPartitions;
            public byte uPartitionBits;
            public byte uPBits;
            public byte uRotationBits;
            public byte uIndexModeBits;
            public byte uIndexPrec;
            public byte uIndexPrec2;
            public Color RGBAPrec;
            public Color RGBAPrecWithP;

            public ModeInfo(byte _uPartitions,
            byte _uPartitionBits,
            byte _uPBits,
            byte _uRotationBits,
            byte _uIndexModeBits,
            byte _uIndexPrec,
            byte _uIndexPrec2,
            Color _RGBAPrec,
            Color _RGBAPrecWithP)
            {
                uPartitions = _uPartitions;
                uPartitionBits = _uPartitionBits;
                uPBits = _uPBits;
                uRotationBits = _uRotationBits;
                uIndexModeBits = _uIndexModeBits;
                uIndexPrec = _uIndexPrec;
                uIndexPrec2 = _uIndexPrec2;
                RGBAPrec = _RGBAPrec;
                RGBAPrecWithP = _RGBAPrecWithP;
            }
        }

        /// <summary>
        /// BC7 compression: uPartitions, uPartitionBits, uPBits, uRotationBits, uIndexModeBits, uIndexPrec, uIndexPrec2, RGBAPrec, RGBAPrecWithP
        /// </summary>
        static readonly ModeInfo[] ms_aInfo =
                {
            new ModeInfo(2, 4, 6, 0, 0, 3, 0, new Color(4,4,4,0), new Color(5,5,5,0)),
                // Mode 0: Color only, 3 Subsets, RGBP 4441 (unique P-bit), 3-bit indecies, 16 partitions
            new ModeInfo(1, 6, 2, 0, 0, 3, 0, new Color(6,6,6,0), new Color(7,7,7,0)),
                // Mode 1: Color only, 2 Subsets, RGBP 6661 (shared P-bit), 3-bit indecies, 64 partitions
            new ModeInfo(2, 6, 0, 0, 0, 2, 0, new Color(5,5,5,0), new Color(5,5,5,0)),
                // Mode 2: Color only, 3 Subsets, RGB 555, 2-bit indecies, 64 partitions
            new ModeInfo(1, 6, 4, 0, 0, 2, 0, new Color(7,7,7,0), new Color(8,8,8,0)),
                // Mode 3: Color only, 2 Subsets, RGBP 7771 (unique P-bit), 2-bits indecies, 64 partitions
            new ModeInfo(0, 0, 0, 2, 1, 2, 3, new Color(5,5,5,6), new Color(5,5,5,6)),
                // Mode 4: Color w/ Separate Alpha, 1 Subset, RGB 555, A6, 16x2/16x3-bit indices, 2-bit rotation, 1-bit index selector
            new ModeInfo(0, 0, 0, 2, 0, 2, 2, new Color(7,7,7,8), new Color(7,7,7,8)),
                // Mode 5: Color w/ Separate Alpha, 1 Subset, RGB 777, A8, 16x2/16x2-bit indices, 2-bit rotation
            new ModeInfo(0, 0, 2, 0, 0, 4, 0, new Color(7,7,7,7), new Color(8,8,8,8)),
                // Mode 6: Color+Alpha, 1 Subset, RGBAP 77771 (unique P-bit), 16x4-bit indecies
            new ModeInfo(1, 6, 4, 0, 0, 2, 0, new Color(5,5,5,5), new Color(6,6,6,6))
            // Mode 7: Color+Alpha, 2 Subsets, RGBAP 55551 (unique P-bit), 2-bit indices, 64 partitions
        };

        public D3DX_BC7() : base(16) { }

        public Color4[] Decode(Logger log)
        {
            Color4[] pOut = new Color4[BC.NUM_PIXELS_PER_BLOCK];
            uint uFirst = 0;
            while (uFirst < 128 && GetBit(ref uFirst) <= 0) { }
            byte uMode = (byte)(uFirst - 1);

            if (uMode < 8)
            {
                byte uPartitions = ms_aInfo[uMode].uPartitions;
                Debug.Assert(uPartitions < BC67.BC7_MAX_REGIONS);

                byte uNumEndPts = (byte)((uPartitions + 1u) << 1);
                byte uIndexPrec = ms_aInfo[uMode].uIndexPrec;
                byte uIndexPrec2 = ms_aInfo[uMode].uIndexPrec2;
                uint i;
                uint uStartBit = uMode + 1U;
                byte[] P = new byte[6];
                byte uShape = GetBits(ref uStartBit, ms_aInfo[uMode].uPartitionBits);
                Debug.Assert(uShape < BC67.BC7_MAX_SHAPES);

                byte uRotation = GetBits(ref uStartBit, ms_aInfo[uMode].uRotationBits);
                Debug.Assert(uRotation < 4);

                byte uIndexMode = GetBits(ref uStartBit, ms_aInfo[uMode].uIndexModeBits);
                Debug.Assert(uIndexMode < 2);

                Color[] c = new Color[BC67.BC7_MAX_REGIONS << 1];
                Color RGBAPrec = ms_aInfo[uMode].RGBAPrec;
                Color RGBAPrecWithP = ms_aInfo[uMode].RGBAPrecWithP;

                Debug.Assert(uNumEndPts <= (BC67.BC7_MAX_REGIONS << 1));

                // Red channel
                for (i = 0; i < uNumEndPts; i++)
                {
                    if (uStartBit + RGBAPrec.R > 128)
                    {
# if DEBUG
                        log.Error("BC7: Invalid block encountered during decoding\n");
#endif
                        BC67.FillWithErrorColors(pOut);
                        return pOut;
                    }

                    c[i].R = GetBits(ref uStartBit, RGBAPrec.R);
                }

                // Green channel
                for (i = 0; i < uNumEndPts; i++)
                {
                    if (uStartBit + RGBAPrec.G > 128)
                    {
# if DEBUG
                        log.Error("BC7: Invalid block encountered during decoding\n");
#endif
                        BC67.FillWithErrorColors(pOut);
                        return pOut;
                    }

                    c[i].G = GetBits(ref uStartBit, RGBAPrec.G);
                }

                // Blue channel
                for (i = 0; i < uNumEndPts; i++)
                {
                    if (uStartBit + RGBAPrec.B > 128)
                    {
# if DEBUG
                        log.Error("BC7: Invalid block encountered during decoding\n");
#endif
                        BC67.FillWithErrorColors(pOut);
                        return pOut;
                    }

                    c[i].B = GetBits(ref uStartBit, RGBAPrec.B);
                }

                // Alpha channel
                for (i = 0; i < uNumEndPts; i++)
                {
                    if (uStartBit + RGBAPrec.A > 128)
                    {
# if DEBUG
                        log.Error("BC7: Invalid block encountered during decoding\n");
#endif
                        BC67.FillWithErrorColors(pOut);
                        return pOut;
                    }

                    c[i].A = (byte)(RGBAPrec.A > 0 ? GetBits(ref uStartBit, RGBAPrec.A) : 255);
                }

                // P-bits
                Debug.Assert(ms_aInfo[uMode].uPBits <= 6);
                for (i = 0; i < ms_aInfo[uMode].uPBits; i++)
                {
                    if (uStartBit > 127)
                    {
# if DEBUG
                        log.Error("BC7: Invalid block encountered during decoding\n");
#endif
                        BC67.FillWithErrorColors(pOut);
                        return pOut;
                    }

                    P[i] = GetBit(ref uStartBit);
                }

                if (ms_aInfo[uMode].uPBits > 0)
                {
                    for (i = 0; i < uNumEndPts; i++)
                    {
                        uint pi = i * ms_aInfo[uMode].uPBits / uNumEndPts;
                        for (byte ch = 0; ch < BC67.BC7_NUM_CHANNELS; ch++)
                        {
                            if (RGBAPrec[ch] != RGBAPrecWithP[ch])
                            {
                                c[i][ch] = (byte)((c[i][ch] << 1) | P[pi]);
                            }
                        }
                    }
                }

                for (i = 0; i < uNumEndPts; i++)
                {
                    c[i] = Unquantize(c[i], RGBAPrecWithP);
                }

                byte[] w1 = new byte[BC.NUM_PIXELS_PER_BLOCK];
                byte[] w2 = new byte[BC.NUM_PIXELS_PER_BLOCK];

                // read color indices
                for (i = 0; i < BC.NUM_PIXELS_PER_BLOCK; i++)
                {
                    uint uNumBits = BC67.IsFixUpOffset(ms_aInfo[uMode].uPartitions, uShape, i) ? uIndexPrec - 1U : uIndexPrec;
                    if (uStartBit + uNumBits > 128)
                    {
# if DEBUG
                        log.Error("BC7: Invalid block encountered during decoding\n");
#endif
                        BC67.FillWithErrorColors(pOut);
                        return pOut;
                    }
                    w1[i] = GetBits(ref uStartBit, uNumBits);
                }

                // read alpha indices
                if (uIndexPrec2 > 0)
                {
                    for (i = 0; i < BC.NUM_PIXELS_PER_BLOCK; i++)
                    {
                        uint uNumBits = i > 0 ? uIndexPrec2 : uIndexPrec2 - 1U;
                        if (uStartBit + uNumBits > 128)
                        {
# if DEBUG
                            log.Error("BC7: Invalid block encountered during decoding\n");
#endif
                            BC67.FillWithErrorColors(pOut);
                            return pOut;
                        }
                        w2[i] = GetBits(ref uStartBit, uNumBits);
                    }
                }

                for (i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
                {
                    byte uRegion = BC67.g_aPartitionTable[uPartitions][uShape][i];
                    Color outPixel = new Color();
                    if (uIndexPrec2 == 0)
                    {
                        BC67.Interpolate(c[uRegion << 1], c[(uRegion << 1) + 1], w1[i], w1[i], uIndexPrec, uIndexPrec, ref outPixel);
                    }
                    else
                    {
                        if (uIndexMode == 0)
                        {
                            BC67.Interpolate(c[uRegion << 1], c[(uRegion << 1) + 1], w1[i], w2[i], uIndexPrec, uIndexPrec2, ref outPixel);
                        }
                        else
                        {
                            BC67.Interpolate(c[uRegion << 1], c[(uRegion << 1) + 1], w2[i], w1[i], uIndexPrec2, uIndexPrec, ref outPixel);
                        }
                    }

                    switch (uRotation)
                    {
                        case 1: BC67.Swap(ref outPixel.R, ref outPixel.A); break;
                        case 2: BC67.Swap(ref outPixel.G, ref outPixel.A); break;
                        case 3: BC67.Swap(ref outPixel.B, ref outPixel.A); break;
                    }

                    pOut[i] = (Color4)outPixel;
                }
            }
            else
            {
#if DEBUG
                log.Error("BC7: Reserved mode 8 encountered during decoding\n");
#endif
                // Per the BC7 format spec, we must return transparent black
                for (int i = 0; i < pOut.Length; i++)
                    pOut[i] = new Color4();
            }

            return pOut;
        }

        public void Encode(BCFlags flags, Color4[] pIn, Context context)
        {
            byte[] final = new byte[m_uBits.Length];
            Buffer.BlockCopy(m_uBits, 0, final, 0, final.Length);

            EncodeParams EP = new EncodeParams(pIn);
            float fMSEBest = float.MaxValue;
            uint alphaMask = 0xFF;

            for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
            {
                EP.aLDRPixels[i].R = (byte)Math.Max(0.0f, Math.Min(255.0f, pIn[i].R * 255.0f + 0.01f));
                EP.aLDRPixels[i].G = (byte)Math.Max(0.0f, Math.Min(255.0f, pIn[i].G * 255.0f + 0.01f));
                EP.aLDRPixels[i].B = (byte)Math.Max(0.0f, Math.Min(255.0f, pIn[i].B * 255.0f + 0.01f));
                EP.aLDRPixels[i].A = (byte)Math.Max(0.0f, Math.Min(255.0f, pIn[i].A * 255.0f + 0.01f));
                alphaMask &= EP.aLDRPixels[i].A;
            }

            bool bHasAlpha = (alphaMask != 0xFF);

            for (EP.uMode = 0; EP.uMode < 8 && fMSEBest > 0; ++EP.uMode)
            {

                // 3 subset modes tend to be used rarely and add significant compression time
                if (!BC.HasFlags(flags, BCFlags.USE_3SUBSETS) && (EP.uMode == 0 || EP.uMode == 2))
                    continue;

                if (BC.HasFlags(flags, BCFlags.BC7_QUICK) && (EP.uMode != 6))
                    continue;

                // There is no value in using mode 7 for completely opaque blocks (the other 2 subset modes handle this case for opaque blocks), so skip it for a small perf win.
                if ((!bHasAlpha) && (EP.uMode == 7))
                    continue;

                uint uShapes = 1U << ms_aInfo[EP.uMode].uPartitionBits;
                Debug.Assert(uShapes <= BC67.BC7_MAX_SHAPES);

                uint uNumRots = 1U << ms_aInfo[EP.uMode].uRotationBits;
                uint uNumIdxMode = 1U << ms_aInfo[EP.uMode].uIndexModeBits;
                // Number of rough cases to look at. reasonable values of this are 1, uShapes/4, and uShapes
                // uShapes/4 gets nearly all the cases; you can increase that a bit (say by 3 or 4) if you really want to squeeze the last bit out
                uint uItems = Math.Max(1, uShapes >> 2);
                float[] afRoughMSE = new float[BC67.BC7_MAX_SHAPES];
                uint[] auShape = new uint[BC67.BC7_MAX_SHAPES];

                for (uint r = 0; r < uNumRots && fMSEBest > 0; ++r)
                {
                    switch (r)
                    {
                        case 1: for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; i++) BC67.Swap(ref EP.aLDRPixels[i].R, ref EP.aLDRPixels[i].A); break;
                        case 2: for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; i++) BC67.Swap(ref EP.aLDRPixels[i].G, ref EP.aLDRPixels[i].A); break;
                        case 3: for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; i++) BC67.Swap(ref EP.aLDRPixels[i].B, ref EP.aLDRPixels[i].A); break;
                    }

                    for (uint im = 0; im < uNumIdxMode && fMSEBest > 0; ++im)
                    {
                        // pick the best uItems shapes and refine these.
                        for (uint s = 0; s < uShapes; s++)
                        {
                            afRoughMSE[s] = RoughMSE(EP, s, im, context);
                            auShape[s] = s;
                        }

                        // Bubble up the first uItems items
                        for (uint i = 0; i < uItems; i++)
                        {
                            for (uint j = i + 1; j < uShapes; j++)
                            {
                                if (afRoughMSE[i] > afRoughMSE[j])
                                {
                                    BC67.Swap(ref afRoughMSE[i], ref afRoughMSE[j]);
                                    BC67.Swap(ref auShape[i], ref auShape[j]);
                                }
                            }
                        }

                        for (uint i = 0; i < uItems && fMSEBest > 0; i++)
                        {
                            float fMSE = Refine(EP, auShape[i], r, im);
                            if (fMSE < fMSEBest)
                            {
                                Buffer.BlockCopy(m_uBits, 0, final, 0, final.Length); // Reset data. //final = *this;
                                fMSEBest = fMSE;
                            }
                        }
                    }

                    switch (r)
                    {
                        case 1: for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; i++) BC67.Swap(ref EP.aLDRPixels[i].R, ref EP.aLDRPixels[i].A); break;
                        case 2: for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; i++) BC67.Swap(ref EP.aLDRPixels[i].G, ref EP.aLDRPixels[i].A); break;
                        case 3: for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; i++) BC67.Swap(ref EP.aLDRPixels[i].B, ref EP.aLDRPixels[i].A); break;
                    }
                }
            }

            Buffer.BlockCopy(final, 0, m_uBits, 0, final.Length);  // Copy final data back into this.m_uBits. // *this = final;
        }

#pragma warning(pop)

        private static byte Quantize(byte comp, byte uPrec)
        {
            Debug.Assert(0 < uPrec && uPrec <= 8);
            byte rnd = (byte)Math.Min(255u, comp + (1u << (7 - uPrec)));
            return (byte)(rnd >> (8 - uPrec));
        }

        private static Color Quantize(Color c, Color RGBAPrec)
        {
            Color q;
            q.R = Quantize(c.R, RGBAPrec.R);
            q.G = Quantize(c.G, RGBAPrec.G);
            q.B = Quantize(c.B, RGBAPrec.B);
            if (RGBAPrec.A > 0)
                q.A = Quantize(c.A, RGBAPrec.A);
            else
                q.A = 255;
            return q;
        }

        private static byte Unquantize(byte comp, uint uPrec)
        {
            Debug.Assert(0 < uPrec && uPrec <= 8);
            comp = (byte)(comp << (8 - (int)uPrec));
            return (byte)(comp | (comp >> (int)uPrec));
        }

        private static Color Unquantize(Color c, Color RGBAPrec)
        {
            return new Color()
            {
                R = Unquantize(c.R, RGBAPrec.R),
                G = Unquantize(c.G, RGBAPrec.G),
                B = Unquantize(c.B, RGBAPrec.B),
                A = (byte)(RGBAPrec.A > 0 ? Unquantize(c.A, RGBAPrec.A) : 255),
            };
        }

        private static void GeneratePaletteQuantized(EncodeParams pEP, uint uIndexMode, BC67.LDREndPntPair endPts, Color[] aPalette)
        {
            uint uIndexPrec = uIndexMode > 0 ? ms_aInfo[pEP.uMode].uIndexPrec2 : ms_aInfo[pEP.uMode].uIndexPrec;
            uint uIndexPrec2 = uIndexMode > 0 ? ms_aInfo[pEP.uMode].uIndexPrec : ms_aInfo[pEP.uMode].uIndexPrec2;
            uint uNumIndices = 1U << (int)uIndexPrec;
            uint uNumIndices2 = 1U << (int)uIndexPrec2;
            Debug.Assert(uNumIndices > 0 && uNumIndices2 > 0);
            Debug.Assert((uNumIndices <= BC67.BC7_MAX_INDICES) && (uNumIndices2 <= BC67.BC7_MAX_INDICES));

            Color a = Unquantize(endPts.A, ms_aInfo[pEP.uMode].RGBAPrecWithP);
            Color b = Unquantize(endPts.B, ms_aInfo[pEP.uMode].RGBAPrecWithP);
            if (uIndexPrec2 == 0)
            {
                for (uint i = 0; i < uNumIndices; i++)
                    BC67.Interpolate(a, b, i, i, uIndexPrec, uIndexPrec, ref aPalette[i]);
            }
            else
            {
                for (uint i = 0; i < uNumIndices; i++)
                    BC67.InterpolateRGB(a, b, i, uIndexPrec, ref aPalette[i]);
                for (uint i = 0; i < uNumIndices2; i++)
                    BC67.InterpolateA(a, b, i, uIndexPrec2, ref aPalette[i]);
            }
        }

        private float PerturbOne(EncodeParams pEP, Color[] aColors, uint np, uint uIndexMode, int ch,
            BC67.LDREndPntPair oldEndPts, out BC67.LDREndPntPair newEndPts, float fOldErr, byte do_b)
        {
            int prec = ms_aInfo[pEP.uMode].RGBAPrecWithP[ch];
            BC67.LDREndPntPair tmp_endPts = newEndPts = oldEndPts;
            float fMinErr = fOldErr;
            byte pnew_c = (do_b > 0 ? newEndPts.B[ch] : newEndPts.A[ch]);
            byte ptmp_c = (do_b > 0 ? tmp_endPts.B[ch] : tmp_endPts.A[ch]);

            // do a logarithmic search for the best error for this endpoint (which)
            for (int step = 1 << (prec - 1); step > 0; step >>= 1)
            {
                bool bImproved = false;
                int beststep = 0;
                for (int sign = -1; sign <= 1; sign += 2)
                {
                    int tmp = pnew_c + sign * step;
                    if (tmp < 0 || tmp >= (1 << prec))
                        continue;
                    else
                        ptmp_c = (byte)tmp;

                    float fTotalErr = MapColors(pEP, aColors, np, uIndexMode, tmp_endPts, fMinErr);
                    if (fTotalErr < fMinErr)
                    {
                        bImproved = true;
                        fMinErr = fTotalErr;
                        beststep = sign * step;
                    }
                }

                // if this was an improvement, move the endpoint and continue search from there
                if (bImproved)
                    pnew_c += (byte)beststep;
            }
            return fMinErr;
        }

        // perturb the endpoints at least -3 to 3.
        // always ensure endpoint ordering is preserved (no need to overlap the scan)
        private void Exhaustive(EncodeParams pEP, Color[] aColors, uint np, uint uIndexMode, int ch,
            float fOrgErr, ref BC67.LDREndPntPair optEndPt)
        {
            byte uPrec = ms_aInfo[pEP.uMode].RGBAPrecWithP[ch];
            BC67.LDREndPntPair tmpEndPt;
            if (fOrgErr == 0)
                return;

            int delta = 5;

            // ok figure out the range of A and B
            tmpEndPt = optEndPt;
            int alow = Math.Max(0, optEndPt.A[ch] - delta);
            int ahigh = Math.Min((1 << uPrec) - 1, optEndPt.A[ch] + delta);
            int blow = Math.Max(0, optEndPt.B[ch] - delta);
            int bhigh = Math.Min((1 << uPrec) - 1, optEndPt.B[ch] + delta);
            int amin = 0;
            int bmin = 0;

            float fBestErr = fOrgErr;
            if (optEndPt.A[ch] <= optEndPt.B[ch])
            {
                // keep a <= b
                for (int a = alow; a <= ahigh; ++a)
                {
                    for (int b = Math.Max(a, blow); b < bhigh; ++b)
                    {
                        tmpEndPt.A[ch] = (byte)a;
                        tmpEndPt.B[ch] = (byte)b;

                        float fErr = MapColors(pEP, aColors, np, uIndexMode, tmpEndPt, fBestErr);
                        if (fErr < fBestErr)
                        {
                            amin = a;
                            bmin = b;
                            fBestErr = fErr;
                        }
                    }
                }
            }
            else
            {
                // keep b <= a
                for (int b = blow; b < bhigh; ++b)
                {
                    for (int a = Math.Max(b, alow); a <= ahigh; ++a)
                    {
                        tmpEndPt.A[ch] = (byte)a;
                        tmpEndPt.B[ch] = (byte)b;

                        float fErr = MapColors(pEP, aColors, np, uIndexMode, tmpEndPt, fBestErr);
                        if (fErr < fBestErr)
                        {
                            amin = a;
                            bmin = b;
                            fBestErr = fErr;
                        }
                    }
                }
            }

            if (fBestErr < fOrgErr)
            {
                optEndPt.A[ch] = (byte)amin;
                optEndPt.B[ch] = (byte)bmin;
                fOrgErr = fBestErr;
            }
        }

        private void OptimizeOne(EncodeParams pEP, Color[] aColors, uint np, uint uIndexMode,
            float fOrgErr, BC67.LDREndPntPair org, out BC67.LDREndPntPair opt)
        {
            float fOptErr = fOrgErr;
            opt = org;

            BC67.LDREndPntPair new_a, new_b;
            BC67.LDREndPntPair newEndPts;
            byte do_b;

            // now optimize each channel separately
            for (int ch = 0; ch < BC67.BC7_NUM_CHANNELS; ++ch)
            {
                if (ms_aInfo[pEP.uMode].RGBAPrecWithP[ch] == 0)
                    continue;

                // figure out which endpoint when perturbed gives the most improvement and start there
                // if we just alternate, we can easily end up in a local minima
                float fErr0 = PerturbOne(pEP, aColors, np, uIndexMode, ch, opt, out new_a, fOptErr, 0); // perturb endpt A
                float fErr1 = PerturbOne(pEP, aColors, np, uIndexMode, ch, opt, out new_b, fOptErr, 1); // perturb endpt B

                byte copt_a = opt.A[ch];
                byte copt_b = opt.B[ch];
                byte cnew_a = new_a.A[ch];
                byte cnew_b = new_a.B[ch];

                if (fErr0 < fErr1)
                {
                    if (fErr0 >= fOptErr)
                        continue;
                    copt_a = cnew_a;
                    fOptErr = fErr0;
                    do_b = 1;       // do B next
                }
                else
                {
                    if (fErr1 >= fOptErr)
                        continue;
                    copt_b = cnew_b;
                    fOptErr = fErr1;
                    do_b = 0;       // do A next
                }

                // now alternate endpoints and keep trying until there is no improvement
                for (; ; )
                {
                    float fErr = PerturbOne(pEP, aColors, np, uIndexMode, ch, opt, out newEndPts, fOptErr, do_b);
                    if (fErr >= fOptErr)
                        break;
                    if (do_b == 0)
                        copt_a = cnew_a;
                    else
                        copt_b = cnew_b;
                    fOptErr = fErr;
                    do_b = (byte)(1 - do_b);    // now move the other endpoint
                }
            }

            // finally, do a small exhaustive search around what we think is the global minima to be sure
            for (int ch = 0; ch < BC67.BC7_NUM_CHANNELS; ch++)
                Exhaustive(pEP, aColors, np, uIndexMode, ch, fOptErr, ref opt);
        }

        private void OptimizeEndPoints(EncodeParams pEP, uint uShape, uint uIndexMode, float[] afOrgErr,
            BC67.LDREndPntPair[] aOrgEndPts, BC67.LDREndPntPair[] aOptEndPts)
        {
            byte uPartitions = ms_aInfo[pEP.uMode].uPartitions;
            Debug.Assert(uPartitions < BC67.BC7_MAX_REGIONS && uShape < BC67.BC7_MAX_SHAPES);

            Color[] aPixels = new Color[BC.NUM_PIXELS_PER_BLOCK];

            for (uint p = 0; p <= uPartitions; ++p)
            {
                // collect the pixels in the region
                uint np = 0;
                for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
                    if (BC67.g_aPartitionTable[uPartitions][uShape][i] == p)
                        aPixels[np++] = pEP.aLDRPixels[i];

                OptimizeOne(pEP, aPixels, np, uIndexMode, afOrgErr[p], aOrgEndPts[p], out aOptEndPts[p]);
            }
        }

        private void AssignIndices(EncodeParams pEP, uint uShape, uint uIndexMode, BC67.LDREndPntPair[] endPts, uint[] aIndices, uint[] aIndices2,
            float[] afTotErr)
        {
            Debug.Assert(uShape < BC67.BC7_MAX_SHAPES);

            byte uPartitions = ms_aInfo[pEP.uMode].uPartitions;
            Debug.Assert(uPartitions < BC67.BC7_MAX_REGIONS);

            byte uIndexPrec = uIndexMode > 0 ? ms_aInfo[pEP.uMode].uIndexPrec2 : ms_aInfo[pEP.uMode].uIndexPrec;
            byte uIndexPrec2 = uIndexMode > 0 ? ms_aInfo[pEP.uMode].uIndexPrec : ms_aInfo[pEP.uMode].uIndexPrec2;
            byte uNumIndices = (byte)(1u << uIndexPrec);
            byte uNumIndices2 = (byte)(1u << uIndexPrec2);

            Debug.Assert((uNumIndices <= BC67.BC7_MAX_INDICES) && (uNumIndices2 <= BC67.BC7_MAX_INDICES));

            byte uHighestIndexBit = (byte)(uNumIndices >> 1);
            byte uHighestIndexBit2 = (byte)(uNumIndices2 >> 1);
            Color[][] aPalette = new Color[BC67.BC7_MAX_REGIONS][];
            for (int i = 0; i < aPalette.Length; i++)
                aPalette[i] = new Color[BC67.BC7_MAX_INDICES];

            // build list of possibles
            for (uint p = 0; p <= uPartitions; p++)
            {
                GeneratePaletteQuantized(pEP, uIndexMode, endPts[p], aPalette[p]);
                afTotErr[p] = 0;
            }

            for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; i++)
            {
                byte uRegion = BC67.g_aPartitionTable[uPartitions][uShape][i];
                Debug.Assert(uRegion < BC67.BC7_MAX_REGIONS);
                afTotErr[uRegion] += BC67.ComputeError(pEP.aLDRPixels[i], aPalette[uRegion], uIndexPrec, uIndexPrec2, out aIndices[i], out aIndices2[i]);
            }

            // swap endpoints as needed to ensure that the indices at index_positions have a 0 high-order bit
            if (uIndexPrec2 == 0)
            {
                for (uint p = 0; p <= uPartitions; p++)
                {
                    if ((aIndices[BC67.g_aFixUp[uPartitions][uShape][p]] & uHighestIndexBit) == uHighestIndexBit)
                    {
                        BC67.Swap(ref endPts[p].A, ref endPts[p].B);
                        for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; i++)
                            if (BC67.g_aPartitionTable[uPartitions][uShape][i] == p)
                                aIndices[i] = uNumIndices - 1U - aIndices[i];
                    }
                    Debug.Assert((aIndices[BC67.g_aFixUp[uPartitions][uShape][p]] & uHighestIndexBit) == 0);
                }
            }
            else
            {
                for (uint p = 0; p <= uPartitions; p++)
                {
                    if ((aIndices[BC67.g_aFixUp[uPartitions][uShape][p]] & uHighestIndexBit) == uHighestIndexBit)
                    {
                        BC67.Swap(ref endPts[p].A.R, ref endPts[p].B.R);
                        BC67.Swap(ref endPts[p].A.G, ref endPts[p].B.G);
                        BC67.Swap(ref endPts[p].A.B, ref endPts[p].B.B);
                        for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; i++)
                            if (BC67.g_aPartitionTable[uPartitions][uShape][i] == p)
                                aIndices[i] = uNumIndices - 1U - aIndices[i];
                    }
                    Debug.Assert((aIndices[BC67.g_aFixUp[uPartitions][uShape][p]] & uHighestIndexBit) == 0);

                    if ((aIndices2[0] & uHighestIndexBit2) == uHighestIndexBit2)
                    {
                        BC67.Swap(ref endPts[p].A.A, ref endPts[p].B.A);
                        for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; i++)
                            aIndices2[i] = uNumIndices2 - 1U - aIndices2[i];
                    }
                    Debug.Assert((aIndices2[0] & uHighestIndexBit2) == 0);
                }
            }
        }

        private void EmitBlock(EncodeParams pEP, uint uShape, uint uRotation, uint uIndexMode, BC67.LDREndPntPair[] aEndPts, uint[] aIndex, uint[] aIndex2)
        {
            byte uPartitions = ms_aInfo[pEP.uMode].uPartitions;
            Debug.Assert(uPartitions < BC67.BC7_MAX_REGIONS);

            uint uPBits = ms_aInfo[pEP.uMode].uPBits;
            uint uIndexPrec = ms_aInfo[pEP.uMode].uIndexPrec;
            uint uIndexPrec2 = ms_aInfo[pEP.uMode].uIndexPrec2;
            Color RGBAPrec = ms_aInfo[pEP.uMode].RGBAPrec;
            Color RGBAPrecWithP = ms_aInfo[pEP.uMode].RGBAPrecWithP;
            uint i;
            uint uStartBit = 0;
            SetBits(ref uStartBit, pEP.uMode, 0);
            SetBits(ref uStartBit, 1U, 1);
            SetBits(ref uStartBit, ms_aInfo[pEP.uMode].uRotationBits, (byte)(uRotation));
            SetBits(ref uStartBit, ms_aInfo[pEP.uMode].uIndexModeBits, (byte)(uIndexMode));
            SetBits(ref uStartBit, ms_aInfo[pEP.uMode].uPartitionBits, (byte)(uShape));

            if (uPBits > 0)
            {
                uint uNumEP = (uPartitions + 1U) << 1;
                byte[] aPVote = { 0, 0, 0, 0, 0, 0 };
                byte[] aCount = { 0, 0, 0, 0, 0, 0 };
                for (byte ch = 0; ch < BC67.BC7_NUM_CHANNELS; ch++)
                {
                    byte ep = 0;
                    for (i = 0; i <= uPartitions; i++)
                    {
                        if (RGBAPrec[ch] == RGBAPrecWithP[ch])
                        {
                            SetBits(ref uStartBit, RGBAPrec[ch], aEndPts[i].A[ch]);
                            SetBits(ref uStartBit, RGBAPrec[ch], aEndPts[i].B[ch]);
                        }
                        else
                        {
                            SetBits(ref uStartBit, RGBAPrec[ch], (byte)(aEndPts[i].A[ch] >> 1));
                            SetBits(ref uStartBit, RGBAPrec[ch], (byte)(aEndPts[i].B[ch] >> 1));
                            uint idx = ep++ * uPBits / uNumEP;
                            Debug.Assert(idx < (BC67.BC7_MAX_REGIONS << 1));
                            aPVote[idx] += (byte)(aEndPts[i].A[ch] & 0x01);
                            aCount[idx]++;
                            idx = ep++ * uPBits / uNumEP;
                            Debug.Assert(idx < (BC67.BC7_MAX_REGIONS << 1));
                            aPVote[idx] += (byte)(aEndPts[i].B[ch] & 0x01);
                            aCount[idx]++;
                        }
                    }
                }

                for (i = 0; i < uPBits; i++)
                {
                    SetBits(ref uStartBit, 1, (byte)(aPVote[i] > (aCount[i] >> 1) ? 1 : 0));
                }
            }
            else
            {
                for (int ch = 0; ch < BC67.BC7_NUM_CHANNELS; ch++)
                {
                    for (i = 0; i <= uPartitions; i++)
                    {
                        SetBits(ref uStartBit, RGBAPrec[ch], aEndPts[i].A[ch]);
                        SetBits(ref uStartBit, RGBAPrec[ch], aEndPts[i].B[ch]);
                    }
                }
            }

            uint[] aI1 = uIndexMode > 0 ? aIndex2 : aIndex;
            uint[] aI2 = uIndexMode > 0 ? aIndex : aIndex2;
            for (i = 0; i < BC.NUM_PIXELS_PER_BLOCK; i++)
            {
                if (BC67.IsFixUpOffset(ms_aInfo[pEP.uMode].uPartitions, uShape, i))
                    SetBits(ref uStartBit, uIndexPrec - 1, (byte)aI1[i]);
                else
                    SetBits(ref uStartBit, uIndexPrec, (byte)aI1[i]);
            }
            if (uIndexPrec2 > 0)
                for (i = 0; i < BC.NUM_PIXELS_PER_BLOCK; i++)
                    SetBits(ref uStartBit, i > 0 ? uIndexPrec2 : uIndexPrec2 - 1, (byte)aI2[i]);

            Debug.Assert(uStartBit == 128);
        }

        private void FixEndpointPBits(EncodeParams pEP, BC67.LDREndPntPair[] pOrigEndpoints, BC67.LDREndPntPair[] pFixedEndpoints)
        {
            uint uPartitions = ms_aInfo[pEP.uMode].uPartitions;

            pFixedEndpoints[0] = pOrigEndpoints[0];
            pFixedEndpoints[1] = pOrigEndpoints[1];
            pFixedEndpoints[2] = pOrigEndpoints[2];

            uint uPBits = ms_aInfo[pEP.uMode].uPBits;

            if (uPBits > 0)
            {
                uint uNumEP = (1 + uPartitions) << 1;
                byte[] aPVote = { 0, 0, 0, 0, 0, 0 };
                byte[] aCount = { 0, 0, 0, 0, 0, 0 };

                Color RGBAPrec = ms_aInfo[pEP.uMode].RGBAPrec;
                Color RGBAPrecWithP = ms_aInfo[pEP.uMode].RGBAPrecWithP;

                for (byte ch = 0; ch < BC67.BC7_NUM_CHANNELS; ch++)
                {
                    byte ep = 0;
                    for (uint i = 0; i <= uPartitions; i++)
                    {
                        if (RGBAPrec[ch] == RGBAPrecWithP[ch])
                        {
                            pFixedEndpoints[i].A[ch] = pOrigEndpoints[i].A[ch];
                            pFixedEndpoints[i].B[ch] = pOrigEndpoints[i].B[ch];
                        }
                        else
                        {
                            pFixedEndpoints[i].A[ch] = (byte)(pOrigEndpoints[i].A[ch] >> 1);
                            pFixedEndpoints[i].B[ch] = (byte)(pOrigEndpoints[i].B[ch] >> 1);

                            uint idx = ep++ * uPBits / uNumEP;
                            Debug.Assert(idx < (BC67.BC7_MAX_REGIONS << 1));
                            aPVote[idx] += (byte)(pOrigEndpoints[i].A[ch] & 0x01);
                            aCount[idx]++;
                            idx = ep++ * uPBits / uNumEP;
                            Debug.Assert(idx < (BC67.BC7_MAX_REGIONS << 1));
                            aPVote[idx] += (byte)(pOrigEndpoints[i].B[ch] & 0x01);
                            aCount[idx]++;
                        }
                    }
                }

                // Compute the actual pbits we'll use when we encode block. Note this is not 
                // rounding the component indices correctly in cases the pbits != a component's LSB.
                int[] pbits = new int[BC67.BC7_MAX_REGIONS << 1];
                for (uint i = 0; i < uPBits; i++)
                    pbits[i] = aPVote[i] > (aCount[i] >> 1) ? 1 : 0;

                // Now calculate the actual endpoints with proper pbits, so error calculations are accurate.
                if (pEP.uMode == 1)
                {
                    // shared pbits
                    for (byte ch = 0; ch < BC67.BC7_NUM_CHANNELS; ch++)
                    {
                        for (uint i = 0; i <= uPartitions; i++)
                        {
                            pFixedEndpoints[i].A[ch] = (byte)((pFixedEndpoints[i].A[ch] << 1) | pbits[i]);
                            pFixedEndpoints[i].B[ch] = (byte)((pFixedEndpoints[i].B[ch] << 1) | pbits[i]);
                        }
                    }
                }
                else
                {
                    for (byte ch = 0; ch < BC67.BC7_NUM_CHANNELS; ch++)
                    {
                        for (uint i = 0; i <= uPartitions; i++)
                        {
                            pFixedEndpoints[i].A[ch] = (byte)((pFixedEndpoints[i].A[ch] << 1) | pbits[i * 2 + 0]);
                            pFixedEndpoints[i].B[ch] = (byte)((pFixedEndpoints[i].B[ch] << 1) | pbits[i * 2 + 1]);
                        }
                    }
                }
            }
        }

        private float Refine(EncodeParams pEP, uint uShape, uint uRotation, uint uIndexMode)
        {
            Debug.Assert(uShape < BC67.BC7_MAX_SHAPES);

            uint uPartitions = ms_aInfo[pEP.uMode].uPartitions;
            Debug.Assert(uPartitions < BC67.BC7_MAX_REGIONS);

            BC67.LDREndPntPair[] aOrgEndPts = new BC67.LDREndPntPair[BC67.BC7_MAX_REGIONS];
            BC67.LDREndPntPair[] aOptEndPts = new BC67.LDREndPntPair[BC67.BC7_MAX_REGIONS];
            uint[] aOrgIdx = new uint[BC.NUM_PIXELS_PER_BLOCK];
            uint[] aOrgIdx2 = new uint[BC.NUM_PIXELS_PER_BLOCK];
            uint[] aOptIdx = new uint[BC.NUM_PIXELS_PER_BLOCK];
            uint[] aOptIdx2 = new uint[BC.NUM_PIXELS_PER_BLOCK];
            float[] aOrgErr = new float[BC67.BC7_MAX_REGIONS];
            float[] aOptErr = new float[BC67.BC7_MAX_REGIONS];

            BC67.LDREndPntPair[] aEndPts = pEP.aEndPts[uShape];

            for (uint p = 0; p <= uPartitions; p++)
            {
                aOrgEndPts[p].A = Quantize(aEndPts[p].A, ms_aInfo[pEP.uMode].RGBAPrecWithP);
                aOrgEndPts[p].B = Quantize(aEndPts[p].B, ms_aInfo[pEP.uMode].RGBAPrecWithP);
            }

            BC67.LDREndPntPair[] newEndPts1 = new BC67.LDREndPntPair[BC67.BC7_MAX_REGIONS];
            FixEndpointPBits(pEP, aOrgEndPts, newEndPts1);

            AssignIndices(pEP, uShape, uIndexMode, newEndPts1, aOrgIdx, aOrgIdx2, aOrgErr);

            OptimizeEndPoints(pEP, uShape, uIndexMode, aOrgErr, newEndPts1, aOptEndPts);

            BC67.LDREndPntPair[] newEndPts2 = new BC67.LDREndPntPair[BC67.BC7_MAX_REGIONS];
            FixEndpointPBits(pEP, aOptEndPts, newEndPts2);

            AssignIndices(pEP, uShape, uIndexMode, newEndPts2, aOptIdx, aOptIdx2, aOptErr);

            float fOrgTotErr = 0, fOptTotErr = 0;
            for (uint p = 0; p <= uPartitions; p++)
            {
                fOrgTotErr += aOrgErr[p];
                fOptTotErr += aOptErr[p];
            }
            if (fOptTotErr < fOrgTotErr)
            {
                EmitBlock(pEP, uShape, uRotation, uIndexMode, newEndPts2, aOptIdx, aOptIdx2);
                return fOptTotErr;
            }
            else
            {
                EmitBlock(pEP, uShape, uRotation, uIndexMode, newEndPts1, aOrgIdx, aOrgIdx2);
                return fOrgTotErr;
            }
        }

        private float MapColors(EncodeParams pEP, Color[] aColors, uint np, uint uIndexMode, BC67.LDREndPntPair endPts, float fMinErr)
        {
            byte uIndexPrec = uIndexMode > 0 ? ms_aInfo[pEP.uMode].uIndexPrec2 : ms_aInfo[pEP.uMode].uIndexPrec;
            byte uIndexPrec2 = uIndexMode > 0 ? ms_aInfo[pEP.uMode].uIndexPrec : ms_aInfo[pEP.uMode].uIndexPrec2;
            Color[] aPalette = new Color[BC67.BC7_MAX_INDICES];
            float fTotalErr = 0;

            GeneratePaletteQuantized(pEP, uIndexMode, endPts, aPalette);
            for (uint i = 0; i < np; ++i)
            {
                uint dummy;
                fTotalErr += BC67.ComputeError(aColors[i], aPalette, uIndexPrec, uIndexPrec2, out dummy, out dummy);
                if (fTotalErr > fMinErr)   // check for early exit
                {
                    fTotalErr = float.MaxValue;
                    break;
                }
            }

            return fTotalErr;
        }

        private float RoughMSE(EncodeParams pEP, uint uShape, uint uIndexMode, Context cxt)
        {
            Debug.Assert(uShape < BC67.BC7_MAX_SHAPES);
            BC67.LDREndPntPair[] aEndPts = pEP.aEndPts[uShape];

            byte uPartitions = ms_aInfo[pEP.uMode].uPartitions;
            Debug.Assert(uPartitions < BC67.BC7_MAX_REGIONS);

            byte uIndexPrec = uIndexMode > 0 ? ms_aInfo[pEP.uMode].uIndexPrec2 : ms_aInfo[pEP.uMode].uIndexPrec;
            byte uIndexPrec2 = uIndexMode > 0 ? ms_aInfo[pEP.uMode].uIndexPrec : ms_aInfo[pEP.uMode].uIndexPrec2;
            byte uNumIndices = (byte)(1u << uIndexPrec);
            byte uNumIndices2 = (byte)(1u << uIndexPrec2);
            uint[] auPixIdx = new uint[BC.NUM_PIXELS_PER_BLOCK];
            Color[][] aPalette = new Color[BC67.BC7_MAX_REGIONS][];
            for (int i = 0; i < aPalette.Length; i++)
                aPalette[i] = new Color[BC67.BC7_MAX_INDICES];

            for (uint p = 0; p <= uPartitions; p++)
            {
                uint np = 0;
                for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; i++)
                {
                    if (BC67.g_aPartitionTable[uPartitions][uShape][i] == p)
                        auPixIdx[np++] = i;
                }

                // handle simple cases
                Debug.Assert(np > 0);
                if (np == 1)
                {
                    aEndPts[p].A = pEP.aLDRPixels[auPixIdx[0]];
                    aEndPts[p].B = pEP.aLDRPixels[auPixIdx[0]];
                    continue;
                }
                else if (np == 2)
                {
                    aEndPts[p].A = pEP.aLDRPixels[auPixIdx[0]];
                    aEndPts[p].B = pEP.aLDRPixels[auPixIdx[1]];
                    continue;
                }

                if (uIndexPrec2 == 0)
                {
                    Color4 epA, epB;
                    BC67.OptimizeRGBA(pEP.aHDRPixels, out epA, out epB, 4, np, auPixIdx);
                    epA.Clamp(0.0f, 1.0f);
                    epB.Clamp(0.0f, 1.0f);
                    epA *= 255.0f;
                    epB *= 255.0f;
                    aEndPts[p].A = BC67.HDRToLDRExplicit(epA);
                    aEndPts[p].B = BC67.HDRToLDRExplicit(epB);
                }
                else
                {
                    byte uMinAlpha = 255, uMaxAlpha = 0;
                    for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
                    {
                        uMinAlpha = Math.Max(uMinAlpha, pEP.aLDRPixels[auPixIdx[i]].A);
                        uMaxAlpha = Math.Max(uMaxAlpha, pEP.aLDRPixels[auPixIdx[i]].A);
                    }

                    Color4 epA, epB;
                    BC67.OptimizeRGB(pEP.aHDRPixels, out epA, out epB, 4, np, auPixIdx, cxt);
                    epA.Clamp(0.0f, 1.0f);
                    epB.Clamp(0.0f, 1.0f);
                    epA *= 255.0f;
                    epB *= 255.0f;
                    aEndPts[p].A = BC67.HDRToLDRExplicit(epA);
                    aEndPts[p].B = BC67.HDRToLDRExplicit(epB);
                    aEndPts[p].A.A = uMinAlpha;
                    aEndPts[p].B.A = uMaxAlpha;
                }
            }

            if (uIndexPrec2 == 0)
            {
                for (uint p = 0; p <= uPartitions; p++)
                    for (uint i = 0; i < uNumIndices; i++)
                        BC67.Interpolate(aEndPts[p].A, aEndPts[p].B, i, i, uIndexPrec, uIndexPrec, ref aPalette[p][i]);
            }
            else
            {
                for (uint p = 0; p <= uPartitions; p++)
                {
                    for (uint i = 0; i < uNumIndices; i++)
                        BC67.InterpolateRGB(aEndPts[p].A, aEndPts[p].B, i, uIndexPrec, ref aPalette[p][i]);
                    for (uint i = 0; i < uNumIndices2; i++)
                        BC67.InterpolateA(aEndPts[p].A, aEndPts[p].B, i, uIndexPrec2, ref aPalette[p][i]);
                }
            }

            float fTotalErr = 0;
            for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; i++)
            {
                byte uRegion = BC67.g_aPartitionTable[uPartitions][uShape][i];
                uint dummy;
                fTotalErr += BC67.ComputeError(pEP.aLDRPixels[i], aPalette[uRegion], uIndexPrec, uIndexPrec2, out dummy, out dummy);
            }

            return fTotalErr;
        }
    }
}
