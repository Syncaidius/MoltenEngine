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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Textures
{
    internal static partial class BC67
    {
        //-------------------------------------------------------------------------------------
        // Constants
        //-------------------------------------------------------------------------------------

        internal const ushort F16S_MASK = 0x8000;   // f16 sign mask
        internal const ushort F16EM_MASK = 0x7fff;   // f16 exp & mantissa mask
        internal const ushort F16MAX = 0x7bff;   // MAXFLT bit pattern for XMHALF

        internal const uint BC6H_NUM_CHANNELS = 3;
        internal const uint BC6H_MAX_SHAPES = 32;

        internal const uint BC7_NUM_CHANNELS = 4;
        internal const uint BC7_MAX_SHAPES = 64;

        internal const int BC67_WEIGHT_MAX = 64;
        internal const int BC67_WEIGHT_SHIFT = 6;
        internal const int BC67_WEIGHT_ROUND = 32;

        internal const int BC6H_MAX_REGIONS = 2;
        internal const int BC6H_MAX_INDICES = 16;
        internal const int BC7_MAX_REGIONS = 3;
        internal const int BC7_MAX_INDICES = 16;

        internal const float fEpsilon = (0.25f / 64.0f) * (0.25f / 64.0f);
        internal static readonly float[] pC3 = { 2.0f / 2.0f, 1.0f / 2.0f, 0.0f / 2.0f };
        internal static readonly float[] pD3 = { 0.0f / 2.0f, 1.0f / 2.0f, 2.0f / 2.0f };
        internal static readonly float[] pC4 = { 3.0f / 3.0f, 2.0f / 3.0f, 1.0f / 3.0f, 0.0f / 3.0f };
        internal static readonly float[] pD4 = { 0.0f / 3.0f, 1.0f / 3.0f, 2.0f / 3.0f, 3.0f / 3.0f };

        // Partition, Shape, Fixup
        //[3][64] [3]


        internal static readonly int[] g_aWeights2 = { 0, 21, 43, 64 };
        internal static readonly int[] g_aWeights3 = { 0, 9, 18, 27, 37, 46, 55, 64 };
        internal static readonly int[] g_aWeights4 = { 0, 4, 9, 13, 17, 21, 26, 30, 34, 38, 43, 47, 51, 55, 60, 64 };

        internal struct LDREndPntPair
        {
            internal Color A;
            internal Color B;
        };

        internal struct INTEndPntPair
        {
            internal INTColor A;
            internal INTColor B;

            /// <summary>
            /// Swaps points A and B.
            /// </summary>
            internal void SwapPoints()
            {
                INTColor C = A;
                A = B;
                B = C;
            }
        };

        internal static void Swap<T>(ref T a, ref T b) where T: struct
        {
            T temp = a;
            a = b;
            b = temp;
        }

        internal static int SIGN_EXTEND(int x, int nb)
        {
            int val = (1 << ((nb) - 1));
            return ((x & val) == val ? ((~0) << (nb)) : 0) | (x);
        }

        //-------------------------------------------------------------------------------------
        // Helper functions
        //-------------------------------------------------------------------------------------
        internal static bool IsFixUpOffset(uint uPartitions, uint uShape, uint uOffset)
        {
            Debug.Assert(uPartitions < 3 && uShape < 64 && uOffset < 16);
            for (uint p = 0; p <= uPartitions; p++)
            {
                if (uOffset == g_aFixUp[uPartitions][uShape][p])
                {
                    return true;
                }
            }
            return false;
        }

        internal static void TransformForward(INTEndPntPair[] aEndPts)
        {
            aEndPts[0].B -= aEndPts[0].A;
            aEndPts[1].A -= aEndPts[0].A;
            aEndPts[1].B -= aEndPts[0].A;
        }

        internal static void TransformInverse(INTEndPntPair[] aEndPts, Color Prec, bool bSigned)
        {
            INTColor WrapMask = new INTColor((1 << Prec.R) - 1, (1 << Prec.G) - 1, (1 << Prec.B) - 1);
            aEndPts[0].B += aEndPts[0].A; aEndPts[0].B &= WrapMask;
            aEndPts[1].A += aEndPts[0].A; aEndPts[1].A &= WrapMask;
            aEndPts[1].B += aEndPts[0].A; aEndPts[1].B &= WrapMask;
            if (bSigned)
            {
                aEndPts[0].B.SignExtend(Prec);
                aEndPts[1].A.SignExtend(Prec);
                aEndPts[1].B.SignExtend(Prec);
            }
        }

        internal static float Norm(INTColor a, INTColor b)
        {
            float dr = a.r - b.r;
            float dg = a.g - b.g;
            float db = a.b - b.b;
            return dr * dr + dg * dg + db * db;
        }

        // return # of bits needed to store n. handle signed or unsigned cases properly
        internal static int NBits(int n, bool bIsSigned)
        {
            int nb;
            if (n == 0)
            {
                return 0;   // no bits needed for 0, signed or not
            }
            else if (n > 0)
            {
                for (nb = 0; n > 0; ++nb, n >>= 1) ;
                return nb + (bIsSigned ? 1 : 0);
            }
            else
            {
                Debug.Assert(bIsSigned);
                for (nb = 0; n < -1; ++nb, n >>= 1) ;
                return nb + 1;
            }
        }


        //-------------------------------------------------------------------------------------
        internal static float OptimizeRGB(Color4[] pPoints, out Color4 pX, out Color4 pY, uint cSteps, uint cPixels, uint[] pIndex, BCContext cxt)
        {
            pX = new Color4();
            pY = new Color4();
            float fError = float.MaxValue;
            float[] pC = (3 == cSteps) ? pC3 : pC4;
            float[] pD = (3 == cSteps) ? pD3 : pD4;

            // Find Min and Max points, as starting point
            Color4 X = new Color4(1.0f, 1.0f, 1.0f, 0.0f);
            Color4 Y = new Color4(0.0f, 0.0f, 0.0f, 0.0f);

            for (uint iPoint = 0; iPoint < cPixels; iPoint++)
            {
                if (pPoints[pIndex[iPoint]].R < X.R) X.R = pPoints[pIndex[iPoint]].R;
                if (pPoints[pIndex[iPoint]].G < X.G) X.G = pPoints[pIndex[iPoint]].G;
                if (pPoints[pIndex[iPoint]].B < X.B) X.B = pPoints[pIndex[iPoint]].B;
                if (pPoints[pIndex[iPoint]].R > Y.R) Y.R = pPoints[pIndex[iPoint]].R;
                if (pPoints[pIndex[iPoint]].G > Y.G) Y.G = pPoints[pIndex[iPoint]].G;
                if (pPoints[pIndex[iPoint]].B > Y.B) Y.B = pPoints[pIndex[iPoint]].B;
            }

            // Diagonal axis
            Color4 AB = new Color4()
            {
                R = Y.R - X.R,
                G = Y.G - X.G,
                B = Y.B - X.B,
            };

            float fAB = AB.R * AB.R + AB.G * AB.G + AB.B * AB.B;

            // Single color block.. no need to root-find
            if (fAB < float.MinValue)
            {
                pX.R = X.R; pX.G = X.G; pX.B = X.B;
                pY.R = Y.R; pY.G = Y.G; pY.B = Y.B;
                return 0.0f;
            }

            // Try all four axis directions, to determine which diagonal best fits data
            float fABInv = 1.0f / fAB;

            Color4 Dir;
            Dir.R = AB.R * fABInv;
            Dir.G = AB.G * fABInv;
            Dir.B = AB.B * fABInv;

            Color4 Mid;
            Mid.R = (X.R + Y.R) * 0.5f;
            Mid.G = (X.G + Y.G) * 0.5f;
            Mid.B = (X.B + Y.B) * 0.5f;


            //fDir[0] = fDir[1] = fDir[2] = fDir[3] = 0.0f;

            for (uint iPoint = 0; iPoint < cPixels; iPoint++)
            {
                Color4 Pt;
                Pt.R = (pPoints[pIndex[iPoint]].R - Mid.R) * Dir.R;
                Pt.G = (pPoints[pIndex[iPoint]].G - Mid.G) * Dir.G;
                Pt.B = (pPoints[pIndex[iPoint]].B - Mid.B) * Dir.B;

                float f;
                f = Pt.R + Pt.G + Pt.B; cxt.fDir[0] += f * f;
                f = Pt.R + Pt.G - Pt.B; cxt.fDir[1] += f * f;
                f = Pt.R - Pt.G + Pt.B; cxt.fDir[2] += f * f;
                f = Pt.R - Pt.G - Pt.B; cxt.fDir[3] += f * f;
            }

            float fDirMax = cxt.fDir[0];
            uint iDirMax = 0;

            for (uint iDir = 1; iDir < 4; iDir++)
            {
                if (cxt.fDir[iDir] > fDirMax)
                {
                    fDirMax = cxt.fDir[iDir];
                    iDirMax = iDir;
                }
            }

            if ((iDirMax & 2) == 2) // std::swap(X.G, Y.G);
            {
                float temp = X.G;
                X.G = Y.G;
                Y.G = temp;
            }
            if ((iDirMax & 1) == 1) // std::swap(X.B, Y.B);
            {
                float temp = X.B;
                X.B = Y.B;
                Y.B = temp;
            }

            // Two color block.. no need to root-find
            if (fAB < 1.0f / 4096.0f)
            {
                pX.R = X.R; pX.G = X.G; pX.B = X.B;
                pY.R = Y.R; pY.G = Y.G; pY.B = Y.B;
                return 0.0f;
            }

            // Use Newton's Method to find local minima of sum-of-squares error.
            float fSteps = cSteps - 1;

            for (uint iIteration = 0; iIteration < 8; iIteration++)
            {
                for (uint iStep = 0; iStep < cSteps; iStep++)
                {
                    cxt.pSteps[iStep].R = X.R * pC[iStep] + Y.R * pD[iStep];
                    cxt.pSteps[iStep].G = X.G * pC[iStep] + Y.G * pD[iStep];
                    cxt.pSteps[iStep].B = X.B * pC[iStep] + Y.B * pD[iStep];
                }

                // Calculate color direction
                Dir.R = Y.R - X.R;
                Dir.G = Y.G - X.G;
                Dir.B = Y.B - X.B;

                float fLen = (Dir.R * Dir.R + Dir.G * Dir.G + Dir.B * Dir.B);

                if (fLen < (1.0f / 4096.0f))
                    break;

                float fScale = fSteps / fLen;

                Dir.R *= fScale;
                Dir.G *= fScale;
                Dir.B *= fScale;

                // Evaluate function, and derivatives
                float d2X = 0.0f, d2Y = 0.0f;
                Color4 dX = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
                Color4 dY = new Color4(0.0f, 0.0f, 0.0f, 0.0f);

                for (uint iPoint = 0; iPoint < cPixels; iPoint++)
                {
                    float fDot = (pPoints[pIndex[iPoint]].R - X.R) * Dir.R +
                        (pPoints[pIndex[iPoint]].G - X.G) * Dir.G +
                        (pPoints[pIndex[iPoint]].B - X.B) * Dir.B;

                    uint iStep;
                    if (fDot <= 0.0f)
                        iStep = 0;
                    else if (fDot >= fSteps)
                        iStep = cSteps - 1;
                    else
                        iStep = (uint)(fDot + 0.5f);

                    Color4 Diff;
                    Diff.R = cxt.pSteps[iStep].R - pPoints[pIndex[iPoint]].R;
                    Diff.G = cxt.pSteps[iStep].G - pPoints[pIndex[iPoint]].G;
                    Diff.B = cxt.pSteps[iStep].B - pPoints[pIndex[iPoint]].B;

                    float fC = pC[iStep] * (1.0f / 8.0f);
                    float fD = pD[iStep] * (1.0f / 8.0f);

                    d2X += fC * pC[iStep];
                    dX.R += fC * Diff.R;
                    dX.G += fC * Diff.G;
                    dX.B += fC * Diff.B;

                    d2Y += fD * pD[iStep];
                    dY.R += fD * Diff.R;
                    dY.G += fD * Diff.G;
                    dY.B += fD * Diff.B;
                }

                // Move endpoints
                if (d2X > 0.0f)
                {
                    float f = -1.0f / d2X;

                    X.R += dX.R * f;
                    X.G += dX.G * f;
                    X.B += dX.B * f;
                }

                if (d2Y > 0.0f)
                {
                    float f = -1.0f / d2Y;

                    Y.R += dY.R * f;
                    Y.G += dY.G * f;
                    Y.B += dY.B * f;
                }

                if ((dX.R * dX.R < fEpsilon) && (dX.G * dX.G < fEpsilon) && (dX.B * dX.B < fEpsilon) &&
                    (dY.R * dY.R < fEpsilon) && (dY.G * dY.G < fEpsilon) && (dY.B * dY.B < fEpsilon))
                {
                    break;
                }
            }


            pX.R = X.R; pX.G = X.G; pX.B = X.B;
            pY.R = Y.R; pY.G = Y.G; pY.B = Y.B;
            return fError;
        }


        //-------------------------------------------------------------------------------------
        internal static float OptimizeRGBA(Color4[] pPoints, out Color4 pX, out Color4 pY, uint cSteps, uint cPixels, uint[] pIndex)
        {
            float fError = float.MaxValue;
            float[] pC = (3 == cSteps) ? pC3 : pC4;
            float[] pD = (3 == cSteps) ? pD3 : pD4;

            // Find Min and Max points, as starting point
            Color4 X = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            Color4 Y = new Color4(0.0f, 0.0f, 0.0f, 0.0f);

            for (uint iPoint = 0; iPoint < cPixels; iPoint++)
            {
                if (pPoints[pIndex[iPoint]].R < X.R) X.R = pPoints[pIndex[iPoint]].R;
                if (pPoints[pIndex[iPoint]].G < X.G) X.G = pPoints[pIndex[iPoint]].G;
                if (pPoints[pIndex[iPoint]].B < X.B) X.B = pPoints[pIndex[iPoint]].B;
                if (pPoints[pIndex[iPoint]].A < X.A) X.A = pPoints[pIndex[iPoint]].A;
                if (pPoints[pIndex[iPoint]].R > Y.R) Y.R = pPoints[pIndex[iPoint]].R;
                if (pPoints[pIndex[iPoint]].G > Y.G) Y.G = pPoints[pIndex[iPoint]].G;
                if (pPoints[pIndex[iPoint]].B > Y.B) Y.B = pPoints[pIndex[iPoint]].B;
                if (pPoints[pIndex[iPoint]].A > Y.A) Y.A = pPoints[pIndex[iPoint]].A;
            }

            // Diagonal axis
            Color4 AB = Y - X;
            float fAB = Color4.Dot(AB, AB);

            // Single color block.. no need to root-find
            if (fAB < float.MinValue)
            {
                pX = X;
                pY = Y;
                return 0.0f;
            }

            // Try all four axis directions, to determine which diagonal best fits data
            float fABInv = 1.0f / fAB;
            Color4 Dir = AB * fABInv;
            Color4 Mid = (X + Y) * 0.5f;

            float[] fDir = new float[8]; // fDir[0] = fDir[1] = fDir[2] = fDir[3] = fDir[4] = fDir[5] = fDir[6] = fDir[7] = 0.0f;

            for (uint iPoint = 0; iPoint < cPixels; iPoint++)
            {
                Color4 Pt;
                Pt.R = (pPoints[pIndex[iPoint]].R - Mid.R) * Dir.R;
                Pt.G = (pPoints[pIndex[iPoint]].G - Mid.G) * Dir.G;
                Pt.B = (pPoints[pIndex[iPoint]].B - Mid.B) * Dir.B;
                Pt.A = (pPoints[pIndex[iPoint]].A - Mid.A) * Dir.A;

                float f;
                f = Pt.R + Pt.G + Pt.B + Pt.A; fDir[0] += f * f;
                f = Pt.R + Pt.G + Pt.B - Pt.A; fDir[1] += f * f;
                f = Pt.R + Pt.G - Pt.B + Pt.A; fDir[2] += f * f;
                f = Pt.R + Pt.G - Pt.B - Pt.A; fDir[3] += f * f;
                f = Pt.R - Pt.G + Pt.B + Pt.A; fDir[4] += f * f;
                f = Pt.R - Pt.G + Pt.B - Pt.A; fDir[5] += f * f;
                f = Pt.R - Pt.G - Pt.B + Pt.A; fDir[6] += f * f;
                f = Pt.R - Pt.G - Pt.B - Pt.A; fDir[7] += f * f;
            }

            float fDirMax = fDir[0];
            uint iDirMax = 0;

            for (uint iDir = 1; iDir < 8; iDir++)
            {
                if (fDir[iDir] > fDirMax)
                {
                    fDirMax = fDir[iDir];
                    iDirMax = iDir;
                }
            }

            if ((iDirMax & 4) == 4) // std::swap(X.g, Y.g);
            {
                float temp = X.G;
                X.G = Y.G;
                Y.G = temp;
            }
            if ((iDirMax & 2) == 2) // std::swap(X.b, Y.b);
            {
                float temp = X.B;
                X.B = Y.B;
                Y.B = temp;
            }
            if ((iDirMax & 1) == 1) // std::swap(X.a, Y.a);
            {
                float temp = X.A;
                X.B = Y.A;
                Y.A = temp;
            }

            // Two color block.. no need to root-find
            if (fAB < 1.0f / 4096.0f)
            {
                pX = X;
                pY = Y;
                return 0.0f;
            }

            // Use Newton's Method to find local minima of sum-of-squares error.
            float fSteps = cSteps - 1;

            for (uint iIteration = 0; iIteration < 8 && fError > 0.0f; iIteration++)
            {
                // Calculate new steps
                Color4[] pSteps = new Color4[BC7_MAX_INDICES];

                Color lX, lY;
                lX = HDRToLDRExplicit((X * 255.0f)); // .ToLDRColorA().
                lY = HDRToLDRExplicit((Y * 255.0f)); // .ToLDRColorA();

                for (uint iStep = 0; iStep < cSteps; iStep++)
                {
                    pSteps[iStep] = X * pC[iStep] + Y * pD[iStep];
                    //Color::Interpolate(lX, lY, i, i, wcprec, waprec, aSteps[i]);
                }

                // Calculate color direction
                Dir = Y - X;
                float fLen = Color4.Dot(Dir, Dir);
                if (fLen < (1.0f / 4096.0f))
                    break;

                float fScale = fSteps / fLen;
                Dir *= fScale;

                // Evaluate function, and derivatives
                float d2X = 0.0f, d2Y = 0.0f;
                Color4 dX = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
                Color4 dY = new Color4(0.0f, 0.0f, 0.0f, 0.0f);

                for (uint iPoint = 0; iPoint < cPixels; ++iPoint)
                {
                    float fDot = Color4.Dot((pPoints[pIndex[iPoint]] - X), Dir);

                    uint iStep;
                    if (fDot <= 0.0f)
                        iStep = 0;
                    else if (fDot >= fSteps)
                        iStep = cSteps - 1;
                    else
                        iStep = (uint)(fDot + 0.5f);

                    Color4 Diff = pSteps[iStep] - pPoints[pIndex[iPoint]];
                    float fC = pC[iStep] * (1.0f / 8.0f);
                    float fD = pD[iStep] * (1.0f / 8.0f);

                    d2X += fC * pC[iStep];
                    dX += Diff * fC;

                    d2Y += fD * pD[iStep];
                    dY += Diff * fD;
                }

                // Move endpoints
                if (d2X > 0.0f)
                {
                    float f = -1.0f / d2X;
                    X += dX * f;
                }

                if (d2Y > 0.0f)
                {
                    float f = -1.0f / d2Y;
                    Y += dY * f;
                }

                if ((Color4.Dot(dX, dX) < fEpsilon) && (Color4.Dot(dY, dY) < fEpsilon))
                    break;
            }

            pX = X;
            pY = Y;
            return fError;
        }

        internal static void InterpolateRGB(Color c0, Color c1, uint wc, uint wcprec, ref Color output)
        {
            int[] aWeights = null;
            switch (wcprec)
            {
                case 2: aWeights = g_aWeights2; Debug.Assert(wc < 4); break;
                case 3: aWeights = g_aWeights3; Debug.Assert(wc < 8); break;
                case 4: aWeights = g_aWeights4; Debug.Assert(wc < 16); break;
                default: Debug.Assert(false); output.R = output.G = output.B = 0; return;
            }
            output.R = (byte)(c0.R * (uint)(BC67_WEIGHT_MAX - aWeights[wc]) + c1.R * (uint)(aWeights[wc] + BC67_WEIGHT_ROUND) >> BC67_WEIGHT_SHIFT);
            output.G = (byte)(c0.G * (uint)(BC67_WEIGHT_MAX - aWeights[wc]) + c1.G * (uint)(aWeights[wc] + BC67_WEIGHT_ROUND) >> BC67_WEIGHT_SHIFT);
            output.B = (byte)(c0.B * (uint)(BC67_WEIGHT_MAX - aWeights[wc]) + c1.B * (uint)(aWeights[wc] + BC67_WEIGHT_ROUND) >> BC67_WEIGHT_SHIFT);
        }

        internal static void InterpolateA(Color c0, Color c1, uint wa, uint waprec, ref Color output)
        {
            int[] aWeights = null;
            switch (waprec)
            {
                case 2: aWeights = g_aWeights2; Debug.Assert(wa < 4); break;
                case 3: aWeights = g_aWeights3; Debug.Assert(wa < 8); break;
                case 4: aWeights = g_aWeights4; Debug.Assert(wa < 16); break;
                default: Debug.Assert(false); output.A = 0; return;
            }
            output.A = (byte)((c0.A * (uint)(BC67_WEIGHT_MAX - aWeights[wa]) + c1.A * (uint)(aWeights[wa]) + BC67_WEIGHT_ROUND) >> BC67_WEIGHT_SHIFT);
        }

        internal static void Interpolate(Color c0, Color c1, uint wc, uint wa, uint wcprec, uint waprec, ref Color output)
        {
            InterpolateRGB(c0, c1, wc, wcprec, ref output);
            InterpolateA(c0, c1, wa, waprec, ref output);
        }

        internal static Color4 LDRToHDRExplicit(Color c)
        {
            return new Color4()
            {
                R = c.R,
                G = c.G,
                B = c.B,
                A = c.A,
            };
        }

        internal static Color HDRToLDRExplicit(Color4 c)
        {
            return new Color((byte)(c.R + 0.01f), (byte)(c.G + 0.01f), (byte)(c.B + 0.01f), (byte)(c.A + 0.01f));
        }

        internal static Color HDRUnitToLDR(Color4 c)
        {
            Color4 tmp = Color4.Clamp(c, 0.0f, 1.0f) * 255.0f;
            return new Color()
            {
                R = (byte)(tmp.R + 0.001f),
                G = (byte)(tmp.G + 0.001f),
                B = (byte)(tmp.B + 0.001f),
                A = (byte)(tmp.A + 0.001f),
            };
        }


        //-------------------------------------------------------------------------------------
        internal static float ComputeError(Color pixel, Color[] aPalette,
            byte uIndexPrec,
            byte uIndexPrec2,
            out uint pBestIndex,
            out uint pBestIndex2)
        {
            uint uNumIndices = 1U << uIndexPrec;
            uint uNumIndices2 = 1U << uIndexPrec2;
            float fTotalErr = 0;
            float fBestErr = float.MaxValue;

            pBestIndex = 0;
            pBestIndex2 = 0;

            Vector4F vpixel = new Vector4F(pixel.R, pixel.G, pixel.B, pixel.A);

            if (uIndexPrec2 == 0)
            {
                for (uint i = 0; i < uNumIndices && fBestErr > 0; i++)
                {
                    Color col = aPalette[i];
                    Vector4F tpixel = new Vector4F(col.R, col.G, col.B, col.A);

                    // Compute ErrorMetric
                    tpixel = vpixel - tpixel;
                    float fErr = Vector4F.Dot(tpixel, tpixel);

                    if (fErr > fBestErr)    // error increased, so we're done searching
                        break;
                    if (fErr < fBestErr)
                    {
                        fBestErr = fErr;
                        pBestIndex = i;
                    }
                }
                fTotalErr += fBestErr;
            }
            else
            {
                for (uint i = 0; i < uNumIndices && fBestErr > 0; i++)
                {
                    Color col = aPalette[i];
                    Vector4F tpixel = new Vector4F(col.R, col.G, col.B, col.A);

                    // Compute ErrorMetricRGB
                    tpixel = vpixel - tpixel;  //XMVectorSubtract(vpixel, tpixel);
                    float fErr = Vector3F.Dot((Vector3F)tpixel, (Vector3F)tpixel); // VectXMVectorGetX(XMVector3Dot(tpixel, tpixel));
                    if (fErr > fBestErr)    // error increased, so we're done searching
                        break;
                    if (fErr < fBestErr)
                    {
                        fBestErr = fErr;
                        pBestIndex = i;
                    }
                }
                fTotalErr += fBestErr;
                fBestErr = float.MaxValue;
                for (uint i = 0; i < uNumIndices2 && fBestErr > 0; i++)
                {
                    // Compute ErrorMetricAlpha
                    float ea = (float)pixel.A - aPalette[i].A;
                    float fErr = ea * ea;
                    if (fErr > fBestErr)    // error increased, so we're done searching
                        break;
                    if (fErr < fBestErr)
                    {
                        fBestErr = fErr;
                        pBestIndex2 = i;
                    }
                }
                fTotalErr += fBestErr;
            }

            return fTotalErr;
        }


        internal static void FillWithErrorColors(Color4[] pOut)
        {
            for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
            {
# if DEBUG
                // Use Magenta in debug as a highly-visible error color
                pOut[i] = new Color4(1.0f, 0.0f, 1.0f, 1.0f);
#else
                // In production use, default to black
                pOut[i] = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
#endif
            }
        }
    }
}
