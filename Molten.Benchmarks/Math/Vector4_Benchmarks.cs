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
using Microsoft.Diagnostics.Runtime;

namespace Molten.Benchmarks
{
    public interface IVector4<T>
        where T : unmanaged, INumber<T>
    {
        public T X { get; set; }

        public T Y { get; set; }

        public T Z { get; set; }

        public T W { get; set; }
    }

    public interface IVector4<T, SELF> : IVector4<T>
        where T : unmanaged, INumber<T>
        where SELF : unmanaged, IVector4<T, SELF>
    {
        public abstract static SELF operator +(SELF a, IVector4<T> b);

        public abstract static void Add(ref SELF a, ref IVector4<T> b, out SELF result);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct NewVector4F : IVector4<float, NewVector4F>
    {
        float _x;
        float _y;
        float _z;
        float _w;

        public float X
        {
            get => _x; 
            set => _x = value;
        }

        public float Y
        {
            get => _y;
            set => _y = value;
        }

        public float Z
        {
            get => _z;
            set => _z = value;
        }

        public float W
        {
            get => _w;
            set => _w = value;
        }

        public NewVector4F(float x, float y, float z, float w)
        {
            _x = x;
            _y = y;
            _z = z;
            _w = w;
        }

        public NewVector4F(float value)
        {
            _x = value;
            _y = value;
            _z = value;
            _w = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NewVector4F operator +(NewVector4F a, IVector4<float> b)
        {
            return new NewVector4F(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(ref NewVector4F a, ref IVector4<float> b, out NewVector4F result)
        {
            result = new NewVector4F(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Vector4<T>
        where T : unmanaged, INumber<T>
    {
        public T X;
        public T Y;
        public T Z;
        public T W;

        public Vector4(T x, T y, T z, T w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Vector4(T value)
        {
            X = value;
            Y = value;
            Z = value;
            W = value;
        }

        public static Vector4<T> operator +(Vector4<T> a, Vector4<T> b)
        {
            return new Vector4<T>(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        }

        public static void Add(ref Vector4<T> a, ref Vector4<T> b, out Vector4<T> result)
        {
            result = new Vector4<T>(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
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
        public unsafe void Vector4_GenericOperator()
        {
            Vector4<float> a, b, result;
            for (int i = 0; i < ITERATIONS; i++)
            {
                a = new Vector4<float>(85640.1F, 3560.2F, 248.5f, 19.8f);
                b = new Vector4<float>(i);
                result = a + b;
            }
        }

        [Benchmark]
        public unsafe void Vector4_GenericByRef()
        {
            Vector4<float> a, b, result;
            for (int i = 0; i < ITERATIONS; i++)
            {
                a = new Vector4<float>(85640.1F, 3560.2F, 248.5f, 19.8f);
                b = new Vector4<float>(i);
                Vector4<float>.Add(ref a, ref b, out result);
            }
        }

        [Benchmark]
        public unsafe void NewVector4_GenericOperator()
        {
            NewVector4F a, result;
            IVector4<float> b;
            for (int i = 0; i < ITERATIONS; i++)
            {
                a = new NewVector4F(85640.1F, 3560.2F, 248.5f, 19.8f);
                b = new NewVector4F(i);
                result = a + b;
            }
        }

        [Benchmark]
        public unsafe void NewVector4F_GenericByRef()
        {
            NewVector4F a, result;
            IVector4<float> b;

            for (int i = 0; i < ITERATIONS; i++)
            {
                a = new NewVector4F(85640.1F, 3560.2F, 248.5f, 19.8f);
                b = new NewVector4F(i);
                NewVector4F.Add(ref a, ref b, out result);
            }
        }
    }
}
