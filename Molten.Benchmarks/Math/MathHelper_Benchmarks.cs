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
    public class MathHelper_Benchmarks
    {
        public const int ITERATIONS = 1000;

        [Benchmark]
        public void Vector4_Add()
        {
            Vector4D a, b, result;
            for (int i = 0; i < ITERATIONS; i++)
            {
                a = new Vector4D(i, i, i, i);
                b = new Vector4D(i + 21, i - 20, i * 4567, i / 23);
                result = a + b;
            }
        }

        [Benchmark]
        public unsafe void Vector4_Add_SIMD()
        {
            Vector4D a = Vector4D.Zero, b = Vector4D.Zero, result = Vector4D.Zero;

            for (int i = 0; i < ITERATIONS; i++)
            {
                a = new Vector4D(i, i, i, i);
                b = new Vector4D(i + 21, i - 20, i * 4567, i / 23);

                Vector256<double> sseResult = System.Runtime.Intrinsics.X86.Avx2.Add(*(Vector256<double>*)&a, *(Vector256<double>*)&b);
                result = *(Vector4D*)&sseResult;
            }
        }
    }
}
