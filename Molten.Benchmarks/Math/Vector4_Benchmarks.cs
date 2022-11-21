//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Numerics;
//using System.Runtime.CompilerServices;
//using System.Runtime.InteropServices;
//using System.Runtime.Intrinsics;
//using System.Runtime.Intrinsics.X86;
//using System.Text;
//using System.Threading.Tasks;
//using BenchmarkDotNet.Attributes;
//using Microsoft.Diagnostics.Runtime;

//namespace Molten.Benchmarks
//{
//    public interface IVector4<T>
//        where T : unmanaged, INumber<T>
//    {
//        public T X { get; set; }

//        public T Y { get; set; }

//        public T Z { get; set; }

//        public T W { get; set; }
//    }

//    public interface IVector4<T, SELF> : IVector4<T>
//        where T : unmanaged, INumber<T>
//        where SELF : unmanaged, IVector4<T, SELF>
//    {
//        public abstract static SELF operator +(in SELF a, in SELF b);
//    }

//    [StructLayout(LayoutKind.Sequential, Pack = 4)]
//    public struct NewVector4F : IVector4<float, NewVector4F>
//    {
//        float _x;
//        float _y;
//        float _z;
//        float _w;

//        public float X
//        {
//            get => _x; 
//            set => _x = value;
//        }

//        public float Y
//        {
//            get => _y;
//            set => _y = value;
//        }

//        public float Z
//        {
//            get => _z;
//            set => _z = value;
//        }

//        public float W
//        {
//            get => _w;
//            set => _w = value;
//        }

//        public NewVector4F(float x, float y, float z, float w)
//        {
//            _x = x;
//            _y = y;
//            _z = z;
//            _w = w;
//        }

//        public NewVector4F(float value)
//        {
//            _x = value;
//            _y = value;
//            _z = value;
//            _w = value;
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static NewVector4F operator +(in NewVector4F a, in NewVector4F b)
//        {
//            return new NewVector4F(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
//        }
//    }

//    public static class Vector4
//    {
//        public static void Add<T, V>(ref V a, ref V b, out V result)
//            where T : unmanaged, INumber<T>
//            where V : unmanaged, IVector4<T>
//        {
//            result = new V()
//            {
//                X = a.X + b.X,
//                Y = a.Y + b.Y,
//                Z = a.Z + b.Z,
//                W = a.W + b.W
//            };
//        }
//    }

 
//    public class Vector4Benchmarks
//    {
//        public const int ITERATIONS = 1000;

//        [Benchmark]
//        public void Vector4F()
//        {
//            Vector4F a, b, result;
//            for (int i = 0; i < ITERATIONS; i++)
//            {
//                a = new Vector4F(85640.1F, 3560.2F, 248.5f, 19.8f);
//                b = new Vector4F(i);
//                result = a + b;
//            }
//        }

//        [Benchmark]
//        public void Vector4F_ByRef()
//        {
//            Vector4F a, b, result;
//            for (int i = 0; i < ITERATIONS; i++)
//            {
//                a = new Vector4F(85640.1F, 3560.2F, 248.5f, 19.8f);
//                b = new Vector4F(i);
//                Molten.Vector4F.Add(ref a, ref b, out result);
//            }
//        }

//        [Benchmark]
//        public unsafe void NewVector4F_GenericOperator()
//        {
//            NewVector4F a, result;
//            NewVector4F b;
//            for (int i = 0; i < ITERATIONS; i++)
//            {
//                a = new NewVector4F(85640.1F, 3560.2F, 248.5f, 19.8f);
//                b = new NewVector4F(i);
//                result = a + b;
//            }
//        }

//        [Benchmark]
//        public unsafe void NewVector4F_GenericByRef()
//        {
//            NewVector4F a, result;
//            NewVector4F b;

//            for (int i = 0; i < ITERATIONS; i++)
//            {
//                a = new NewVector4F(85640.1F, 3560.2F, 248.5f, 19.8f);
//                b = new NewVector4F(i);
//                Vector4.Add<float, NewVector4F>(ref a, ref b, out result);
//            }
//        }
//    }
//}
