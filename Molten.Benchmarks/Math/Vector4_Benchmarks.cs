using BenchmarkDotNet.Attributes;
using Molten.DoublePrecision;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Molten.Benchmarks;

[StructLayout(LayoutKind.Explicit, Pack = 4)]
public unsafe struct NewVector4D
{
    [FieldOffset(0)]
    public double X;

    [FieldOffset(4)]
    public double Y;

    [FieldOffset(8)]
    public double Z;

    [FieldOffset(12)]
    public double W;

    [FieldOffset(0)]
    private Vector256<double> _smid;

    public NewVector4D(double x, double y, double z, double w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public NewVector4D(double value)
    {
        X = value;
        Y = value;
        Z = value;
        W = value;
    }

    ///<summary>Performs a add operation on two <see cref="Vector4D"/>.</summary>
    ///<param name="a">The first <see cref="Vector4D"/> to add.</param>
    ///<param name="b">The second <see cref="Vector4D"/> to add.</param>
    ///<param name="result">Output for the result of the operation.</param>
    public static unsafe void Add(ref NewVector4D a, ref NewVector4D b, out NewVector4D result)
    {
        Unsafe.SkipInit(out result);
        result._smid = Avx.Add(a._smid, b._smid);
    }


    public static NewVector4D operator +(NewVector4D a, NewVector4D b)
    {
        a._smid = Avx.Add(a._smid, b._smid);
        return a;
    }
}

public class Vector4Benchmarks
{
    public const int ITERATIONS = 1000000;

    [Benchmark]
    public void Vector4DByOperator()
    {
        Vector4D a, b;
        Vector4D result = new Vector4D();

        for (int i = 0; i < ITERATIONS; i++)
        {
            a = new Vector4D(85640.1, 3560.2, 248.5, 19.8);
            b = new Vector4D(i, i * 2, i, i + 1);
            result += a + b;
        }
    }

    [Benchmark]
    public void Vector4D_ByRef()
    {
        Vector4D a, b;
        Vector4D result = new Vector4D();

        for (int i = 0; i < ITERATIONS; i++)
        {
            a = new Vector4D(85640.1, 3560.2, 248.5, 19.8);
            b = new Vector4D(i, i * 2, i, i + 1);

            Vector4D.Add(ref a, ref b, out a);
            Vector4D.Add(ref a, ref result, out result);
        }
    }

    [Benchmark()]
    public unsafe void NewVector4D_SIMD_ByOperator()
    {
        NewVector4D result = new NewVector4D();
        NewVector4D a, b;

        for (int i = 0; i < ITERATIONS; i++)
        {
            a = new NewVector4D(85640.1D, 3560.2D, 248.5D, 19.8D);
            b = new NewVector4D(i, i * 2, i, i + 1);
            result += a + b;
        }
    }

    [Benchmark]
    public unsafe void NewVector4D_SIMD_Ref()
    {
        NewVector4D result = new NewVector4D();
        NewVector4D a, b;

        for (int i = 0; i < ITERATIONS; i++)
        {
            a = new NewVector4D(85640.1, 3560.2, 248.5, 19.8D);
            b = new NewVector4D(i, i * 2, i, i + 1);

            NewVector4D.Add(ref a, ref b, out a);
            NewVector4D.Add(ref a, ref result, out result);
        }
    }
}
