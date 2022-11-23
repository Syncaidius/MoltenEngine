using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Molten.DoublePrecision;

namespace Molten.Benchmarks
{
    public class MathHelperBenchmarks
    {
        public const int ITERATIONS = 1000;

        [Benchmark]
        public void MathHelper_Lerp_Float()
        {
            float a, b, c;
            float result;

            for (int i = 0; i < ITERATIONS; i++)
            {
                a = i * 248.8f;
                b = i * 9;
                c = i;

                result = MathHelper.Lerp(a, b, i / 1000f);
            }
        }

        [Benchmark]
        public void MathHelper_Lerp_Float_Generic()
        {
            float a, b, c;
            float result;

            for (int i = 0; i < ITERATIONS; i++)
            {
                a = i * 248.8f;
                b = i * 9;
                c = i;

                result = MathHelper.Lerp(a, b, i / 1000f);
            }
        }


        [Benchmark]
        public void MathHelper_Lerp_Double()
        {
            double a, b, c;
            double result;

            for (int i = 0; i < ITERATIONS; i++)
            {
                a = i * 248.8D;
                b = i * 9;
                c = i;

                result = MathHelperDP.Lerp(a, b, i / 1000D);
            }
        }

        [Benchmark]
        public void MathHelper_Lerp_Double_Generic()
        {
            double a, b, c;
            double result;

            for (int i = 0; i < ITERATIONS; i++)
            {
                a = i * 248.8D;
                b = i * 9;
                c = i;

                result = MathHelper.Lerp(a, b, i / 1000D);
            }
        }
    }
}
