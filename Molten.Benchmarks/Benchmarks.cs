using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Molten.Benchmarks
{
    internal class Benchmarks
    {
        static void Main(string[] args)
        {
            Summary[] summary = BenchmarkRunner.Run(typeof(Benchmarks).Assembly);
        }
    }
}
