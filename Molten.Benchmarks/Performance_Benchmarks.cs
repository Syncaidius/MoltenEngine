//using BenchmarkDotNet.Attributes;

//namespace Molten.Benchmarks
//{
//    public class PerformanceBenchmarks
//    {
//        public const int ITERATIONS = 1000;

//        public double AddByValue(double a, double b)
//        {
//            return a + b;
//        }

//        public void AddByRef(ref double a, ref double b, out double result )
//        {
//            result = a + b;
//        }

//        [Benchmark]
//        public void ByValue()
//        {
//            double v, result;

//            for (double i = 0; i < ITERATIONS; i++)
//            {
//                v = i * 250.7;
//                result = AddByValue(i, v);
//            }
//        }

//        [Benchmark]
//        public void ByRef()
//        {
//            double v, result;

//            for (double i = 0; i < ITERATIONS; i++)
//            {
//                v = i * 250.7;
//                AddByRef(ref i, ref v, out result);
//            }
//        }
//    }
//}
