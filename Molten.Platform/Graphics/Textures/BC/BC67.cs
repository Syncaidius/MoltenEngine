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
            internal LDRColorA A;
            internal LDRColorA B;
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

        internal static void TransformInverse(INTEndPntPair[] aEndPts, LDRColorA Prec, bool bSigned)
        {
            INTColor WrapMask = new INTColor((1 << Prec.r) - 1, (1 << Prec.g) - 1, (1 << Prec.b) - 1);
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
        internal static float OptimizeRGB(HDRColorA[] pPoints, out HDRColorA pX, out HDRColorA pY, uint cSteps, uint cPixels, uint[] pIndex, BCContext cxt)
        {
            pX = new HDRColorA();
            pY = new HDRColorA();
            float fError = float.MaxValue;
            float[] pC = (3 == cSteps) ? pC3 : pC4;
            float[] pD = (3 == cSteps) ? pD3 : pD4;

            // Find Min and Max points, as starting point
            HDRColorA X = new HDRColorA(1.0f, 1.0f, 1.0f, 0.0f);
            HDRColorA Y = new HDRColorA(0.0f, 0.0f, 0.0f, 0.0f);

            for (uint iPoint = 0; iPoint < cPixels; iPoint++)
            {
                if (pPoints[pIndex[iPoint]].r < X.r) X.r = pPoints[pIndex[iPoint]].r;
                if (pPoints[pIndex[iPoint]].g < X.g) X.g = pPoints[pIndex[iPoint]].g;
                if (pPoints[pIndex[iPoint]].b < X.b) X.b = pPoints[pIndex[iPoint]].b;
                if (pPoints[pIndex[iPoint]].r > Y.r) Y.r = pPoints[pIndex[iPoint]].r;
                if (pPoints[pIndex[iPoint]].g > Y.g) Y.g = pPoints[pIndex[iPoint]].g;
                if (pPoints[pIndex[iPoint]].b > Y.b) Y.b = pPoints[pIndex[iPoint]].b;
            }

            // Diagonal axis
            HDRColorA AB = new HDRColorA()
            {
                r = Y.r - X.r,
                g = Y.g - X.g,
                b = Y.b - X.b,
            };

            float fAB = AB.r * AB.r + AB.g * AB.g + AB.b * AB.b;

            // Single color block.. no need to root-find
            if (fAB < float.MinValue)
            {
                pX.r = X.r; pX.g = X.g; pX.b = X.b;
                pY.r = Y.r; pY.g = Y.g; pY.b = Y.b;
                return 0.0f;
            }

            // Try all four axis directions, to determine which diagonal best fits data
            float fABInv = 1.0f / fAB;

            HDRColorA Dir;
            Dir.r = AB.r * fABInv;
            Dir.g = AB.g * fABInv;
            Dir.b = AB.b * fABInv;

            HDRColorA Mid;
            Mid.r = (X.r + Y.r) * 0.5f;
            Mid.g = (X.g + Y.g) * 0.5f;
            Mid.b = (X.b + Y.b) * 0.5f;


            //fDir[0] = fDir[1] = fDir[2] = fDir[3] = 0.0f;

            for (uint iPoint = 0; iPoint < cPixels; iPoint++)
            {
                HDRColorA Pt;
                Pt.r = (pPoints[pIndex[iPoint]].r - Mid.r) * Dir.r;
                Pt.g = (pPoints[pIndex[iPoint]].g - Mid.g) * Dir.g;
                Pt.b = (pPoints[pIndex[iPoint]].b - Mid.b) * Dir.b;

                float f;
                f = Pt.r + Pt.g + Pt.b; cxt.fDir[0] += f * f;
                f = Pt.r + Pt.g - Pt.b; cxt.fDir[1] += f * f;
                f = Pt.r - Pt.g + Pt.b; cxt.fDir[2] += f * f;
                f = Pt.r - Pt.g - Pt.b; cxt.fDir[3] += f * f;
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

            if ((iDirMax & 2) == 2) // std::swap(X.g, Y.g);
            {
                float temp = X.g;
                X.g = Y.g;
                Y.g = temp;
            }
            if ((iDirMax & 1) == 1) // std::swap(X.b, Y.b);
            {
                float temp = X.b;
                X.b = Y.b;
                Y.b = temp;
            }

            // Two color block.. no need to root-find
            if (fAB < 1.0f / 4096.0f)
            {
                pX.r = X.r; pX.g = X.g; pX.b = X.b;
                pY.r = Y.r; pY.g = Y.g; pY.b = Y.b;
                return 0.0f;
            }

            // Use Newton's Method to find local minima of sum-of-squares error.
            float fSteps = cSteps - 1;

            for (uint iIteration = 0; iIteration < 8; iIteration++)
            {
                for (uint iStep = 0; iStep < cSteps; iStep++)
                {
                    cxt.pSteps[iStep].r = X.r * pC[iStep] + Y.r * pD[iStep];
                    cxt.pSteps[iStep].g = X.g * pC[iStep] + Y.g * pD[iStep];
                    cxt.pSteps[iStep].b = X.b * pC[iStep] + Y.b * pD[iStep];
                }

                // Calculate color direction
                Dir.r = Y.r - X.r;
                Dir.g = Y.g - X.g;
                Dir.b = Y.b - X.b;

                float fLen = (Dir.r * Dir.r + Dir.g * Dir.g + Dir.b * Dir.b);

                if (fLen < (1.0f / 4096.0f))
                    break;

                float fScale = fSteps / fLen;

                Dir.r *= fScale;
                Dir.g *= fScale;
                Dir.b *= fScale;

                // Evaluate function, and derivatives
                float d2X = 0.0f, d2Y = 0.0f;
                HDRColorA dX = new HDRColorA(0.0f, 0.0f, 0.0f, 0.0f);
                HDRColorA dY = new HDRColorA(0.0f, 0.0f, 0.0f, 0.0f);

                for (uint iPoint = 0; iPoint < cPixels; iPoint++)
                {
                    float fDot = (pPoints[pIndex[iPoint]].r - X.r) * Dir.r +
                        (pPoints[pIndex[iPoint]].g - X.g) * Dir.g +
                        (pPoints[pIndex[iPoint]].b - X.b) * Dir.b;

                    uint iStep;
                    if (fDot <= 0.0f)
                        iStep = 0;
                    else if (fDot >= fSteps)
                        iStep = cSteps - 1;
                    else
                        iStep = (uint)(fDot + 0.5f);

                    HDRColorA Diff;
                    Diff.r = cxt.pSteps[iStep].r - pPoints[pIndex[iPoint]].r;
                    Diff.g = cxt.pSteps[iStep].g - pPoints[pIndex[iPoint]].g;
                    Diff.b = cxt.pSteps[iStep].b - pPoints[pIndex[iPoint]].b;

                    float fC = pC[iStep] * (1.0f / 8.0f);
                    float fD = pD[iStep] * (1.0f / 8.0f);

                    d2X += fC * pC[iStep];
                    dX.r += fC * Diff.r;
                    dX.g += fC * Diff.g;
                    dX.b += fC * Diff.b;

                    d2Y += fD * pD[iStep];
                    dY.r += fD * Diff.r;
                    dY.g += fD * Diff.g;
                    dY.b += fD * Diff.b;
                }

                // Move endpoints
                if (d2X > 0.0f)
                {
                    float f = -1.0f / d2X;

                    X.r += dX.r * f;
                    X.g += dX.g * f;
                    X.b += dX.b * f;
                }

                if (d2Y > 0.0f)
                {
                    float f = -1.0f / d2Y;

                    Y.r += dY.r * f;
                    Y.g += dY.g * f;
                    Y.b += dY.b * f;
                }

                if ((dX.r * dX.r < fEpsilon) && (dX.g * dX.g < fEpsilon) && (dX.b * dX.b < fEpsilon) &&
                    (dY.r * dY.r < fEpsilon) && (dY.g * dY.g < fEpsilon) && (dY.b * dY.b < fEpsilon))
                {
                    break;
                }
            }


            pX.r = X.r; pX.g = X.g; pX.b = X.b;
            pY.r = Y.r; pY.g = Y.g; pY.b = Y.b;
            return fError;
        }


        //-------------------------------------------------------------------------------------
        internal static float OptimizeRGBA(HDRColorA[] pPoints, out HDRColorA pX, out HDRColorA pY, uint cSteps, uint cPixels, uint[] pIndex)
        {
            float fError = float.MaxValue;
            float[] pC = (3 == cSteps) ? pC3 : pC4;
            float[] pD = (3 == cSteps) ? pD3 : pD4;

            // Find Min and Max points, as starting point
            HDRColorA X = new HDRColorA(1.0f, 1.0f, 1.0f, 1.0f);
            HDRColorA Y = new HDRColorA(0.0f, 0.0f, 0.0f, 0.0f);

            for (uint iPoint = 0; iPoint < cPixels; iPoint++)
            {
                if (pPoints[pIndex[iPoint]].r < X.r) X.r = pPoints[pIndex[iPoint]].r;
                if (pPoints[pIndex[iPoint]].g < X.g) X.g = pPoints[pIndex[iPoint]].g;
                if (pPoints[pIndex[iPoint]].b < X.b) X.b = pPoints[pIndex[iPoint]].b;
                if (pPoints[pIndex[iPoint]].a < X.a) X.a = pPoints[pIndex[iPoint]].a;
                if (pPoints[pIndex[iPoint]].r > Y.r) Y.r = pPoints[pIndex[iPoint]].r;
                if (pPoints[pIndex[iPoint]].g > Y.g) Y.g = pPoints[pIndex[iPoint]].g;
                if (pPoints[pIndex[iPoint]].b > Y.b) Y.b = pPoints[pIndex[iPoint]].b;
                if (pPoints[pIndex[iPoint]].a > Y.a) Y.a = pPoints[pIndex[iPoint]].a;
            }

            // Diagonal axis
            HDRColorA AB = Y - X;
            float fAB = AB * AB;

            // Single color block.. no need to root-find
            if (fAB < float.MinValue)
            {
                pX = X;
                pY = Y;
                return 0.0f;
            }

            // Try all four axis directions, to determine which diagonal best fits data
            float fABInv = 1.0f / fAB;
            HDRColorA Dir = AB * fABInv;
            HDRColorA Mid = (X + Y) * 0.5f;

            float[] fDir = new float[8]; // fDir[0] = fDir[1] = fDir[2] = fDir[3] = fDir[4] = fDir[5] = fDir[6] = fDir[7] = 0.0f;

            for (uint iPoint = 0; iPoint < cPixels; iPoint++)
            {
                HDRColorA Pt;
                Pt.r = (pPoints[pIndex[iPoint]].r - Mid.r) * Dir.r;
                Pt.g = (pPoints[pIndex[iPoint]].g - Mid.g) * Dir.g;
                Pt.b = (pPoints[pIndex[iPoint]].b - Mid.b) * Dir.b;
                Pt.a = (pPoints[pIndex[iPoint]].a - Mid.a) * Dir.a;

                float f;
                f = Pt.r + Pt.g + Pt.b + Pt.a; fDir[0] += f * f;
                f = Pt.r + Pt.g + Pt.b - Pt.a; fDir[1] += f * f;
                f = Pt.r + Pt.g - Pt.b + Pt.a; fDir[2] += f * f;
                f = Pt.r + Pt.g - Pt.b - Pt.a; fDir[3] += f * f;
                f = Pt.r - Pt.g + Pt.b + Pt.a; fDir[4] += f * f;
                f = Pt.r - Pt.g + Pt.b - Pt.a; fDir[5] += f * f;
                f = Pt.r - Pt.g - Pt.b + Pt.a; fDir[6] += f * f;
                f = Pt.r - Pt.g - Pt.b - Pt.a; fDir[7] += f * f;
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
                float temp = X.g;
                X.g = Y.g;
                Y.g = temp;
            }
            if ((iDirMax & 2) == 2) // std::swap(X.b, Y.b);
            {
                float temp = X.b;
                X.b = Y.b;
                Y.b = temp;
            }
            if ((iDirMax & 1) == 1) // std::swap(X.a, Y.a);
            {
                float temp = X.a;
                X.b = Y.a;
                Y.a = temp;
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
                HDRColorA[] pSteps = new HDRColorA[BC7_MAX_INDICES];

                LDRColorA lX, lY;
                lX = (X * 255.0f).ToLDRColorA();
                lY = (Y * 255.0f).ToLDRColorA();

                for (uint iStep = 0; iStep < cSteps; iStep++)
                {
                    pSteps[iStep] = X * pC[iStep] + Y * pD[iStep];
                    //LDRColorA::Interpolate(lX, lY, i, i, wcprec, waprec, aSteps[i]);
                }

                // Calculate color direction
                Dir = Y - X;
                float fLen = Dir * Dir;
                if (fLen < (1.0f / 4096.0f))
                    break;

                float fScale = fSteps / fLen;
                Dir *= fScale;

                // Evaluate function, and derivatives
                float d2X = 0.0f, d2Y = 0.0f;
                HDRColorA dX = new HDRColorA(0.0f, 0.0f, 0.0f, 0.0f);
                HDRColorA dY = new HDRColorA(0.0f, 0.0f, 0.0f, 0.0f);

                for (uint iPoint = 0; iPoint < cPixels; ++iPoint)
                {
                    float fDot = (pPoints[pIndex[iPoint]] - X) * Dir;

                    uint iStep;
                    if (fDot <= 0.0f)
                        iStep = 0;
                    else if (fDot >= fSteps)
                        iStep = cSteps - 1;
                    else
                        iStep = (uint)(fDot + 0.5f);

                    HDRColorA Diff = pSteps[iStep] - pPoints[pIndex[iPoint]];
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

                if ((dX * dX < fEpsilon) && (dY * dY < fEpsilon))
                    break;
            }

            pX = X;
            pY = Y;
            return fError;
        }


        //-------------------------------------------------------------------------------------
        internal static float ComputeError(LDRColorA pixel, LDRColorA[] aPalette,
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

            Vector4F vpixel = new Vector4F(pixel.r, pixel.g, pixel.b, pixel.a);

            if (uIndexPrec2 == 0)
            {
                for (uint i = 0; i < uNumIndices && fBestErr > 0; i++)
                {
                    LDRColorA col = aPalette[i];
                    Vector4F tpixel = new Vector4F(col.r, col.g, col.b, col.a);

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
                    LDRColorA col = aPalette[i];
                    Vector4F tpixel = new Vector4F(col.r, col.g, col.b, col.a);

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
                    float ea = (float)pixel.a - aPalette[i].a;
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


        internal static void FillWithErrorColors(HDRColorA[] pOut)
        {
            for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
            {
# if DEBUG
                // Use Magenta in debug as a highly-visible error color
                pOut[i] = new HDRColorA(1.0f, 0.0f, 1.0f, 1.0f);
#else
                // In production use, default to black
                pOut[i] = new HDRColorA(0.0f, 0.0f, 0.0f, 1.0f);
#endif
            }
        }
    }
}
