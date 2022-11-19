using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
namespace Molten.Benchmarks
{
    public class Vector4Benchmarks
    {
        public const int ITERATIONS = 1000;
        [Benchmark]
        public void Vector4D_Math()
        {
            Vector4D a, b, result;
            for (int i = 0; i < ITERATIONS; i++)
            {
                a = new Vector4D(85640.1, 3560.2, 1230.3, 10.7);
                b = new Vector4D(i);
                result = a + b;
            }
        }

        [Benchmark]
        public unsafe void Vector4D_SIMD()
        {
            if (Vector256.IsHardwareAccelerated && Avx.IsSupported)
            {
                Vector4D a, b;

                Vector256<double> pResult;
                for (int i = 0; i < ITERATIONS; i++)
                {
                    a = new Vector4D(85640.1, 3560.2, 1230.3, 10.7);
                    b = new Vector4D(i);

                    pResult = Avx.Add(*(Vector256<double>*)&a, *(Vector256<double>*)&b);
                }
            }
        }

        [Benchmark]
        public unsafe void Vector4D_Floor()
        {
            Vector4D a;

            for (int i = 0; i < ITERATIONS; i++)
            {
                a = new Vector4D(85640.1 , 3560.2 , 1230.3, 10.7);
                a *= i;

                a.Floor();
            }
        }

        [Benchmark]
        public unsafe void Vector4D_Floor_SIMD()
        {
            Vector4D a;
            Vector4D b;
            Vector256<double> result;
            for (int i = 0; i < ITERATIONS; i++)
            {
                a = new Vector4D(85640.1, 3560.2, 1230.3, 10.7);
                b = new Vector4D(i);
                result = Avx.Multiply(Avx.LoadVector256((double*)&a), Avx.LoadVector256((double*)&b));
                result = Avx.Floor(result);
                Avx.Store((double*)&a, result);
            }
        }
    }
}
