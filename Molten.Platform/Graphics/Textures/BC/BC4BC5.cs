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

namespace Molten.Graphics.Textures
{
    internal static class BC4BC5
    {
        const uint dwMostNeg = (1 << (8 * sizeof(sbyte) - 1));
        const uint BLOCK_SIZE = BCHelper.BLOCK_DIMENSIONS * BCHelper.BLOCK_DIMENSIONS;

        //-------------------------------------------------------------------------------------
        // Convert a floating point value to an 8-bit SNORM
        //-------------------------------------------------------------------------------------
        internal static sbyte FloatToSNorm(float fVal)
        {
            if (float.IsNaN(fVal))
                fVal = 0;
            else
                if (fVal > 1)
                fVal = 1;    // Clamp to 1
            else
                    if (fVal < -1)
                fVal = -1;    // Clamp to -1

            fVal = fVal * (dwMostNeg - 1);

            if (fVal >= 0)
                fVal += .5f;
            else
                fVal -= .5f;

            return (sbyte)fVal;
        }


        //------------------------------------------------------------------------------
        private static void FindEndPointsBC4U(float[] theTexelsU, out byte endpointU_0, out byte endpointU_1)
        {
            // The boundary of codec for signed/unsigned format
            const float MIN_NORM = 0f;
            const float MAX_NORM = 1f;

            // Find max/min of input texels
            float fBlockMax = theTexelsU[0];
            float fBlockMin = theTexelsU[0];
            for (uint i = 0; i < BLOCK_SIZE; ++i)
            {
                if (theTexelsU[i] < fBlockMin)
                    fBlockMin = theTexelsU[i];
                else if (theTexelsU[i] > fBlockMax)
                    fBlockMax = theTexelsU[i];
            }

            //  If there are boundary values in input texels, should use 4 interpolated color values to guarantee
            //  the exact code of the boundary values.
            bool bUsing4BlockCodec = (MIN_NORM == fBlockMin || MAX_NORM == fBlockMax);

            // Using Optimize
            float fStart, fEnd;

            if (!bUsing4BlockCodec)
            {
                // 6 interpolated color values
                BC.OptimizeAlpha(false, out fStart, out fEnd, theTexelsU, 8);

                byte iStart = (byte)(fStart * 255.0f);
                byte iEnd = (byte)(fEnd * 255.0f);

                endpointU_0 = iEnd;
                endpointU_1 = iStart;
            }
            else
            {
                // 4 interpolated color values
                BC.OptimizeAlpha(false, out fStart, out fEnd, theTexelsU, 6);

                byte iStart = (byte)(fStart * 255.0f);
                byte iEnd = (byte)(fEnd * 255.0f);

                endpointU_1 = iEnd;
                endpointU_0 = iStart;
            }
        }

        private static void FindEndPointsBC4S(float[] theTexelsU, out sbyte endpointU_0, out sbyte endpointU_1)
        {
            //  The boundary of codec for signed/unsigned format
            const float MIN_NORM = -1f;
            const float MAX_NORM = 1f;

            // Find max/min of input texels
            float fBlockMax = theTexelsU[0];
            float fBlockMin = theTexelsU[0];
            for (uint i = 0; i < BLOCK_SIZE; ++i)
            {
                if (theTexelsU[i] < fBlockMin)
                    fBlockMin = theTexelsU[i];
                else if (theTexelsU[i] > fBlockMax)
                    fBlockMax = theTexelsU[i];
            }

            //  If there are boundary values in input texels, should use 4 interpolated color values to guarantee
            //  the exact code of the boundary values.
            bool bUsing4BlockCodec = (MIN_NORM == fBlockMin || MAX_NORM == fBlockMax);

            // Using Optimize
            float fStart, fEnd;

            if (!bUsing4BlockCodec)
            {
                // 6 interpolated color values
                BC.OptimizeAlpha(true, out fStart, out fEnd, theTexelsU, 8);

                sbyte iStart = FloatToSNorm(fStart);
                sbyte iEnd = FloatToSNorm(fEnd);

                endpointU_0 = iEnd;
                endpointU_1 = iStart;
            }
            else
            {
                // 4 interpolated color values
                BC.OptimizeAlpha(true, out fStart, out fEnd, theTexelsU, 6);

                sbyte iStart = FloatToSNorm(fStart);
                sbyte iEnd = FloatToSNorm(fEnd);

                endpointU_1 = iEnd;
                endpointU_0 = iStart;
            }
        }


        //------------------------------------------------------------------------------
        private static void FindEndPointsBC5U(float[] theTexelsU, float[] theTexelsV,
            out byte endpointU_0,
            out byte endpointU_1,
            out byte endpointV_0,
            out byte endpointV_1)
        {
            //Encoding the U and V channel by BC4 codec separately.
            FindEndPointsBC4U(theTexelsU, out endpointU_0, out endpointU_1);
            FindEndPointsBC4U(theTexelsV, out endpointV_0, out endpointV_1);
        }

        private static void FindEndPointsBC5S(float[] theTexelsU, float[] theTexelsV,
            out sbyte endpointU_0,
            out sbyte endpointU_1,
            out sbyte endpointV_0,
            out sbyte endpointV_1)
        {
            //Encoding the U and V channel by BC4 codec separately.
            FindEndPointsBC4S(theTexelsU, out endpointU_0, out endpointU_1);
            FindEndPointsBC4S(theTexelsV, out endpointV_0, out endpointV_1);
        }


        //------------------------------------------------------------------------------
        private static void FindClosestUNORM(BC4_UNORM pBC, float[] theTexelsU)
        {
            float[] rGradient = new float[8];
            for (uint i = 0; i < 8; ++i)
                rGradient[i] = pBC.DecodeFromIndex(i);

            for (int i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
            {
                uint uBestIndex = 0;
                float fBestDelta = 100000;
                for (uint uIndex = 0; uIndex < 8; uIndex++)
                {
                    float fCurrentDelta = Math.Abs(rGradient[uIndex] - theTexelsU[i]);
                    if (fCurrentDelta < fBestDelta)
                    {
                        uBestIndex = uIndex;
                        fBestDelta = fCurrentDelta;
                    }
                }
                pBC.SetIndex(i, uBestIndex);
            }
        }

        private static void FindClosestSNORM(BC4_SNORM pBC, float[] theTexelsU)
        {
            float[] rGradient = new float[8];
            for (uint i = 0; i < 8; ++i)
                rGradient[i] = pBC.DecodeFromIndex(i);

            for (int i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
            {
                uint uBestIndex = 0;
                float fBestDelta = 100000;
                for (uint uIndex = 0; uIndex < 8; uIndex++)
                {
                    float fCurrentDelta = Math.Abs(rGradient[uIndex] - theTexelsU[i]);
                    if (fCurrentDelta < fBestDelta)
                    {
                        uBestIndex = uIndex;
                        fBestDelta = fCurrentDelta;
                    }
                }
                pBC.SetIndex(i, uBestIndex);
            }
        }

        //=====================================================================================
        // Entry points
        //=====================================================================================

        //-------------------------------------------------------------------------------------
        // BC4 Compression
        //-------------------------------------------------------------------------------------
        internal static Color4[] D3DXDecodeBC4U(BC4_UNORM pBC4)
        {
            Color4[] pColor = new Color4[BC.NUM_PIXELS_PER_BLOCK];
            for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
                pColor[i] = new Color4(pBC4.R(i), 0, 0, 1.0f);

            return pColor;
        }

        internal static Color4[] D3DXDecodeBC4S(BC4_SNORM pBC4)
        {
            Color4[] pColor = new Color4[BC.NUM_PIXELS_PER_BLOCK];
            for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
                pColor[i] = new Color4(pBC4.R(i), 0, 0, 1.0f);
            return pColor;
        }

        internal static BC4_UNORM D3DXEncodeBC4U(Color4[] pColor)
        {
            BC4_UNORM pBC4 = new BC4_UNORM();
            float[] theTexelsU = new float[BC.NUM_PIXELS_PER_BLOCK];

            for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
                theTexelsU[i] = pColor[i].R;

            FindEndPointsBC4U(theTexelsU, out pBC4.red_0, out pBC4.red_1);
            FindClosestUNORM(pBC4, theTexelsU);
            return pBC4;
        }

        internal static BC4_SNORM D3DXEncodeBC4S(Color4[] pColor)
        {
            BC4_SNORM pBC4 = new BC4_SNORM();
            float[] theTexelsU = new float[BC.NUM_PIXELS_PER_BLOCK];

            for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
                theTexelsU[i] = pColor[i].R;

            FindEndPointsBC4S(theTexelsU, out pBC4.red_0, out pBC4.red_1);
            FindClosestSNORM(pBC4, theTexelsU);
            return pBC4;
        }


        //-------------------------------------------------------------------------------------
        // BC5 Compression
        //-------------------------------------------------------------------------------------
        internal static Color4[] D3DXDecodeBC5U(BC5_UNORM pBC5)
        {
            Color4[] pColor = new Color4[BC.NUM_PIXELS_PER_BLOCK];
            for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
                pColor[i] = new Color4(pBC5.Red.R(i), pBC5.Green.R(i), 0, 1.0f);

            return pColor;
        }

        internal static Color4[] D3DXDecodeBC5S(BC5_SNORM pBC5)
        {
            Color4[] pColor = new Color4[BC.NUM_PIXELS_PER_BLOCK];
            for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
                pColor[i] = new Color4(pBC5.Red.R(i), pBC5.Green.R(i), 0, 1.0f);
            return pColor;
        }

        internal static BC5_UNORM D3DXEncodeBC5U(Color4[] pColor)
        {
            BC5_UNORM pBC5 = new BC5_UNORM();
            float[] theTexelsU = new float[BC.NUM_PIXELS_PER_BLOCK];
            float[] theTexelsV = new float[BC.NUM_PIXELS_PER_BLOCK];

            for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
            {
                Color4 clr = pColor[i];
                theTexelsU[i] = clr.R;
                theTexelsV[i] = clr.G;
            }

            FindEndPointsBC5U(
                theTexelsU,
                theTexelsV,
                out pBC5.Red.red_0,
                out pBC5.Red.red_1,
                out pBC5.Green.red_0,
                out pBC5.Green.red_1);

            FindClosestUNORM(pBC5.Red, theTexelsU);
            FindClosestUNORM(pBC5.Green, theTexelsV);
            return pBC5;
        }

        internal static BC5_SNORM D3DXEncodeBC5S(Color4[] pColor)
        {
            BC5_SNORM pBC5 = new BC5_SNORM();
            float[] theTexelsU = new float[BC.NUM_PIXELS_PER_BLOCK];
            float[] theTexelsV = new float[BC.NUM_PIXELS_PER_BLOCK];

            for (uint i = 0; i < BC.NUM_PIXELS_PER_BLOCK; ++i)
            {
                Color4 clr = pColor[i];
                theTexelsU[i] = clr.R;
                theTexelsV[i] = clr.G;
            }

            FindEndPointsBC5S(
                theTexelsU,
                theTexelsV,
                        out pBC5.Red.red_0,
                        out pBC5.Red.red_1,
                        out pBC5.Green.red_0,
                        out pBC5.Green.red_1);

            FindClosestSNORM(pBC5.Red, theTexelsU);
            FindClosestSNORM(pBC5.Green, theTexelsV);
            return pBC5;
        }
    }
}
