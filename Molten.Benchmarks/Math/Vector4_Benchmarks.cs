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
        public void Vector4F_Math()
        {
            Vector4F a, b, result;
            for (int i = 0; i < ITERATIONS; i++)
            {
                a = new Vector4F(i * 85640.1f, i * 3560.2f, i * 1230.3f, i * 10.7f);
                b = new Vector4F(i + 21, i - 20, i * 4567, i / 23);
                result = a + b;
            }
        }

        [Benchmark]
        public void Vector4D_Math()
        {
            Vector4D a, b, result;
            for (int i = 0; i < ITERATIONS; i++)
            {
                a = new Vector4D(i * 85640.1, i * 3560.2, i * 1230.3, i * 10.7);
                b = new Vector4D(i + 21, i - 20, i * 4567, i / 23);
                result = a + b;
            }
        }
    }
}
