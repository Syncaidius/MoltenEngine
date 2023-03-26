using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NewVector4F operator +(NewVector4F a, NewVector4F b)
        {
            Sse.Store(a.Values, Sse.Add(Sse.LoadAlignedVector128(a.Values), Sse.LoadAlignedVector128(b.Values)));
            return a;
        }
    }

    public class Vector4Benchmarks
    {
        public const int ITERATIONS = 1000;

        [Benchmark]
        public void Vector4F()
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
                Molten.Vector4F.Add(ref a, ref b, out result);
            }
        }

        [Benchmark]
        public unsafe void NewVector4F_GenericOperator()
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
    }
}
