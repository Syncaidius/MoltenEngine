using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;

namespace Molten.Benchmarks
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct NewVector4F
    {
        public fixed float Values[4];

        public ref float X => ref Values[0];

        public ref float Y => ref Values[1];

        public ref float Z => ref Values[2];

        public ref float W => ref Values[3];

        public NewVector4F(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public NewVector4F(float value)
        {
            X = value;
            Y = value;
            Z = value;
            W = value;
        }

        ///<summary>Performs a add operation on two <see cref="Vector4F"/>.</summary>
        ///<param name="a">The first <see cref="Vector4F"/> to add.</param>
        ///<param name="b">The second <see cref="Vector4F"/> to add.</param>
        ///<param name="result">Output for the result of the operation.</param>
        public static void Add(ref NewVector4F a, ref NewVector4F b, out NewVector4F result)
        {
            NewVector4F r;

            fixed (float* aPtr = a.Values)
            {
                fixed (float* bPtr = b.Values)
                {
                    Sse.Store(r.Values, Sse.Add(Sse.LoadVector128(aPtr), Sse.LoadVector128(bPtr)));
                }
            }

            result = r;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NewVector4F operator +(NewVector4F a, NewVector4F b)
        {
            Sse.Store(a.Values, Sse.Add(Sse.LoadVector128(a.Values), Sse.LoadVector128(b.Values)));
            return a;
        }
    }

    public class Vector4Benchmarks
    {
        public const int ITERATIONS = 1000;

        [Benchmark]
        public void Vector4F_ByOperator()
        {
            Vector4F a, b, result;
            for (int i = 0; i < ITERATIONS; i++)
            {
                a = new Vector4F(85640.1F, 3560.2F, 248.5f, 19.8f);
                b = new Vector4F(i);
                result = a + b;
            }
        }

        [Benchmark]
        public void Vector4F_ByRef()
        {
            Vector4F a, b, result;
            for (int i = 0; i < ITERATIONS; i++)
            {
                a = new Vector4F(85640.1F, 3560.2F, 248.5f, 19.8f);
                b = new Vector4F(i);
                Vector4F.Add(ref a, ref b, out result);
            }
        }

        [Benchmark]
        public unsafe void NewVector4F_SIMD_ByOperator()
        {
            NewVector4F a, result;
            NewVector4F b;
            for (int i = 0; i < ITERATIONS; i++)
            {
                a = new NewVector4F(85640.1F, 3560.2F, 248.5f, 19.8f);
                b = new NewVector4F(i);
                result = a + b;
            }
        }



        [Benchmark]
        public unsafe void NewVector4F_SIMD_Ref()
        {
            NewVector4F a, result;
            NewVector4F b;
            for (int i = 0; i < ITERATIONS; i++)
            {
                a = new NewVector4F(85640.1F, 3560.2F, 248.5f, 19.8f);
                b = new NewVector4F(i);
                NewVector4F.Add(ref a, ref b, out result);
            }
        }

        [Benchmark]
        public unsafe void NewVector4F_SIMD()
        {
            NewVector4F a;
            NewVector4F b;
            for (int i = 0; i < ITERATIONS; i++)
            {
                a = new NewVector4F(85640.1F, 3560.2F, 248.5f, 19.8f);
                b = new NewVector4F(i);
                Sse.Store(a.Values, Sse.Add(Sse.LoadVector128(a.Values), Sse.LoadVector128(b.Values)));
            }
        }
    }
}
