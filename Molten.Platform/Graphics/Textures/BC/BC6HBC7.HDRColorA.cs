using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Textures
{
    internal static partial class BC6HBC7
    {
        internal struct HDRColorA
        {
            public float r, g, b, a;

            public HDRColorA(LDRColorA c)
            {
                r = c.r * (1.0f / 255.0f);
                g = c.g * (1.0f / 255.0f);
                b = c.b * (1.0f / 255.0f);
                a = c.a * (1.0f / 255.0f);
            }

            public HDRColorA(float _r, float _g, float _b, float _a)
            {
                r = _r;
                g = _g;
                b = _b;
                a = _a;
            }

            public float this[uint uElement]
            {
                get
                {
                    switch (uElement)
                    {
                        case 0: return r;
                        case 1: return g;
                        case 2: return b;
                        case 3: return a;
                        default: Debug.Assert(false); return r;
                    }
                }
            }

            // binary operators
            public static HDRColorA operator +(HDRColorA c0, HDRColorA c1)
            {
                return new HDRColorA(c0.r + c1.r, c0.g + c1.g, c0.b + c1.b, c0.a + c1.a);
            }

            public static HDRColorA operator -(HDRColorA c0, HDRColorA c1)
            {
                return new HDRColorA(c0.r - c1.r, c0.g - c1.g, c0.b - c1.b, c0.a - c1.a);
            }

            public static HDRColorA operator *(HDRColorA c0, float f)
            {
                return new HDRColorA(c0.r * f, c0.g * f, c0.b * f, c0.a * f);
            }

            public static HDRColorA operator /(HDRColorA c0, float f)
            {
                float fInv = 1.0f / f;
                return new HDRColorA(c0.r * fInv, c0.g * fInv, c0.b * fInv, c0.a * fInv);
            }

            public static float operator *(HDRColorA c0, HDRColorA c1)
            {
                return c0.r * c1.r + c0.g * c1.g + c0.b * c1.b + c0.a * c1.a;
            }

            public static explicit operator HDRColorA(LDRColorA c)
            {
                return new HDRColorA()
                {
                    r = c.r,
                    g = c.g,
                    b = c.b,
                    a = c.a,
                };
            }

            public LDRColorA ToLDRColorA()
            {
                return new LDRColorA((byte)(r + 0.01f), (byte)(g + 0.01f), (byte)(b + 0.01f), (byte)(a + 0.01f));
            }

            public void Clamp(float fMin, float fMax)
            {
                r = Math.Min(fMax, Math.Max(fMin, r));
                g = Math.Min(fMax, Math.Max(fMin, g));
                b = Math.Min(fMax, Math.Max(fMin, b));
                a = Math.Min(fMax, Math.Max(fMin, a));
            }

            public static HDRColorA Clamp(HDRColorA val, float fMin, float fMax)
            {
                return new HDRColorA()
                {
                    r = Math.Min(fMax, Math.Max(fMin, val.r)),
                    g = Math.Min(fMax, Math.Max(fMin, val.g)),
                    b = Math.Min(fMax, Math.Max(fMin, val.b)),
                    a = Math.Min(fMax, Math.Max(fMin, val.a)),
                };

            }

            public static void InterpolateRGB(LDRColorA c0, LDRColorA c1, uint wc, uint wcprec, ref LDRColorA output)
            {
                int[] aWeights = null;
                switch (wcprec)
                {
                    case 2: aWeights = g_aWeights2; Debug.Assert(wc < 4); break;
                    case 3: aWeights = g_aWeights3; Debug.Assert(wc < 8); break;
                    case 4: aWeights = g_aWeights4; Debug.Assert(wc < 16); break;
                    default: Debug.Assert(false); output.r = output.g = output.b = 0; return;
                }
                output.r = (byte)(c0.r * (uint)(BC67_WEIGHT_MAX - aWeights[wc]) + c1.r * (uint)(aWeights[wc] + BC67_WEIGHT_ROUND) >> BC67_WEIGHT_SHIFT);
                output.g = (byte)(c0.g * (uint)(BC67_WEIGHT_MAX - aWeights[wc]) + c1.g * (uint)(aWeights[wc] + BC67_WEIGHT_ROUND) >> BC67_WEIGHT_SHIFT);
                output.b = (byte)(c0.b * (uint)(BC67_WEIGHT_MAX - aWeights[wc]) + c1.b * (uint)(aWeights[wc] + BC67_WEIGHT_ROUND) >> BC67_WEIGHT_SHIFT);
            }

            public static void InterpolateA(LDRColorA c0, LDRColorA c1, uint wa, uint waprec, ref LDRColorA output)
            {
                int[] aWeights = null;
                switch (waprec)
                {
                    case 2: aWeights = g_aWeights2; Debug.Assert(wa < 4); break;
                    case 3: aWeights = g_aWeights3; Debug.Assert(wa < 8); break;
                    case 4: aWeights = g_aWeights4; Debug.Assert(wa < 16); break;
                    default: Debug.Assert(false); output.a = 0; return;
                }
                output.a = (byte)((c0.a * (uint)(BC67_WEIGHT_MAX - aWeights[wa]) + c1.a * (uint)(aWeights[wa]) + BC67_WEIGHT_ROUND) >> BC67_WEIGHT_SHIFT);
            }

            public static void Interpolate(LDRColorA c0, LDRColorA c1, uint wc, uint wa, uint wcprec, uint waprec, ref LDRColorA output)
            {
                InterpolateRGB(c0, c1, wc, wcprec, ref output);
                InterpolateA(c0, c1, wa, waprec, ref output);
            }
        };
    }
}
