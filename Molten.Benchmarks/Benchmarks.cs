using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Molten.Benchmarks
{
    public class Benchmarks
    {
        static void Main(string[] args)
        {
            Summary[] summary = BenchmarkRunner.Run(typeof(Benchmarks).Assembly);
        }
    }
}
