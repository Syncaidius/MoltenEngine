using Molten;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Textures
{
    internal struct INTColor
    {
        public int r, g, b;
        public int pad;

        public INTColor(int nr, int ng, int nb)
        {
            r = nr;
            g = ng;
            b = nb;
            pad = 0;
        }

        public static INTColor operator +(INTColor c0, INTColor c1)
        {
            return new INTColor(c0.r + c1.r, c0.g + c1.g, c0.b + c1.b);
        }

        public static INTColor operator -(INTColor c0, INTColor c1)
        {
            return new INTColor(c0.r - c1.r, c0.g - c1.g, c0.b - c1.b);
        }

        public static INTColor operator &(INTColor c0, INTColor c1)
        {
            return new INTColor(c0.r & c1.r, c0.g & c1.g, c0.b & c1.b);
        }

        public int this[byte i]
        {
            get
            {
                Debug.Assert(i < Marshal.SizeOf<INTColor>() / sizeof(int));
                switch (i)
                {
                    case 0: return r;
                    case 1: return g;
                    case 2: return b;
                    default:
                        Debug.Assert(false);
                        return 0;
                }
            }

            set
            {
                Debug.Assert(i < Marshal.SizeOf<INTColor>() / sizeof(int));
                switch (i)
                {
                    case 0: r = value; break;
                    case 1: g = value; break;
                    case 2: b = value; break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            }
        }

        public void Set(Color4 c, bool bSigned)
        {
            Half4 aF16 = new Half4(c.R, c.G, c.B, c.A);

            r = F16ToINT(aF16.X, bSigned);
            g = F16ToINT(aF16.Y, bSigned);
            b = F16ToINT(aF16.Z, bSigned);
        }

        public void Clamp(int iMin, int iMax)
        {
            r = Math.Min(iMax, Math.Max(iMin, r));
            g = Math.Min(iMax, Math.Max(iMin, g));
            b = Math.Min(iMax, Math.Max(iMin, b));
        }

        public void SignExtend(Color Prec)
        {
            r = BC67.SIGN_EXTEND(r, Prec.R);
            g = BC67.SIGN_EXTEND(g, Prec.G);
            b = BC67.SIGN_EXTEND(b, Prec.B);
        }

        public Half[] ToF16(bool bSigned)
        {
            Half[] aF16 = new Half[3];
            aF16[0] = INT2F16(r, bSigned);
            aF16[1] = INT2F16(g, bSigned);
            aF16[2] = INT2F16(b, bSigned);
            return aF16;
        }

        private unsafe static int F16ToINT(Half f, bool bSigned)
        {
            ushort input = *(ushort*)&f;
            int output = 0;
            int s = 0;
            if (bSigned)
            {
                s = input & BC67.F16S_MASK;
                input &= BC67.F16EM_MASK;
                if (input > BC67.F16MAX) output = BC67.F16MAX;
                else output = input;
                output = s > 0 ? -output : output;
            }
            else
            {
                if ((input & BC67.F16S_MASK) == BC67.F16S_MASK) output = 0;
                else output = input;
            }
            return output;
        }

        private unsafe static Half INT2F16(int input, bool bSigned)
        {
            ushort output;
            if (bSigned)
            {
                int s = 0;
                if (input < 0)
                {
                    s = BC67.F16S_MASK;
                    input = -input;
                }
                output = (ushort)(s | input);
            }
            else
            {
                Debug.Assert(input >= 0 && input <= BC67.F16MAX);
                output = (ushort)input;
            }

            return *(Half*)&output;
        }
    }
}
