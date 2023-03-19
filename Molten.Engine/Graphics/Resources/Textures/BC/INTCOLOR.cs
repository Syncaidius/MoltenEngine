using System.Diagnostics;
using Molten.HalfPrecision;

namespace Molten.Graphics.Textures
{
    internal struct IntColor
    {
        public int R, G, B;
        public int Pad;

        public IntColor(int nr, int ng, int nb)
        {
            R = nr;
            G = ng;
            B = nb;
            Pad = 0;
        }

        public static IntColor operator +(IntColor c0, IntColor c1)
        {
            return new IntColor(c0.R + c1.R, c0.G + c1.G, c0.B + c1.B);
        }

        public static IntColor operator -(IntColor c0, IntColor c1)
        {
            return new IntColor(c0.R - c1.R, c0.G - c1.G, c0.B - c1.B);
        }

        public static IntColor operator &(IntColor c0, IntColor c1)
        {
            return new IntColor(c0.R & c1.R, c0.G & c1.G, c0.B & c1.B);
        }

        public int this[byte i]
        {
            get
            {
                switch (i)
                {
                    case 0: return R;
                    case 1: return G;
                    case 2: return B;
                    default:
                        Debug.Assert(false);
                        return 0;
                }
            }

            set
            {
                switch (i)
                {
                    case 0: R = value; break;
                    case 1: G = value; break;
                    case 2: B = value; break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            }
        }

        public void Set(Color4 c, bool bSigned)
        {
            Half4 aF16 = new Half4(c.R, c.G, c.B, c.A);

            R = F16ToINT(aF16.X, bSigned);
            G = F16ToINT(aF16.Y, bSigned);
            B = F16ToINT(aF16.Z, bSigned);
        }

        public void Clamp(int iMin, int iMax)
        {
            R = Math.Min(iMax, Math.Max(iMin, R));
            G = Math.Min(iMax, Math.Max(iMin, G));
            B = Math.Min(iMax, Math.Max(iMin, B));
        }

        public void SignExtend(Color Prec)
        {
            R = BC67.SIGN_EXTEND(R, Prec.R);
            G = BC67.SIGN_EXTEND(G, Prec.G);
            B = BC67.SIGN_EXTEND(B, Prec.B);
        }

        public Half[] ToF16(bool bSigned)
        {
            Half[] aF16 = new Half[3];
            aF16[0] = INT2F16(R, bSigned);
            aF16[1] = INT2F16(G, bSigned);
            aF16[2] = INT2F16(B, bSigned);
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
