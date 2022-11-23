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
        public void DegreesToRadians_Float()
        {
            float a, b, c;
            float result;

            for (int i = 0; i < ITERATIONS; i++)
            {
                a = i * 248.8f;
                b = i * 9;
                c = i;

                result = MathHelper.DegreesToRadians(i * 9.8f);
            }
        }

        [Benchmark]
        public void DegreesToRadians_Float_Generic()
        {
            float a, b, c;
            float result;

            for (int i = 0; i < ITERATIONS; i++)
            {
                a = i * 248.8f;
                b = i * 9;
                c = i;

               // result = MathHelper.DegreesToRadiansGeneric(i * 9.8f);
            }
        }

        [Benchmark]
        public void DegreesToRadians_Double()
        {
            double a, b, c;
            double result;

            for (int i = 0; i < ITERATIONS; i++)
            {
                a = i * 248.8f;
                b = i * 9;
                c = i;

                result = MathHelper.DegreesToRadians(i * 9.8);
            }
        }


        [Benchmark]
        public void DegreesToRadians_Double_Generic()
        {
            double a, b, c;
            double result;

            for (int i = 0; i < ITERATIONS; i++)
            {
                a = i * 248.8D;
                b = i * 9;
                c = i;

              //  result = MathHelper.DegreesToRadiansGeneric(i * 9.8);
            }
        }
    }
}
