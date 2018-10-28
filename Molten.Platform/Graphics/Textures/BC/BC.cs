// Converted to C# by James Yarwood.
// MIT License.

//-------------------------------------------------------------------------------------
// BC.cpp
//  
// Block-compression (BC) functionality for BC1, BC2, BC3 (orginal DXTn formats)
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
    internal static class BC
    {
        //-------------------------------------------------------------------------------------
        // Constants
        //-------------------------------------------------------------------------------------

        // Perceptual weightings for the importance of each channel.
        static readonly Color4 g_Luminance = new Color4(0.2125f / 0.7154f, 1.0f, 0.0721f / 0.7154f, 1.0f);
        static readonly Color4 g_LuminanceInv = new Color4(0.7154f / 0.2125f, 1.0f, 0.7154f / 0.0721f, 1.0f);
        static readonly float[] pC3 = { 2.0f / 2.0f, 1.0f / 2.0f, 0.0f / 2.0f };
        static readonly float[] pD3 = { 0.0f / 2.0f, 1.0f / 2.0f, 2.0f / 2.0f };
        static readonly float[] pC4 = { 3.0f / 3.0f, 2.0f / 3.0f, 1.0f / 3.0f, 0.0f / 3.0f };
        static readonly float[] pD4 = { 0.0f / 3.0f, 1.0f / 3.0f, 2.0f / 3.0f, 3.0f / 3.0f };
        static readonly float[] pC6 = { 5.0f / 5.0f, 4.0f / 5.0f, 3.0f / 5.0f, 2.0f / 5.0f, 1.0f / 5.0f, 0.0f / 5.0f };
        static readonly float[] pD6 = { 0.0f / 5.0f, 1.0f / 5.0f, 2.0f / 5.0f, 3.0f / 5.0f, 4.0f / 5.0f, 5.0f / 5.0f };
        static readonly float[] pC8 = { 7.0f / 7.0f, 6.0f / 7.0f, 5.0f / 7.0f, 4.0f / 7.0f, 3.0f / 7.0f, 2.0f / 7.0f, 1.0f / 7.0f, 0.0f / 7.0f };
        static readonly float[] pD8 = { 0.0f / 7.0f, 1.0f / 7.0f, 2.0f / 7.0f, 3.0f / 7.0f, 4.0f / 7.0f, 5.0f / 7.0f, 6.0f / 7.0f, 7.0f / 7.0f };
        const float fEpsilon = (0.25f / 64.0f) * (0.25f / 64.0f);
        static readonly uint[] pSteps3 = { 0, 2, 1 };
        static readonly uint[] pSteps4 = { 0, 2, 3, 1 };
        static readonly uint[] pSteps6 = { 0, 2, 3, 4, 5, 1 };
        static readonly uint[] pSteps8 = { 0, 2, 3, 4, 5, 6, 7, 1 };
        public const uint NUM_PIXELS_PER_BLOCK = 16;
        const float FLT_MIN = 1.175494351e-38F;        // min normalized positive value
        static readonly Color4 s_Scale = new Color4(1f / 31f, 1f / 63f, 1f / 31f, 1f);

        // Taken from DirectXMath.h.
        const uint XM_SELECT_0 = 0x00000000;
        const uint XM_SELECT_1 = 0xFFFFFFFF;
        static Vector4UI g_XMSelect1110 = new Vector4UI(XM_SELECT_1, XM_SELECT_1, XM_SELECT_1, XM_SELECT_0);
        static Color4 g_XMIdentityR3 = new Color4(0.0f, 0.0f, 0.0f, 1.0f);

        /// <summary>
        /// Decode/Encode RGB 5/6/5 colors
        /// </summary>
        /// <param name="w565">The 16-bit encoded RGB value to be decoded. 5 bits for red, 6 bits for green and 5 bits for blue.</param>
        /// <returns></returns>
        private static Color4 Decode565(ushort w565)
        {
            return new Color4()
            {
                R = ((w565 >> 11) & 31) * (1.0f / 31.0f),
                G = ((w565 >> 5) & 63) * (1.0f / 63.0f),
                B = ((w565 >> 0) & 31) * (1.0f / 31.0f),
                A = 1.0f,
            };
        }

        private static ushort Encode565(Color4 pColor)
        {

            pColor.R = (pColor.R < 0.0f) ? 0.0f : (pColor.R > 1.0f) ? 1.0f : pColor.R;
            pColor.G = (pColor.G < 0.0f) ? 0.0f : (pColor.G > 1.0f) ? 1.0f : pColor.G;
            pColor.B = (pColor.B < 0.0f) ? 0.0f : (pColor.B > 1.0f) ? 1.0f : pColor.B;
            return (ushort)(((int)(pColor.R * 31.0f + 0.5f) << 11) | ((int)(pColor.G * 63.0f + 0.5f) << 5) | ((int)(pColor.B * 31.0f + 0.5f) << 0));
        }

        internal static bool HasFlags(BCFlags value, BCFlags flags)
        {
            return (value & flags) == flags;
        }


        //-------------------------------------------------------------------------------------
        private static void OptimizeRGB(
            out Color4 pX,
            out Color4 pY,
            Color4[] pPoints,
            uint cSteps,
            BCFlags flags)
        {
            float[] pC = (3 == cSteps) ? pC3 : pC4;
            float[] pD = (3 == cSteps) ? pD3 : pD4;

            // Find Min and Max points, as starting point
            Color4 X = (HasFlags(flags, BCFlags.UNIFORM)) ? new Color4(1f, 1f, 1f, 1f) : g_Luminance;
            Color4 Y = new Color4(0.0f, 0.0f, 0.0f, 1.0f);

            for (uint iPoint = 0; iPoint < NUM_PIXELS_PER_BLOCK; iPoint++)
            {
#if COLOR_WEIGHTS
                if (pPoints[iPoint].A > 0.0f)
#endif // COLOR_WEIGHTS
                {
                    if (pPoints[iPoint].R < X.R)
                        X.R = pPoints[iPoint].R;

                    if (pPoints[iPoint].G < X.G)
                        X.G = pPoints[iPoint].G;

                    if (pPoints[iPoint].B < X.B)
                        X.B = pPoints[iPoint].B;

                    if (pPoints[iPoint].R > Y.R)
                        Y.R = pPoints[iPoint].R;

                    if (pPoints[iPoint].G > Y.G)
                        Y.G = pPoints[iPoint].G;

                    if (pPoints[iPoint].B > Y.B)
                        Y.B = pPoints[iPoint].B;
                }
            }

            // Diagonal axis
            Color4 AB = new Color4(Y.R - X.R, Y.G - X.G, Y.B - X.B, 0.0f);

            float fAB = AB.R * AB.R + AB.G * AB.G + AB.B * AB.B;

            // Single color block.. no need to root-find
            if (fAB < FLT_MIN)
            {
                pX.R = X.R; pX.G = X.G; pX.B = X.B; pX.A = 1.0f;
                pY.R = Y.R; pY.G = Y.G; pY.B = Y.B; pY.A = 1.0f;
                return;
            }

            // Try all four axis directions, to determine which diagonal best fits data
            float fABInv = 1.0f / fAB;

            Color4 Dir = new Color4(AB.R * fABInv, AB.G * fABInv, AB.B * fABInv, 0.0f);

            Color4 Mid = new Color4(
                (X.R + Y.R) * 0.5f,
                (X.G + Y.G) * 0.5f,
                (X.B + Y.B) * 0.5f,
                0.0f);

            float[] fDir = new float[4];

            for (uint iPoint = 0; iPoint < NUM_PIXELS_PER_BLOCK; iPoint++)
            {
                Color4 Pt;
                Pt.R = (pPoints[iPoint].R - Mid.R) * Dir.R;
                Pt.G = (pPoints[iPoint].G - Mid.G) * Dir.G;
                Pt.B = (pPoints[iPoint].B - Mid.B) * Dir.B;
                Pt.A = 0.0f;

                float f;

#if COLOR_WEIGHTS
                f = Pt.R + Pt.G + Pt.B;
                fDir[0] += pPoints[iPoint].a * f * f;

                f = Pt.R + Pt.G - Pt.B;
                fDir[1] += pPoints[iPoint].a * f * f;

                f = Pt.R - Pt.G + Pt.B;
                fDir[2] += pPoints[iPoint].a * f * f;

                f = Pt.R - Pt.G - Pt.B;
                fDir[3] += pPoints[iPoint].a * f * f;
#else
                f = Pt.R + Pt.G + Pt.B;
                fDir[0] += f * f;

                f = Pt.R + Pt.G - Pt.B;
                fDir[1] += f * f;

                f = Pt.R - Pt.G + Pt.B;
                fDir[2] += f * f;

                f = Pt.R - Pt.G - Pt.B;
                fDir[3] += f * f;
#endif // COLOR_WEIGHTS
            }

            float fDirMax = fDir[0];
            uint iDirMax = 0;

            for (uint iDir = 1; iDir < 4; iDir++)
            {
                if (fDir[iDir] > fDirMax)
                {
                    fDirMax = fDir[iDir];
                    iDirMax = iDir;
                }
            }

            if ((iDirMax & 2) == 2)
            {
                float f = X.G; X.G = Y.G; Y.G = f;
            }

            if ((iDirMax & 1) == 1)
            {
                float f = X.B; X.B = Y.B; Y.B = f;
            }


            // Two color block.. no need to root-find
            if (fAB < 1.0f / 4096.0f)
            {
                pX.R = X.R; pX.G = X.G; pX.B = X.B; pX.A = 1.0f;
                pY.R = Y.R; pY.G = Y.G; pY.B = Y.B; pY.A = 1.0f;
                return;
            }

            // Use Newton's Method to find local minima of sum-of-squares error.
            float fSteps = cSteps - 1;

            for (uint iIteration = 0; iIteration < 8; iIteration++)
            {
                // Calculate new steps
                Color4[] pSteps = new Color4[4];

                for (uint iStep = 0; iStep < cSteps; iStep++)
                {
                    pSteps[iStep] = new Color4()
                    {
                        R = X.R * pC[iStep] + Y.R * pD[iStep],
                        G = X.G * pC[iStep] + Y.G * pD[iStep],
                        B = X.B * pC[iStep] + Y.B * pD[iStep],
                        A = 1.0f,
                    };
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
                float d2X = 0f;
                float d2Y = 0f;
                Color4 dX = Color4.Zero;
                Color4 dY = Color4.Zero;

                for (uint iPoint = 0; iPoint < NUM_PIXELS_PER_BLOCK; iPoint++)
                {
                    float fDot = (pPoints[iPoint].R - X.R) * Dir.R +
                        (pPoints[iPoint].G - X.G) * Dir.G +
                        (pPoints[iPoint].B - X.B) * Dir.B;


                    uint iStep;
                    if (fDot <= 0.0f)
                        iStep = 0;
                    else if (fDot >= fSteps)
                        iStep = cSteps - 1;
                    else
                        iStep = (uint)(fDot + 0.5f);


                    Color4 Diff = new Color4()
                    {
                        R = pSteps[iStep].R - pPoints[iPoint].R,
                        G = pSteps[iStep].G - pPoints[iPoint].G,
                        B = pSteps[iStep].B - pPoints[iPoint].B,
                        A = 0.0f,
                    };

#if COLOR_WEIGHTS
                float fC = pC[iStep] * pPoints[iPoint].A * (1.0f / 8.0f);
                float fD = pD[iStep] * pPoints[iPoint].A * (1.0f / 8.0f);
#else
                    float fC = pC[iStep] * (1.0f / 8.0f);
                    float fD = pD[iStep] * (1.0f / 8.0f);
#endif // COLOR_WEIGHTS

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

            pX.R = X.R; pX.G = X.G; pX.B = X.B; pX.A = 1.0f;
            pY.R = Y.R; pY.G = Y.G; pY.B = Y.B; pY.A = 1.0f;
        }

        private static unsafe uint FloatToUInt(float f)
        {
            return *(uint*)&f;
        }

        private static unsafe float UIntToFloat(uint i)
        {
            return *(float*)&i;
        }

        /// <summary>
        /// Based on XMVectorSelect from DirectXMathVector.inl.
        /// </summary>
        /// <param name="V1"></param>
        /// <param name="V2"></param>
        /// <param name="Control"></param>
        /// <returns></returns>
        private static Color4 SelectColor(Color4 V1, Color4 V2, Vector4UI Control)
        {
            return new Color4()
            {
                R = UIntToFloat((FloatToUInt(V1.R) & ~Control.X) | (FloatToUInt(V2.R) & Control.X)),
                G = UIntToFloat((FloatToUInt(V1.G) & ~Control.Y) | (FloatToUInt(V2.G) & Control.Y)),
                B = UIntToFloat((FloatToUInt(V1.B) & ~Control.Z) | (FloatToUInt(V2.B) & Control.Z)),
                A = UIntToFloat((FloatToUInt(V1.A) & ~Control.W) | (FloatToUInt(V2.A) & Control.W)),
            };
        }

        private static Color4 Load565(ushort pSource)
        {
            return new Color4()
            {
                R = (pSource & 0x1F),
                G = (pSource >> 5) & 0x3F,
                B = (pSource >> 11) & 0x1F,
            };
        }

        //-------------------------------------------------------------------------------------
        private static Color4[] DecodeBC1(D3DX_BC1 pBC, bool isbc1)
        {
            Color4[] pColor = new Color4[NUM_PIXELS_PER_BLOCK];
            Color4 clr0 = Load565(pBC.rgb[0]);
            Color4 clr1 = Load565(pBC.rgb[1]);

            clr0 *= s_Scale;
            clr1 *= s_Scale;

            clr0 = Color4.Swizzle(clr0, 2, 1, 0, 3); // XMVectorSwizzle < 2, 1, 0, 3 > (clr0);
            clr1 = Color4.Swizzle(clr1, 2, 1, 0, 3); // XMVectorSwizzle < 2, 1, 0, 3 > (clr1);

            clr0 = SelectColor(g_XMIdentityR3, clr0, g_XMSelect1110);
            clr1 = SelectColor(g_XMIdentityR3, clr1, g_XMSelect1110);

            Color4 clr2, clr3;
            if (isbc1 && (pBC.rgb[0] <= pBC.rgb[1]))
            {
                clr2 = Color4.Lerp(clr0, clr1, 0.5f);
                clr3 = Color4.Zero;  // Alpha of 0
            }
            else
            {
                clr2 = Color4.Lerp(clr0, clr1, 1f / 3f);
                clr3 = Color4.Lerp(clr0, clr1, 2f / 3f);
            }

            uint dw = pBC.bitmap;

            for (uint i = 0; i < NUM_PIXELS_PER_BLOCK; ++i, dw >>= 2)
            {
                switch (dw & 3)
                {
                    case 0: pColor[i] = clr0; break;
                    case 1: pColor[i] = clr1; break;
                    case 2: pColor[i] = clr2; break;

                    case 3:
                    default: pColor[i] = clr3; break;
                }
            }

            return pColor;
        }


        //-------------------------------------------------------------------------------------
        private static D3DX_BC1 EncodeBC1(Color4[] pColor,
            bool bColorKey,
            float threshold,
            BCFlags flags)
        {
            if (HasFlags(flags, BCFlags.DITHER_A))
            {
                float[] fError = new float[NUM_PIXELS_PER_BLOCK];

                for (uint i = 0; i < NUM_PIXELS_PER_BLOCK; ++i)
                {
                    Color4 clr = pColor[i];
                    float fAlph = clr.A + fError[i];

                    pColor[i].R = clr.R;
                    pColor[i].G = clr.G;
                    pColor[i].B = clr.B;
                    pColor[i].A = (uint)(clr.A + fError[i] + 0.5f);

                    float fDiff = fAlph - pColor[i].A;

                    if (3 != (i & 3))
                    {
                        Debug.Assert(i < 15);
                        fError[i + 1] += fDiff * (7.0f / 16.0f);
                    }

                    if (i < 12)
                    {
                        if ((i & 3) == 3)
                            fError[i + 3] += fDiff * (3.0f / 16.0f);

                        fError[i + 4] += fDiff * (5.0f / 16.0f);

                        if (3 != (i & 3))
                        {
                            Debug.Assert(i < 11);
                            fError[i + 5] += fDiff * (1.0f / 16.0f);
                        }
                    }
                }
            }

            D3DX_BC1 pBC = new D3DX_BC1();

            // Determine if we need to colorkey this block
            uint uSteps;

            if (bColorKey)
            {
                int uColorKey = 0;

                for (uint i = 0; i < NUM_PIXELS_PER_BLOCK; ++i)
                {
                    if (pColor[i].A < threshold)
                        uColorKey++;
                }

                if (NUM_PIXELS_PER_BLOCK == uColorKey)
                {
                    pBC.rgb[0] = 0x0000;
                    pBC.rgb[1] = 0xffff;
                    pBC.bitmap = 0xffffffff;
                    return pBC;
                }

                uSteps = (uColorKey > 0) ? 3U : 4U;
            }
            else
            {
                uSteps = 4U;
            }

            // Quantize block to R56B5, using Floyd Stienberg error diffusion.  This 
            // increases the chance that colors will map directly to the quantized 
            // axis endpoints.
            Color4[] Color = new Color4[NUM_PIXELS_PER_BLOCK];
            Color4[] Error = new Color4[NUM_PIXELS_PER_BLOCK];

            if (HasFlags(flags, BCFlags.DITHER_RGB))
                Array.Clear(Error, 0, Error.Length);

            for (uint i = 0; i < NUM_PIXELS_PER_BLOCK; ++i)
            {
                Color4 Clr;
                Clr.R = pColor[i].R;
                Clr.G = pColor[i].G;
                Clr.B = pColor[i].B;
                Clr.A = 1.0f;

                if (HasFlags(flags, BCFlags.DITHER_RGB))
                {
                    Clr.R += Error[i].R;
                    Clr.G += Error[i].G;
                    Clr.B += Error[i].B;
                }

                Color[i].R = (Clr.R * 31.0f + 0.5f) * (1.0f / 31.0f);
                Color[i].G = (Clr.G * 63.0f + 0.5f) * (1.0f / 63.0f);
                Color[i].B = (Clr.B * 31.0f + 0.5f) * (1.0f / 31.0f);

# if COLOR_WEIGHTS
                Color[i].A = pColor[i].A;
#else
                Color[i].A = 1.0f;
#endif // COLOR_WEIGHTS

                if (HasFlags(flags, BCFlags.DITHER_RGB))
                {
                    Color4 Diff;
                    Diff.R = Color[i].A * (Clr.R - Color[i].R);
                    Diff.G = Color[i].A * (Clr.G - Color[i].G);
                    Diff.B = Color[i].A * (Clr.B - Color[i].B);
                    Diff.A = 0.0f;

                    if (3 != (i & 3))
                    {
                        Debug.Assert(i < 15);
                        Error[i + 1].R += Diff.R * (7.0f / 16.0f);
                        Error[i + 1].G += Diff.G * (7.0f / 16.0f);
                        Error[i + 1].B += Diff.B * (7.0f / 16.0f);
                    }

                    if (i < 12)
                    {
                        if ((i & 3) == 3)
                        {
                            Error[i + 3].R += Diff.R * (3.0f / 16.0f);
                            Error[i + 3].G += Diff.G * (3.0f / 16.0f);
                            Error[i + 3].B += Diff.B * (3.0f / 16.0f);
                        }

                        Error[i + 4].R += Diff.R * (5.0f / 16.0f);
                        Error[i + 4].G += Diff.G * (5.0f / 16.0f);
                        Error[i + 4].B += Diff.B * (5.0f / 16.0f);

                        if (3 != (i & 3))
                        {
                            Debug.Assert(i < 11);
                            Error[i + 5].R += Diff.R * (1.0f / 16.0f);
                            Error[i + 5].G += Diff.G * (1.0f / 16.0f);
                            Error[i + 5].B += Diff.B * (1.0f / 16.0f);
                        }
                    }
                }

                if (!HasFlags(flags, BCFlags.UNIFORM))
                {
                    Color[i].R *= g_Luminance.R;
                    Color[i].G *= g_Luminance.G;
                    Color[i].B *= g_Luminance.B;
                }
            }

            // Perform 6D root finding function to find two endpoints of color axis.
            // Then quantize and sort the endpoints depending on mode.
            Color4 ColorA, ColorB, ColorC, ColorD;

            OptimizeRGB(out ColorA, out ColorB, Color, uSteps, flags);

            if (HasFlags(flags, BCFlags.UNIFORM))
            {
                ColorC = ColorA;
                ColorD = ColorB;
            }
            else
            {
                ColorC.R = ColorA.R * g_LuminanceInv.R;
                ColorC.G = ColorA.G * g_LuminanceInv.G;
                ColorC.B = ColorA.B * g_LuminanceInv.B;
                ColorC.A = ColorA.A;

                ColorD.R = ColorB.R * g_LuminanceInv.R;
                ColorD.G = ColorB.G * g_LuminanceInv.G;
                ColorD.B = ColorB.B * g_LuminanceInv.B;
                ColorD.A = ColorB.A;
            }

            ushort wColorA = Encode565(ColorC);
            ushort wColorB = Encode565(ColorD);

            if ((uSteps == 4) && (wColorA == wColorB))
            {
                pBC.rgb[0] = wColorA;
                pBC.rgb[1] = wColorB;
                pBC.bitmap = 0x00000000;
                return pBC;
            }

            ColorC = Decode565(wColorA);
            ColorD = Decode565(wColorB);

            if (HasFlags(flags, BCFlags.UNIFORM))
            {
                ColorA = ColorC;
                ColorB = ColorD;
            }
            else
            {
                ColorA.R = ColorC.R * g_Luminance.R;
                ColorA.G = ColorC.G * g_Luminance.G;
                ColorA.B = ColorC.B * g_Luminance.B;

                ColorB.R = ColorD.R * g_Luminance.R;
                ColorB.G = ColorD.G * g_Luminance.G;
                ColorB.B = ColorD.B * g_Luminance.B;
            }

            // Calculate color steps
            Color4[] Step = new Color4[4];

            if ((3 == uSteps) == (wColorA <= wColorB))
            {
                pBC.rgb[0] = wColorA;
                pBC.rgb[1] = wColorB;

                Step[0] = ColorA;
                Step[1] = ColorB;
            }
            else
            {
                pBC.rgb[0] = wColorB;
                pBC.rgb[1] = wColorA;

                Step[0] = ColorB;
                Step[1] = ColorA;
            }

            uint[] pSteps;

            if (3 == uSteps)
            {
                pSteps = pSteps3;
                Step[2] = Color4.Lerp(Step[0], Step[1], 0.5f);
            }
            else
            {
                pSteps = pSteps4;
                Step[2] = Color4.Lerp(Step[0], Step[1], 1.0f / 3.0f);
                Step[3] = Color4.Lerp(Step[0], Step[1], 2.0f / 3.0f);
            }

            // Calculate color direction
            Color4 Dir;
            Dir.R = Step[1].R - Step[0].R;
            Dir.G = Step[1].G - Step[0].G;
            Dir.B = Step[1].B - Step[0].B;
            Dir.A = 0.0f;

            float fSteps = (uSteps - 1);
            float fScale = (wColorA != wColorB) ? (fSteps / (Dir.R * Dir.R + Dir.G * Dir.G + Dir.B * Dir.B)) : 0.0f;

            Dir.R *= fScale;
            Dir.G *= fScale;
            Dir.B *= fScale;

            // Encode colors
            uint dw = 0;
            if (HasFlags(flags, BCFlags.DITHER_RGB))
                Array.Clear(Error, 0, Error.Length);

            for (uint i = 0; i < NUM_PIXELS_PER_BLOCK; ++i)
            {
                if ((3 == uSteps) && (pColor[i].A < threshold))
                {
                    dw = (3u << 30) | (dw >> 2);
                }
                else
                {
                    Color4 Clr;
                    if (HasFlags(flags, BCFlags.UNIFORM))
                    {
                        Clr.R = pColor[i].R;
                        Clr.G = pColor[i].G;
                        Clr.B = pColor[i].B;
                    }
                    else
                    {
                        Clr.R = pColor[i].R * g_Luminance.R;
                        Clr.G = pColor[i].G * g_Luminance.G;
                        Clr.B = pColor[i].B * g_Luminance.B;
                    }
                    Clr.A = 1.0f;

                    if (HasFlags(flags, BCFlags.DITHER_RGB))
                    {
                        Clr.R += Error[i].R;
                        Clr.G += Error[i].G;
                        Clr.B += Error[i].B;
                    }

                    float fDot = (Clr.R - Step[0].R) * Dir.R + (Clr.G - Step[0].G) * Dir.G + (Clr.B - Step[0].B) * Dir.B;

                    uint iStep;
                    if (fDot <= 0.0f)
                        iStep = 0;
                    else if (fDot >= fSteps)
                        iStep = 1;
                    else
                        iStep = pSteps[(uint)(fDot + 0.5f)];

                    dw = (iStep << 30) | (dw >> 2);

                    if (HasFlags(flags, BCFlags.DITHER_RGB))
                    {
                        Color4 Diff;
                        Diff.R = Color[i].A * (Clr.R - Step[iStep].R);
                        Diff.G = Color[i].A * (Clr.G - Step[iStep].G);
                        Diff.B = Color[i].A * (Clr.B - Step[iStep].B);
                        Diff.A = 0.0f;

                        if (3 != (i & 3))
                        {
                            Error[i + 1].R += Diff.R * (7.0f / 16.0f);
                            Error[i + 1].G += Diff.G * (7.0f / 16.0f);
                            Error[i + 1].B += Diff.B * (7.0f / 16.0f);
                        }

                        if (i < 12)
                        {
                            if ((i & 3) == 3)
                            {
                                Error[i + 3].R += Diff.R * (3.0f / 16.0f);
                                Error[i + 3].G += Diff.G * (3.0f / 16.0f);
                                Error[i + 3].B += Diff.B * (3.0f / 16.0f);
                            }

                            Error[i + 4].R += Diff.R * (5.0f / 16.0f);
                            Error[i + 4].G += Diff.G * (5.0f / 16.0f);
                            Error[i + 4].B += Diff.B * (5.0f / 16.0f);

                            if (3 != (i & 3))
                            {
                                Error[i + 5].R += Diff.R * (1.0f / 16.0f);
                                Error[i + 5].G += Diff.G * (1.0f / 16.0f);
                                Error[i + 5].B += Diff.B * (1.0f / 16.0f);
                            }
                        }
                    }
                }
            }

            pBC.bitmap = dw;
            return pBC;
        }

        //-------------------------------------------------------------------------------------
#if COLOR_WEIGHTS
    void EncodeSolidBC1(_Out_ D3DX_BC1 *pBC, _In_reads_(NUM_PIXELS_PER_BLOCK) const Color4* pColor)
    {
#if COLOR_AVG_0WEIGHTS
        // Compute avg color
        Color4 Color;
        Color.R = pColor[0].R;
        Color.G = pColor[0].G;
        Color.B = pColor[0].B;

        for (size_t i = 1; i < NUM_PIXELS_PER_BLOCK; ++i)
        {
            Color.R += pColor[i].R;
            Color.G += pColor[i].G;
            Color.B += pColor[i].B;
        }

        Color.R *= 1.0f / 16.0f;
        Color.G *= 1.0f / 16.0f;
        Color.B *= 1.0f / 16.0f;

        uint16_t wColor = Encode565(&Color);
#else
        uint16_t wColor = 0x0000;
#endif // COLOR_AVG_0WEIGHTS

        // Encode solid block
        pBC.Rgb[0] = wColor;
        pBC.Rgb[1] = wColor;
        pBC.Bitmap = 0x00000000;
    }
#endif // COLOR_WEIGHTS


        //=====================================================================================
        // Entry points
        //=====================================================================================

        //-------------------------------------------------------------------------------------
        // BC1 Compression
        //-------------------------------------------------------------------------------------
        internal static Color4[] D3DXDecodeBC1(D3DX_BC1 pBC1)
        {
            return DecodeBC1(pBC1, true);
        }

        internal static D3DX_BC1 D3DXEncodeBC1(Color4[] pColor, float threshold, BCFlags flags)
        {
            Color4[] Color = new Color4[NUM_PIXELS_PER_BLOCK];

            if (HasFlags(flags, BCFlags.DITHER_A))
            {
                float[] fError = new float[NUM_PIXELS_PER_BLOCK];

                for (uint i = 0; i < NUM_PIXELS_PER_BLOCK; ++i)
                {
                    Color4 clr = pColor[i];

                    float fAlph = clr.A + fError[i];

                    Color[i].R = clr.R;
                    Color[i].G = clr.G;
                    Color[i].B = clr.B;
                    Color[i].A = clr.A + fError[i] + 0.5f;

                    float fDiff = fAlph - Color[i].A;

                    if (3 != (i & 3))
                    {
                        Debug.Assert(i < 15);
                        fError[i + 1] += fDiff * (7.0f / 16.0f);
                    }

                    if (i < 12)
                    {
                        if ((i & 3) == 3)
                            fError[i + 3] += fDiff * (3.0f / 16.0f);

                        fError[i + 4] += fDiff * (5.0f / 16.0f);

                        if (3 != (i & 3))
                        {
                            Debug.Assert(i < 11);
                            fError[i + 5] += fDiff * (1.0f / 16.0f);
                        }
                    }
                }
            }
            else
            {
                for (uint i = 0; i < NUM_PIXELS_PER_BLOCK; ++i)
                    Color[i] = pColor[i];
            }

            return EncodeBC1(Color, true, threshold, flags);
        }


        //-------------------------------------------------------------------------------------
        // BC2 Compression
        //-------------------------------------------------------------------------------------
        internal static Color4[] D3DXDecodeBC2(D3DX_BC2 pBC2)
        {
            Color4[] pColor = DecodeBC1(pBC2.bc1, false);

            // 4-bit alpha part
            uint dw = pBC2.bitmap[0];

            for (uint i = 0; i < 8; ++i, dw >>= 4)
                pColor[i].A = (dw & 0xf) * (1.0f / 15.0f);

            dw = pBC2.bitmap[1];

            for (uint i = 8; i < NUM_PIXELS_PER_BLOCK; ++i, dw >>= 4)
                pColor[i].A = (dw & 0xf) * (1.0f / 15.0f);

            return pColor;
        }

        internal static D3DX_BC2 D3DXEncodeBC2( Color4[] pColor, BCFlags flags)
        {
            D3DX_BC2 pBC2 = new D3DX_BC2();
            Color4[] Color = new Color4[NUM_PIXELS_PER_BLOCK];
            for (uint i = 0; i < NUM_PIXELS_PER_BLOCK; ++i)
                Color[i] = pColor[i];

            // 4-bit alpha part.  Dithered using Floyd Stienberg error diffusion.
            pBC2.bitmap[0] = 0;
            pBC2.bitmap[1] = 0;

            float[] fError = new float[NUM_PIXELS_PER_BLOCK];
            for (uint i = 0; i < NUM_PIXELS_PER_BLOCK; ++i)
            {
                float fAlph = Color[i].A;
                if (HasFlags(flags, BCFlags.DITHER_A))
                    fAlph += fError[i];

                uint u = (uint)(fAlph * 15.0f + 0.5f);

                pBC2.bitmap[i >> 3] >>= 4;
                pBC2.bitmap[i >> 3] |= (u << 28);

                if (HasFlags(flags, BCFlags.DITHER_A))
                {
                    float fDiff = fAlph - (float)u * (1.0f / 15.0f);

                    if (3 != (i & 3))
                    {
                        Debug.Assert(i < 15);
                        fError[i + 1] += fDiff * (7.0f / 16.0f);
                    }

                    if (i < 12)
                    {
                        if ((i & 3) == 3)
                            fError[i + 3] += fDiff * (3.0f / 16.0f);

                        fError[i + 4] += fDiff * (5.0f / 16.0f);

                        if (3 != (i & 3))
                        {
                            Debug.Assert(i < 11);
                            fError[i + 5] += fDiff * (1.0f / 16.0f);
                        }
                    }
                }
            }

            // RGB part
#if COLOR_WEIGHTS
        if (!pBC2.bitmap[0] && !pBC2.bitmap[1])
        {
            EncodeSolidBC1(pBC2.dxt1, Color);
            return;
        }
#endif // COLOR_WEIGHTS

            pBC2.bc1 = EncodeBC1(Color, false, 0f, flags);
            return pBC2;
        }


        //-------------------------------------------------------------------------------------
        // BC3 Compression
        //-------------------------------------------------------------------------------------
        internal static Color4[] D3DXDecodeBC3(D3DX_BC3 pBC3)
        {
            // RGB part
            Color4[] pColor = DecodeBC1(pBC3.bc1, false);

            // Adaptive 3-bit alpha part
            float[] fAlpha = new float[8];

            fAlpha[0] = pBC3.alpha[0] * (1.0f / 255.0f);
            fAlpha[1] = pBC3.alpha[1] * (1.0f / 255.0f);

            if (pBC3.alpha[0] > pBC3.alpha[1])
            {
                for (uint i = 1; i < 7; ++i)
                    fAlpha[i + 1] = (fAlpha[0] * (7 - i) + fAlpha[1] * i) * (1.0f / 7.0f);
            }
            else
            {
                for (uint i = 1; i < 5; ++i)
                    fAlpha[i + 1] = (fAlpha[0] * (5 - i) + fAlpha[1] * i) * (1.0f / 5.0f);

                fAlpha[6] = 0.0f;
                fAlpha[7] = 1.0f;
            }

            uint dw = pBC3.bitmap[0] | (uint)(pBC3.bitmap[1] << 8) | (uint)(pBC3.bitmap[2] << 16);

            for (uint i = 0; i < 8; ++i, dw >>= 3)
                pColor[i].A = fAlpha[dw & 0x7];

            dw = pBC3.bitmap[3] | (uint)(pBC3.bitmap[4] << 8) | (uint)(pBC3.bitmap[5] << 16);

            for (uint i = 8; i < NUM_PIXELS_PER_BLOCK; ++i, dw >>= 3)
                pColor[i].A = fAlpha[dw & 0x7];

            return pColor;
        }

        internal static D3DX_BC3 D3DXEncodeBC3(Color4[] pColor, BCFlags flags)
        {
            Color4[] Color = new Color4[NUM_PIXELS_PER_BLOCK];
            for (uint i = 0; i < NUM_PIXELS_PER_BLOCK; ++i)
                Color[i] = pColor[i];

            D3DX_BC3 pBC3 = new D3DX_BC3();

            // Quantize block to A8, using Floyd Stienberg error diffusion.  This 
            // increases the chance that colors will map directly to the quantized 
            // axis endpoints.
            float[] fAlpha = new float[NUM_PIXELS_PER_BLOCK];
            float[] fError = new float[NUM_PIXELS_PER_BLOCK];

            float fMinAlpha = Color[0].A;
            float fMaxAlpha = Color[0].A;

            for (uint i = 0; i < NUM_PIXELS_PER_BLOCK; ++i)
            {
                float fAlph = Color[i].A;
                if (HasFlags(flags, BCFlags.DITHER_A))
                    fAlph += fError[i];

                fAlpha[i] = (int)(fAlph * 255.0f + 0.5f) * (1.0f / 255.0f);

                if (fAlpha[i] < fMinAlpha)
                    fMinAlpha = fAlpha[i];
                else if (fAlpha[i] > fMaxAlpha)
                    fMaxAlpha = fAlpha[i];

                if (HasFlags(flags, BCFlags.DITHER_A))
                {
                    float fDiff = fAlph - fAlpha[i];

                    if (3 != (i & 3))
                    {
                        Debug.Assert(i < 15);
                        fError[i + 1] += fDiff * (7.0f / 16.0f);
                    }

                    if (i < 12)
                    {
                        if ((i & 3) == 3)
                            fError[i + 3] += fDiff * (3.0f / 16.0f);

                        fError[i + 4] += fDiff * (5.0f / 16.0f);

                        if (3 != (i & 3))
                        {
                            Debug.Assert(i < 11);
                            fError[i + 5] += fDiff * (1.0f / 16.0f);
                        }
                    }
                }
            }

#if COLOR_WEIGHTS
    if (0.0f == fMaxAlpha)
    {
        EncodeSolidBC1(&pBC3.dxt1, Color);
        pBC3.alpha[0] = 0x00;
        pBC3.alpha[1] = 0x00;
        memset(pBC3.bitmap, 0x00, 6);
    }
#endif

            // RGB part
            pBC3.bc1 = EncodeBC1(Color, false, 0f, flags);

            // Alpha part
            if (1.0f == fMinAlpha)
            {
                pBC3.alpha[0] = 0xff;
                pBC3.alpha[1] = 0xff;
                Array.Clear(pBC3.bitmap, 0, 6);
                return pBC3;
            }

            // Optimize and Quantize Min and Max values
            uint uSteps = ((0.0f == fMinAlpha) || (1.0f == fMaxAlpha)) ? 6U : 8U;

            float fAlphaA, fAlphaB;
            OptimizeAlpha(false, out fAlphaA, out fAlphaB, fAlpha, uSteps);

            byte bAlphaA = (byte)(fAlphaA * 255.0f + 0.5f);
            byte bAlphaB = (byte)(fAlphaB * 255.0f + 0.5f);

            fAlphaA = bAlphaA * (1.0f / 255.0f);
            fAlphaB = bAlphaB * (1.0f / 255.0f);

            // Setup block
            if ((8 == uSteps) && (bAlphaA == bAlphaB))
            {
                pBC3.alpha[0] = bAlphaA;
                pBC3.alpha[1] = bAlphaB;
                Array.Clear(pBC3.bitmap, 0, 6);
                return pBC3;
            }

            uint[] pSteps;
            float[] fStep = new float[8];

            if (6 == uSteps)
            {
                pBC3.alpha[0] = bAlphaA;
                pBC3.alpha[1] = bAlphaB;

                fStep[0] = fAlphaA;
                fStep[1] = fAlphaB;

                for (uint i = 1; i < 5; ++i)
                    fStep[i + 1] = (fStep[0] * (5 - i) + fStep[1] * i) * (1.0f / 5.0f);

                fStep[6] = 0.0f;
                fStep[7] = 1.0f;

                pSteps = pSteps6;
            }
            else
            {
                pBC3.alpha[0] = bAlphaB;
                pBC3.alpha[1] = bAlphaA;

                fStep[0] = fAlphaB;
                fStep[1] = fAlphaA;

                for (uint i = 1; i < 7; ++i)
                    fStep[i + 1] = (fStep[0] * (7 - i) + fStep[1] * i) * (1.0f / 7.0f);

                pSteps = pSteps8;
            }

            // Encode alpha bitmap
            float fSteps = uSteps - 1;
            float fScale = (fStep[0] != fStep[1]) ? (fSteps / (fStep[1] - fStep[0])) : 0.0f;

            if (HasFlags(flags, BCFlags.DITHER_A))
                Array.Clear(fError, 0, fError.Length);

            for (uint iSet = 0; iSet < 2; iSet++)
            {
                uint dw = 0;

                uint iMin = iSet * 8;
                uint iLim = iMin + 8;

                for (uint i = iMin; i < iLim; ++i)
                {
                    float fAlph = Color[i].A;
                    if (HasFlags(flags, BCFlags.DITHER_A))
                        fAlph += fError[i];
                    float fDot = (fAlph - fStep[0]) * fScale;

                    uint iStep;
                    if (fDot <= 0.0f)
                        iStep = ((6 == uSteps) && (fAlph <= fStep[0] * 0.5f)) ? 6U : 0U;
                    else if (fDot >= fSteps)
                        iStep = ((6 == uSteps) && (fAlph >= (fStep[1] + 1.0f) * 0.5f)) ? 7U : 1U;
                    else
                        iStep = pSteps[(uint)(fDot + 0.5f)];

                    dw = (iStep << 21) | (dw >> 3);

                    if (HasFlags(flags, BCFlags.DITHER_A))
                    {
                        float fDiff = (fAlph - fStep[iStep]);

                        if (3 != (i & 3))
                            fError[i + 1] += fDiff * (7.0f / 16.0f);

                        if (i < 12)
                        {
                            if ((i & 3) == 3)
                                fError[i + 3] += fDiff * (3.0f / 16.0f);

                            fError[i + 4] += fDiff * (5.0f / 16.0f);

                            if (3 != (i & 3))
                                fError[i + 5] += fDiff * (1.0f / 16.0f);
                        }
                    }
                }

                byte[] dwBytes = BitConverter.GetBytes(dw);
                pBC3.bitmap[0 + iSet * 3] = dwBytes[0];
                pBC3.bitmap[1 + iSet * 3] = dwBytes[1];
                pBC3.bitmap[2 + iSet * 3] = dwBytes[2];
            }

            return pBC3;
        }

        internal static void OptimizeAlpha(bool bRange, out float pX, out float pY, float[] pPoints, uint cSteps)
        {
            float[] pC = (6 == cSteps) ? pC6 : pC8;
            float[] pD = (6 == cSteps) ? pD6 : pD8;

            float MAX_VALUE = 1.0f;
            float MIN_VALUE = (bRange) ? -1.0f : 0.0f;

            // Find Min and Max points, as starting point
            float fX = MAX_VALUE;
            float fY = MIN_VALUE;

            if (8 == cSteps)
            {
                for (uint iPoint = 0; iPoint < NUM_PIXELS_PER_BLOCK; iPoint++)
                {
                    if (pPoints[iPoint] < fX)
                        fX = pPoints[iPoint];

                    if (pPoints[iPoint] > fY)
                        fY = pPoints[iPoint];
                }
            }
            else
            {
                for (uint iPoint = 0; iPoint < NUM_PIXELS_PER_BLOCK; iPoint++)
                {
                    if (pPoints[iPoint] < fX && pPoints[iPoint] > MIN_VALUE)
                        fX = pPoints[iPoint];

                    if (pPoints[iPoint] > fY && pPoints[iPoint] < MAX_VALUE)
                        fY = pPoints[iPoint];
                }

                if (fX == fY)
                {
                    fY = MAX_VALUE;
                }
            }

            // Use Newton's Method to find local minima of sum-of-squares error.
            float fSteps = cSteps - 1;

            for (uint iIteration = 0; iIteration < 8; iIteration++)
            {
                if ((fY - fX) < (1.0f / 256.0f))
                    break;

                float fScale = fSteps / (fY - fX);

                // Calculate new steps
                float[] pSteps = new float[8];

                for (uint iStep = 0; iStep < cSteps; iStep++)
                    pSteps[iStep] = pC[iStep] * fX + pD[iStep] * fY;

                if (6 == cSteps)
                {
                    pSteps[6] = MIN_VALUE;
                    pSteps[7] = MAX_VALUE;
                }

                // Evaluate function, and derivatives
                float dX = 0.0f;
                float dY = 0.0f;
                float d2X = 0.0f;
                float d2Y = 0.0f;

                for (uint iPoint = 0; iPoint < NUM_PIXELS_PER_BLOCK; iPoint++)
                {
                    float fDot = (pPoints[iPoint] - fX) * fScale;

                    uint iStep;
                    if (fDot <= 0.0f)
                        iStep = ((6 == cSteps) && (pPoints[iPoint] <= fX * 0.5f)) ? 6U : 0U;
                    else if (fDot >= fSteps)
                        iStep = ((6 == cSteps) && (pPoints[iPoint] >= (fY + 1.0f) * 0.5f)) ? 7 : (cSteps - 1);
                    else
                        iStep = (uint)(fDot + 0.5f);


                    if (iStep < cSteps)
                    {
                        // D3DX had this computation backwards (pPoints[iPoint] - pSteps[iStep])
                        // this fix improves RMS of the alpha component
                        float fDiff = pSteps[iStep] - pPoints[iPoint];

                        dX += pC[iStep] * fDiff;
                        d2X += pC[iStep] * pC[iStep];

                        dY += pD[iStep] * fDiff;
                        d2Y += pD[iStep] * pD[iStep];
                    }
                }

                // Move endpoints
                if (d2X > 0.0f)
                    fX -= dX / d2X;

                if (d2Y > 0.0f)
                    fY -= dY / d2Y;

                if (fX > fY)
                {
                    float f = fX; fX = fY; fY = f;
                }

                if ((dX * dX < (1.0f / 64.0f)) && (dY * dY < (1.0f / 64.0f)))
                    break;
            }

            pX = (fX < MIN_VALUE) ? MIN_VALUE : (fX > MAX_VALUE) ? MAX_VALUE : fX;
            pY = (fY < MIN_VALUE) ? MIN_VALUE : (fY > MAX_VALUE) ? MAX_VALUE : fY;
        }
    }
}
