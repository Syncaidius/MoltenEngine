﻿// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// Original code from SlimMath project. http://code.google.com/p/slimmath/
// Greetings to SlimDX Group. Original code published with the following license:
// -----------------------------------------------------------------------------
/*
* Copyright (c) 2007-2011 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Molten.DoublePrecision
{
    /// <summary>
    /// Represents a four-component color with double-precision floating-point components.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    [Serializable]
    public struct Color4D : IEquatable<Color4D>, IFormattable
    {
        private const string toStringFormat = "Alpha:{0} Red:{1} Green:{2} Blue:{3}";

        /// <summary>
        /// The Black color (0, 0, 0, 1).
        /// </summary>
        public static readonly Color4D Black = new Color4D(0.0f, 0.0f, 0.0f, 1.0f);

        /// <summary>
        /// The White color (1, 1, 1, 1).
        /// </summary>
        public static readonly Color4D White = new Color4D(1.0f, 1.0f, 1.0f, 1.0f);

        /// <summary>
        /// The transparent color (0,0,0,0). Identical to <see cref="Transparent"/>.
        /// </summary>
        public static readonly Color4D Zero = new Color4D(0, 0, 0, 0);

        /// <summary>
        /// The transparent color (0,0,0,0). Identical to <see cref="Zero"/>.
        /// </summary>
        public static readonly Color4D Transparent = new Color4D(0, 0, 0, 0);

        /// <summary>
        /// The total size of <see cref="Color4D"/>, in bytes.
        /// </summary>
        public static int SizeOf => sizeof(double) * 4;

        /// <summary>
        /// The red component of the color.
        /// </summary>
        [DataMember]
        public double R;

        /// <summary>
        /// The green component of the color.
        /// </summary>
        [DataMember]
        public double G;

        /// <summary>
        /// The blue component of the color.
        /// </summary>
        [DataMember]
        public double B;

        /// <summary>
        /// The alpha component of the color.
        /// </summary>
        [DataMember]
        public double A;

        /// <summary>
        /// Initializes a new instance of the <see cref="Color4D"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public Color4D(double value)
        {
            A = R = G = B = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color4D"/> struct.
        /// </summary>
        /// <param name="values">A pointer to an array of <see cref="double"/> values. If there is less than 4 values, undefined behaviour should be expected.</param>
        public unsafe Color4D(double* values)
        {
            R = values[0];
            G = values[1];
            B = values[2];
            A = values[3];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color4D"/> struct.
        /// </summary>
        /// <param name="red">The red component of the color.</param>
        /// <param name="green">The green component of the color.</param>
        /// <param name="blue">The blue component of the color.</param>
        /// <param name="alpha">The alpha component of the color.</param>
        public Color4D(double red, double green, double blue, double alpha)
        {
            R = red;
            G = green;
            B = blue;
            A = alpha;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color4D"/> struct.
        /// </summary>
        /// <param name="value">The red, green, blue, and alpha components of the color.</param>
        public Color4D(Vector4D value)
        {
            R = value.X;
            G = value.Y;
            B = value.Z;
            A = value.W;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color4D"/> struct.
        /// </summary>
        /// <param name="value">The red, green, and blue components of the color.</param>
        /// <param name="alpha">The alpha component of the color.</param>
        public Color4D(Vector3D value, double alpha)
        {
            R = value.X;
            G = value.Y;
            B = value.Z;
            A = alpha;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color4D"/> struct.
        /// </summary>
        /// <param name="rgba">A packed integer containing all four color components in RGBA order.</param>
        public Color4D(uint rgba)
        {
            A = ((rgba >> 24) & 255) / 255.0f;
            B = ((rgba >> 16) & 255) / 255.0f;
            G = ((rgba >> 8) & 255) / 255.0f;
            R = (rgba & 255) / 255.0f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color4D"/> struct.
        /// </summary>
        /// <param name="rgba">A packed integer containing all four color components in RGBA order.</param>
        public Color4D(int rgba)
        {
            A = ((rgba >> 24) & 255) / 255.0f;
            B = ((rgba >> 16) & 255) / 255.0f;
            G = ((rgba >> 8) & 255) / 255.0f;
            R = (rgba & 255) / 255.0f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color4D"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the red, green, blue, and alpha components of the color. This must be an array with four elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
        public Color4D(double[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 4)
                throw new ArgumentOutOfRangeException("values", "There must be four and only four input values for Color4D.");

            R = values[0];
            G = values[1];
            B = values[2];
            A = values[3];
        }

        /// <summary>
        /// Re-interprets a <see cref="double"/> pointer as a <see cref="Color4D"/>. The pointer should point to the start of data
        /// containing four (RGBA) <see cref="double"/> values.
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public unsafe static Color4D FromdoublePtr(double* ptr)
        {
            return *(Color4D*)ptr;
        }

        /// <summary>
        /// Copies the component color values of the current <see cref="Color4D"/> to the provided pointer.
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public unsafe void CopyToPtr(double* ptr)
        {
            ptr[0] = R;
            ptr[1] = G;
            ptr[2] = B;
            ptr[3] = A;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color4D"/> struct.
        /// </summary>
        /// <param name="color"><see cref="Color3"/> used to initialize the color.</param>
        public Color4D(Color3 color)
        {
          R = color.R;
          G = color.G;
          B = color.B;
          A = 1.0f;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Color4D"/> struct.
        /// </summary>
        /// <param name="color"><see cref="Color3"/> used to initialize the color.</param>
        /// <param name="alpha">The alpha component of the color.</param>
        public Color4D(Color3 color, double alpha)
        {
          R = color.R;
          G = color.G;
          B = color.B;
          A = alpha;
        }

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the red, green, blue, and alpha components, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the alpha component, 1 for the red component, 2 for the green component, and 3 for the blue component.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 3].</exception>
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return R;
                    case 1: return G;
                    case 2: return B;
                    case 3: return A;
                }

                throw new ArgumentOutOfRangeException("index", "Indices for Color4D run from 0 to 3, inclusive.");
            }

            set
            {
                switch (index)
                {
                    case 0: R = value; break;
                    case 1: G = value; break;
                    case 2: B = value; break;
                    case 3: A = value; break;
                    default: throw new ArgumentOutOfRangeException("index", "Indices for Color4D run from 0 to 3, inclusive.");
                }
            }
        }

        /// <summary>
        /// Converts the color into a packed integer.
        /// </summary>
        /// <returns>A packed integer containing all four color components.</returns>
        public int ToBgra()
        {
            uint a = (uint)(A * 255.0f) & 255;
            uint r = (uint)(R * 255.0f) & 255;
            uint g = (uint)(G * 255.0f) & 255;
            uint b = (uint)(B * 255.0f) & 255;

            uint value = b;
            value |= g << 8;
            value |= r << 16;
            value |= a << 24;

            return (int)value;
        }

        /// <summary>
        /// Converts the color into a packed integer.
        /// </summary>
        /// <returns>A packed integer containing all four color components.</returns>
        public void ToBgra(out byte r, out byte g, out byte b, out byte a)
        {
            a = (byte)(A * 255.0f);
            r = (byte)(R * 255.0f);
            g = (byte)(G * 255.0f);
            b = (byte)(B * 255.0f);
        }

        /// <summary>
        /// Converts the color into a packed integer.
        /// </summary>
        /// <returns>A packed integer containing all four color components.</returns>
        public int ToRgba()
        {
            uint a = (uint) (A * 255.0f) & 255;
            uint r = (uint) (R * 255.0f) & 255;
            uint g = (uint) (G * 255.0f) & 255;
            uint b = (uint) (B * 255.0f) & 255;

            uint value = r;
            value |= g << 8;
            value |= b << 16;
            value |= a << 24;

            return (int)value;
        }

        /// <summary>
        /// Converts the color into a three component vector.
        /// </summary>
        /// <returns>A three component vector containing the red, green, and blue components of the color.</returns>
        public Vector3D ToVector3()
        {
            return new Vector3D(R, G, B);
        }

        /// <summary>
        /// Converts the color into a four component vector.
        /// </summary>
        /// <returns>A four component vector containing all four color components.</returns>
        public Vector4D ToVector4()
        {
            return new Vector4D(R, G, B, A);
        }

        /// <summary>
        /// Creates an array containing the elements of the color.
        /// </summary>
        /// <returns>A four-element array containing the components of the color.</returns>
        public double[] ToArray()
        {
            return new double[] { R, G, B, A };
        }

        /// <summary>
        /// Calculates the dot product of two <see cref="Color4D"/>.
        /// </summary>
        /// <param name="c0">The first color.</param>
        /// <param name="c1">The second color.</param>
        /// <returns></returns>
        public static double Dot(Color4D c0, Color4D c1)
        {
            return c0.R * c1.R + c0.G * c1.G + c0.B * c1.B + c0.A * c1.A;
        }

        /// <summary>
        /// Calculates the dot product of two <see cref="Color4D"/>.
        /// </summary>
        /// <param name="c0">The first color.</param>
        /// <param name="c1">The second color.</param>
        /// <param name="result">The destination for the result.</param>
        /// <returns></returns>
        public static void Dot(ref Color4D c0,ref Color4D c1, out double result)
        {
            result = c0.R * c1.R + c0.G * c1.G + c0.B * c1.B + c0.A * c1.A;
        }

        /// <summary>
        /// Adds two colors.
        /// </summary>
        /// <param name="left">The first color to add.</param>
        /// <param name="right">The second color to add.</param>
        /// <param name="result">When the method completes, completes the sum of the two colors.</param>
        public static void Add(ref Color4D left, ref Color4D right, out Color4D result)
        {
            result.A = left.A + right.A;
            result.R = left.R + right.R;
            result.G = left.G + right.G;
            result.B = left.B + right.B;
        }

        /// <summary>
        /// Adds two colors.
        /// </summary>
        /// <param name="left">The first color to add.</param>
        /// <param name="right">The second color to add.</param>
        /// <returns>The sum of the two colors.</returns>
        public static Color4D Add(Color4D left, Color4D right)
        {
            return new Color4D(left.R + right.R, left.G + right.G, left.B + right.B, left.A + right.A);
        }

        /// <summary>
        /// Subtracts two colors.
        /// </summary>
        /// <param name="left">The first color to subtract.</param>
        /// <param name="right">The second color to subtract.</param>
        /// <param name="result">WHen the method completes, contains the difference of the two colors.</param>
        public static void Subtract(ref Color4D left, ref Color4D right, out Color4D result)
        {
            result.A = left.A - right.A;
            result.R = left.R - right.R;
            result.G = left.G - right.G;
            result.B = left.B - right.B;
        }

        /// <summary>
        /// Subtracts two colors.
        /// </summary>
        /// <param name="left">The first color to subtract.</param>
        /// <param name="right">The second color to subtract</param>
        /// <returns>The difference of the two colors.</returns>
        public static Color4D Subtract(Color4D left, Color4D right)
        {
            return new Color4D(left.R - right.R, left.G - right.G, left.B - right.B, left.A - right.A);
        }

        /// <summary>
        /// Modulates two colors.
        /// </summary>
        /// <param name="left">The first color to modulate.</param>
        /// <param name="right">The second color to modulate.</param>
        /// <param name="result">When the method completes, contains the modulated color.</param>
        public static void Modulate(ref Color4D left, ref Color4D right, out Color4D result)
        {
            result.A = left.A * right.A;
            result.R = left.R * right.R;
            result.G = left.G * right.G;
            result.B = left.B * right.B;
        }

        /// <summary>
        /// Modulates two colors.
        /// </summary>
        /// <param name="left">The first color to modulate.</param>
        /// <param name="right">The second color to modulate.</param>
        /// <returns>The modulated color.</returns>
        public static Color4D Modulate(Color4D left, Color4D right)
        {
            return new Color4D(left.R * right.R, left.G * right.G, left.B * right.B, left.A * right.A);
        }

        /// <summary>
        /// Scales a color.
        /// </summary>
        /// <param name="value">The color to scale.</param>
        /// <param name="scale">The amount by which to scale.</param>
        /// <param name="result">When the method completes, contains the scaled color.</param>
        public static void Scale(ref Color4D value, double scale, out Color4D result)
        {
            result.A = value.A * scale;
            result.R = value.R * scale;
            result.G = value.G * scale;
            result.B = value.B * scale;
        }

        /// <summary>
        /// Scales a color.
        /// </summary>
        /// <param name="value">The color to scale.</param>
        /// <param name="scale">The amount by which to scale.</param>
        /// <returns>The scaled color.</returns>
        public static Color4D Scale(Color4D value, double scale)
        {
            return new Color4D(value.R * scale, value.G * scale, value.B * scale, value.A * scale);
        }

        /// <summary>
        /// Negates a color.
        /// </summary>
        /// <param name="value">The color to negate.</param>
        /// <param name="result">When the method completes, contains the negated color.</param>
        public static void Negate(ref Color4D value, out Color4D result)
        {
            result.A = 1.0f - value.A;
            result.R = 1.0f - value.R;
            result.G = 1.0f - value.G;
            result.B = 1.0f - value.B;
        }

        /// <summary>
        /// Negates a color.
        /// </summary>
        /// <param name="value">The color to negate.</param>
        /// <returns>The negated color.</returns>
        public static Color4D Negate(Color4D value)
        {
            return new Color4D(1.0f - value.R, 1.0f - value.G, 1.0f - value.B, 1.0f - value.A);
        }

        /// <summary>
        /// Restricts a value to be within a specified range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="result">When the method completes, contains the clamped value.</param>
        public static void Clamp(ref Color4D value, double min, double max, out Color4D result)
        {
            double alpha = value.A;
            alpha = (alpha > max) ? max : alpha;
            alpha = (alpha < min) ? min : alpha;

            double red = value.R;
            red = (red > max) ? max : red;
            red = (red < min) ? min : red;

            double green = value.G;
            green = (green > max) ? max : green;
            green = (green < min) ? min : green;

            double blue = value.B;
            blue = (blue > max) ? max : blue;
            blue = (blue < min) ? min : blue;

            result = new Color4D(red, green, blue, alpha);
        }

        /// <summary>
        /// Restricts a value to be within a specified range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public static Color4D Clamp(Color4D value, double min, double max)
        {
            Color4D result;
            Clamp(ref value, min, max, out result);
            return result;
        }

        /// <summary>
        /// Restricts a value to be within a specified range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="result">When the method completes, contains the clamped value.</param>
        public static void Clamp(ref Color4D value, ref Color4D min, ref Color4D max, out Color4D result)
        {
            double alpha = value.A;
            alpha = (alpha > max.A) ? max.A : alpha;
            alpha = (alpha < min.A) ? min.A : alpha;

            double red = value.R;
            red = (red > max.R) ? max.R : red;
            red = (red < min.R) ? min.R : red;

            double green = value.G;
            green = (green > max.G) ? max.G : green;
            green = (green < min.G) ? min.G : green;

            double blue = value.B;
            blue = (blue > max.B) ? max.B : blue;
            blue = (blue < min.B) ? min.B : blue;

            result = new Color4D(red, green, blue, alpha);
        }

        /// <summary>
        /// Restricts a value to be within a specified range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        public static Color4D Clamp(Color4D value, Color4D min, Color4D max)
        {
            Color4D result;
            Clamp(ref value, ref min, ref max, out result);
            return result;
        }

        /// <summary>
        /// Performs a linear interpolation between two colors.
        /// </summary>
        /// <param name="start">Start color.</param>
        /// <param name="end">End color.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two colors.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static void Lerp(ref Color4D start, ref Color4D end, double amount, out Color4D result)
        {
            result.R = MathHelper.Lerp(start.R, end.R, amount);
            result.G = MathHelper.Lerp(start.G, end.G, amount);
            result.B = MathHelper.Lerp(start.B, end.B, amount);
            result.A = MathHelper.Lerp(start.A, end.A, amount);
        }

        /// <summary>
        /// Performs a linear interpolation between two colors.
        /// </summary>
        /// <param name="start">Start color.</param>
        /// <param name="end">End color.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <returns>The linear interpolation of the two colors.</returns>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Color4D Lerp(Color4D start, Color4D end, double amount)
        {
            Color4D result;
            Lerp(ref start, ref end, amount, out result);
            return result;
        }

        /// <summary>
        /// Performs a cubic interpolation between two colors.
        /// </summary>
        /// <param name="start">Start color.</param>
        /// <param name="end">End color.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the cubic interpolation of the two colors.</param>
        public static void SmoothStep(ref Color4D start, ref Color4D end, double amount, out Color4D result)
        {
            amount = MathHelperDP.SmoothStep(amount);
            Lerp(ref start, ref end, amount, out result);
        }

        /// <summary>
        /// Performs a cubic interpolation between two colors.
        /// </summary>
        /// <param name="start">Start color.</param>
        /// <param name="end">End color.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <returns>The cubic interpolation of the two colors.</returns>
        public static Color4D SmoothStep(Color4D start, Color4D end, double amount)
        {
            Color4D result;
            SmoothStep(ref start, ref end, amount, out result);
            return result;
        }

        /// <summary>
        /// Returns a color containing the smallest components of the specified colors.
        /// </summary>
        /// <param name="left">The first source color.</param>
        /// <param name="right">The second source color.</param>
        /// <param name="result">When the method completes, contains an new color composed of the largest components of the source colors.</param>
        public static void Max(ref Color4D left, ref Color4D right, out Color4D result)
        {
            result.A = (left.A > right.A) ? left.A : right.A;
            result.R = (left.R > right.R) ? left.R : right.R;
            result.G = (left.G > right.G) ? left.G : right.G;
            result.B = (left.B > right.B) ? left.B : right.B;
        }

        /// <summary>
        /// Returns a color containing the largest components of the specified colors.
        /// </summary>
        /// <param name="left">The first source color.</param>
        /// <param name="right">The second source color.</param>
        /// <returns>A color containing the largest components of the source colors.</returns>
        public static Color4D Max(Color4D left, Color4D right)
        {
            Color4D result;
            Max(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Returns a color containing the smallest components of the specified colors.
        /// </summary>
        /// <param name="left">The first source color.</param>
        /// <param name="right">The second source color.</param>
        /// <param name="result">When the method completes, contains an new color composed of the smallest components of the source colors.</param>
        public static void Min(ref Color4D left, ref Color4D right, out Color4D result)
        {
            result.A = (left.A < right.A) ? left.A : right.A;
            result.R = (left.R < right.R) ? left.R : right.R;
            result.G = (left.G < right.G) ? left.G : right.G;
            result.B = (left.B < right.B) ? left.B : right.B;
        }

        /// <summary>
        /// Returns a color containing the smallest components of the specified colors.
        /// </summary>
        /// <param name="left">The first source color.</param>
        /// <param name="right">The second source color.</param>
        /// <returns>A color containing the smallest components of the source colors.</returns>
        public static Color4D Min(Color4D left, Color4D right)
        {
            Color4D result;
            Min(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Adjusts the contrast of a color.
        /// </summary>
        /// <param name="value">The color whose contrast is to be adjusted.</param>
        /// <param name="contrast">The amount by which to adjust the contrast.</param>
        /// <param name="result">When the method completes, contains the adjusted color.</param>
        public static void AdjustContrast(ref Color4D value, double contrast, out Color4D result)
        {
            result.A = value.A;
            result.R = 0.5f + contrast * (value.R - 0.5f);
            result.G = 0.5f + contrast * (value.G - 0.5f);
            result.B = 0.5f + contrast * (value.B - 0.5f);
        }

        /// <summary>
        /// Adjusts the contrast of a color.
        /// </summary>
        /// <param name="value">The color whose contrast is to be adjusted.</param>
        /// <param name="contrast">The amount by which to adjust the contrast.</param>
        /// <returns>The adjusted color.</returns>
        public static Color4D AdjustContrast(Color4D value, double contrast)
        {
            return new Color4D(                
                0.5f + contrast * (value.R - 0.5f),
                0.5f + contrast * (value.G - 0.5f),
                0.5f + contrast * (value.B - 0.5f),
                value.A);
        }

        /// <summary>
        /// Adjusts the saturation of a color.
        /// </summary>
        /// <param name="value">The color whose saturation is to be adjusted.</param>
        /// <param name="saturation">The amount by which to adjust the saturation.</param>
        /// <param name="result">When the method completes, contains the adjusted color.</param>
        public static void AdjustSaturation(ref Color4D value, double saturation, out Color4D result)
        {
            double grey = value.R * 0.2125f + value.G * 0.7154f + value.B * 0.0721f;

            result.A = value.A;
            result.R = grey + saturation * (value.R - grey);
            result.G = grey + saturation * (value.G - grey);
            result.B = grey + saturation * (value.B - grey);
        }

        /// <summary>
        /// Adjusts the saturation of a color.
        /// </summary>
        /// <param name="value">The color whose saturation is to be adjusted.</param>
        /// <param name="saturation">The amount by which to adjust the saturation.</param>
        /// <returns>The adjusted color.</returns>
        public static Color4D AdjustSaturation(Color4D value, double saturation)
        {
            double grey = value.R * 0.2125f + value.G * 0.7154f + value.B * 0.0721f;

            return new Color4D(                
                grey + saturation * (value.R - grey),
                grey + saturation * (value.G - grey),
                grey + saturation * (value.B - grey),
                value.A);
        }

        /// <summary>
        /// Computes the premultiplied value of the provided color.
        /// </summary>
        /// <param name="value">The non-premultiplied value.</param>
        /// <param name="result">The premultiplied result.</param>
        public static void Premultiply(ref Color4D value, out Color4D result)
        {
            result.A = value.A;
            result.R = value.R * value.A;
            result.G = value.G * value.A;
            result.B = value.B * value.A;
        }

        /// <summary>
        /// Computes the premultiplied value of the provided color.
        /// </summary>
        /// <param name="value">The non-premultiplied value.</param>
        /// <returns>The premultiplied result.</returns>
        public static Color4D Premultiply(Color4D value)
        {
            Color4D result;
            Premultiply(ref value, out result);
            return result;
        }

        /// <summary>
        /// Adds two colors.
        /// </summary>
        /// <param name="left">The first color to add.</param>
        /// <param name="right">The second color to add.</param>
        /// <returns>The sum of the two colors.</returns>
        public static Color4D operator +(Color4D left, Color4D right)
        {
            return new Color4D(left.R + right.R, left.G + right.G, left.B + right.B, left.A + right.A);
        }

        /// <summary>
        /// Assert a color (return it unchanged).
        /// </summary>
        /// <param name="value">The color to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) color.</returns>
        public static Color4D operator +(Color4D value)
        {
            return value;
        }

        /// <summary>
        /// Subtracts two colors.
        /// </summary>
        /// <param name="left">The first color to subtract.</param>
        /// <param name="right">The second color to subtract.</param>
        /// <returns>The difference of the two colors.</returns>
        public static Color4D operator -(Color4D left, Color4D right)
        {
            return new Color4D(left.R - right.R, left.G - right.G, left.B - right.B, left.A - right.A);
        }

        /// <summary>
        /// Negates a color.
        /// </summary>
        /// <param name="value">The color to negate.</param>
        /// <returns>A negated color.</returns>
        public static Color4D operator -(Color4D value)
        {
            return new Color4D(-value.R, -value.G, -value.B, -value.A);
        }

        /// <summary>
        /// Scales a color.
        /// </summary>
        /// <param name="scale">The factor by which to scale the color.</param>
        /// <param name="value">The color to scale.</param>
        /// <returns>The scaled color.</returns>
        public static Color4D operator *(double scale, Color4D value)
        {
            return new Color4D(value.R * scale, value.G * scale, value.B * scale, value.A * scale);
        }

        /// <summary>
        /// Scales a color.
        /// </summary>
        /// <param name="value">The factor by which to scale the color.</param>
        /// <param name="scale">The color to scale.</param>
        /// <returns>The scaled color.</returns>
        public static Color4D operator *(Color4D value, double scale)
        {
            return new Color4D(value.R * scale, value.G * scale, value.B * scale, value.A * scale);
        }

        /// <summary>
        /// Modulates two colors.
        /// </summary>
        /// <param name="left">The first color to modulate.</param>
        /// <param name="right">The second color to modulate.</param>
        /// <returns>The modulated color.</returns>
        public static Color4D operator *(Color4D left, Color4D right)
        {
            return new Color4D(left.R * right.R, left.G * right.G, left.B * right.B, left.A * right.A);
        }


        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Color4D left, Color4D right)
        {
            return left.Equals(ref right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Color4D left, Color4D right)
        {
            return !left.Equals(ref right);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Color4D"/> to <see cref="Color3"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Color3D(Color4D value)
        {
            return new Color3D(value.R, value.G, value.B);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Color4D"/> to <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Vector3D(Color4D value)
        {
            return new Vector3D(value.R, value.G, value.B);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Color4D"/> to <see cref="Vector4D"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Vector4D(Color4D value)
        {
            return new Vector4D(value.R, value.G, value.B, value.A);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Vector3D"/> to <see cref="Color4D"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Color4D(Vector3D value)
        {
            return new Color4D(value.X, value.Y, value.Z, 1.0f);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Vector4D"/> to <see cref="Color4D"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Color4D(Vector4D value)
        {
            return new Color4D(value.X, value.Y, value.Z, value.W);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Vector3D"/> to <see cref="Color4D"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Color4D(ColorBGRA value)
        {
            return new Color4D(value.R, value.G, value.B, value.A);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Vector4D"/> to <see cref="Color4D"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator ColorBGRA(Color4D value)
        {
            return new ColorBGRA((float)value.R, (float)value.G, (float)value.B, (float)value.A);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Color4D"/> to <see cref="System.Int32"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator int(Color4D value)
        {
            return value.ToRgba();
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="System.Int32"/> to <see cref="Color4D"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator Color4D(int value)
        {
            return new Color4D(value);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToString(CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format to apply to each channel (double).</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, toStringFormat, A, R, G, B);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format to apply to each channel (double).</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider,
                                 toStringFormat,
                                 A.ToString(format, formatProvider),
                                 R.ToString(format, formatProvider),
                                 G.ToString(format, formatProvider),
                                 B.ToString(format, formatProvider));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = R.GetHashCode();
                hashCode = (hashCode * 397) ^ G.GetHashCode();
                hashCode = (hashCode * 397) ^ B.GetHashCode();
                hashCode = (hashCode * 397) ^ A.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="Color4D"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Color4D"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Color4D"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref Color4D other)
        {
            return A == other.A && R == other.R && G == other.G && B == other.B;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Color4D"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Color4D"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Color4D"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Color4D other)
        {
            return Equals(ref other);
        }

        public bool Equals(double[] values)
        {
            if (values.Length > 4)
                return false;

            return values[0] == R && 
                values[1] == G && 
                values[2] == B && 
                values[3] == A;
        }

        public void CopyTo(double[] values, int startIndex)
        {
            values[startIndex++] = R;
            values[startIndex++] = G;
            values[startIndex++] = B;
            values[startIndex] = G;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (!(value is Color4D))
                return false;

            var strongValue = (Color4D)value;
            return Equals(ref strongValue);
        }

        /// <summary>
        /// Returns a new <see cref="Color4D"/> with the values of the provided color's components assigned based on their index.<para/>
        /// For example, a swizzle input of (1,1,3,1) on a <see cref="Color4D"/> with RGBA values of 0.7f, 0.2f, 0f, 1.0f, will return a <see cref="Color4D"/> with values 0.2f, 0.2f, 1f, 0.2f.
        /// </summary>
        /// <param name="col"></param>
        /// <param name="rIndex">The axis index of the source color to use for the new red value.</param>
        /// <param name="gIndex">The axis index of the source color to use for the new green value.</param>
        /// <param name="bIndex">The axis index of the source color to use for the new blue value.</param>
        /// <param name="aIndex">The axis index of the source color to use for the new alpha value.</param>
        /// <returns></returns>
        public static unsafe Color4D Swizzle(Color4D col, int rIndex, int gIndex, int bIndex, int aIndex)
        {
            //return new Color4D()
            //{
            //    R = *(&col.R + (rIndex * sizeof(double))),
            //    G = *(&col.G + (gIndex * sizeof(double))),
            //    B = *(&col.B + (bIndex * sizeof(double))),
            //    A = *(&col.A + (aIndex * sizeof(double))),
            //};
            return new Color4D()
            {
                R = col[rIndex],
                G = col[gIndex],
                B = col[bIndex],
                A = col[aIndex],
            };
        }
    }
}
