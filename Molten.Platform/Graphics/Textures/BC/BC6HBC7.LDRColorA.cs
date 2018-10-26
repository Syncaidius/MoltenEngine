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
        internal struct LDRColorA
        {
            public byte r, g, b, a;

            public LDRColorA(byte _r, byte _g, byte _b, byte _a)
            {
                r = _r;
                g = _g;
                b = _b;
                a = _a;
            }

            public byte this[uint uElement]
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

                set
                {
                    switch (uElement)
                    {
                        case 0: r = value; break;
                        case 1: g = value; break;
                        case 2: b = value; break;
                        case 3: a = value; break;
                        default: Debug.Assert(false); break;
                    }
                }
            }

            public static explicit operator LDRColorA(HDRColorA c)
            {
                HDRColorA tmp = HDRColorA.Clamp(c, 0.0f, 1.0f) * 255.0f;
                return new LDRColorA()
                {
                    r = (byte)(tmp.r + 0.001f),
                    g = (byte)(tmp.g + 0.001f),
                    b = (byte)(tmp.b + 0.001f),
                    a = (byte)(tmp.a + 0.001f),
                };
            }

            internal static void InterpolateRGB(LDRColorA c0, LDRColorA c1, uint wc, uint wcprec, ref LDRColorA output)
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

            internal static void InterpolateA(LDRColorA c0, LDRColorA c1, uint wa, uint waprec, ref LDRColorA output)
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

            internal static void Interpolate(LDRColorA c0, LDRColorA c1, uint wc, uint wa, uint wcprec, uint waprec, ref LDRColorA output)
            {
                InterpolateRGB(c0, c1, wc, wcprec, ref output);
                InterpolateA(c0, c1, wa, waprec, ref output);
            }
        };
    }
}
