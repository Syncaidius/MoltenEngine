using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A helper class for converting engine types to SharpDX types.</summary>
    public static class SharpDXInterop
    {
        /// <summary>Converts a Vector4 to SharpDX.Mathematics.Interop.RawVector4.</summary>
        /// <param name="val"></param>
        public static unsafe SharpDX.Mathematics.Interop.RawVector4 ToRawApi(this Vector4 val)
        {
            return *(SharpDX.Mathematics.Interop.RawVector4*)&val;
        }

        /// <summary>Converts a Vector3 to SharpDX.Mathematics.Interop.RawVector3.</summary>
        /// <param name="val"></param>
        public static unsafe SharpDX.Mathematics.Interop.RawVector3 ToRawApi(this Vector3 val)
        {
            return *(SharpDX.Mathematics.Interop.RawVector3*)&val;
        }

        /// <summary>Converts a Vector2 to SharpDX.Mathematics.Interop.RawVector2.</summary>
        /// <param name="val"></param>
        public static unsafe SharpDX.Mathematics.Interop.RawVector2 ToRawApi(this Vector2 val)
        {
            return *(SharpDX.Mathematics.Interop.RawVector2*)&val;
        }

        /// <summary>Converts a Vector4 to SharpDX.Vector4.</summary>
        /// <param name="val"></param>
        public static unsafe SharpDX.Vector4 ToApi(this Vector4 val)
        {
            return *(SharpDX.Vector4*)&val;
        }

        /// <summary>Converts a Vector3 to SharpDX.Vector3.</summary>
        /// <param name="val"></param>
        public static unsafe SharpDX.Vector3 ToApi(this Vector3 val)
        {
            return *(SharpDX.Vector3*)&val;
        }

        /// <summary>Converts a Vector2 to SharpDX.Vector2.</summary>
        /// <param name="val"></param>
        public static unsafe SharpDX.Vector2 ToApi(this Vector2 val)
        {
            return *(SharpDX.Vector2*)&val;
        }

        /// <summary>Converts a Viewport to SharpDX.Viewport.</summary>
        /// <param name="val"></param>
        public static unsafe SharpDX.Viewport ToApi(this Viewport val)
        {
            return *(SharpDX.Viewport*)&val;
        }

        /// <summary>Converts a ViewportF to SharpDX.ViewportF.</summary>
        /// <param name="val"></param>
        public static unsafe SharpDX.ViewportF ToApi(this ViewportF val)
        {
            return *(SharpDX.ViewportF*)&val;
        }

        /// <summary>Converts a Viewport to SharpDX.Viewport.</summary>
        /// <param name="val"></param>
        public static unsafe SharpDX.Mathematics.Interop.RawViewport ToRawApi(this Viewport val)
        {
            return *(SharpDX.Mathematics.Interop.RawViewport*)&val;
        }

        /// <summary>Converts a ViewportF to SharpDX.ViewportF.</summary>
        /// <param name="val"></param>
        public static unsafe SharpDX.Mathematics.Interop.RawViewportF ToRawApi(this ViewportF val)
        {
            return *(SharpDX.Mathematics.Interop.RawViewportF*)&val;
        }

        /// <summary>Converts a Rectangle to SharpDX.Rectangle.</summary>
        /// <param name="val"></param>
        public static unsafe SharpDX.Rectangle ToApi(this Rectangle val)
        {
            return *(SharpDX.Rectangle*)&val;
        }

        /// <summary>Converts a Rectangle to SharpDX.Rectangle.</summary>
        /// <param name="val"></param>
        public static unsafe SharpDX.Mathematics.Interop.RawRectangle ToRawApi(this Rectangle val)
        {
            return *(SharpDX.Mathematics.Interop.RawRectangle*)&val;
        }

        /// <summary>Converts a Rectangle to SharpDX.Rectangle.</summary>
        /// <param name="val"></param>
        public static unsafe SharpDX.Mathematics.Interop.RawRectangleF ToRawApi(this RectangleF val)
        {
            return *(SharpDX.Mathematics.Interop.RawRectangleF*)&val;
        }

        /// <summary>Converts a RectangleF to SharpDX.RectangleF.</summary>
        /// <param name="val"></param>
        public static unsafe SharpDX.RectangleF ToApi(this RectangleF val)
        {
            return *(SharpDX.RectangleF*)&val;
        }

        /// <summary>Converts a Point to SharpDX.Point.</summary>
        /// <param name="val"></param>
        public static unsafe SharpDX.Point ToApi(this IntVector2 val)
        {
            return *(SharpDX.Point*)&val;
        }

        /// <summary>Converts a Color to SharpDX.Color.</summary>
        /// <param name="val"></param>
        public static unsafe SharpDX.Color ToApi(this Color val)
        {
            return *(SharpDX.Color*)&val;
        }

        /// <summary>Converts a Color4 to SharpDX.Color4.</summary>
        /// <param name="val"></param>
        public static unsafe SharpDX.Color4 ToApi(this Color4 val)
        {
            return *(SharpDX.Color4*)&val;
        }

        /// <summary>Converts a Color4 to SharpDX.Color4.</summary>
        /// <param name="val"></param>
        public static unsafe SharpDX.Mathematics.Interop.RawColor4 ToRawApi(this Color4 val)
        {
            return *(SharpDX.Mathematics.Interop.RawColor4*)&val;
        }


        public static SharpDX.DXGI.Format ToApi(this GraphicsFormat format)
        {
            return (SharpDX.DXGI.Format)format;
        }


        /// <summary>Converts a SharpDX.Color4 to Color4.</summary>
        /// <param name="val"></param>
        public static unsafe Color4 FromApi(this SharpDX.Color4 val)
        {
            return *(Color4*)&val;
        }

        /// <summary>Converts a SharpDX.Color4 to Color4.</summary>
        /// <param name="val"></param>
        public static unsafe Color4 FromRawApi(this SharpDX.Mathematics.Interop.RawColor4 val)
        {
            return *(Color4*)&val;
        }

        /// <summary>Converts a SharpDX.Vector3 to Vector3.</summary>
        /// <param name="val"></param>
        public static unsafe Vector3 FromApi(this SharpDX.Vector3 val)
        {
            return *(Vector3*)&val;
        }

        /// <summary>Converts a SharpDX.Vector3 to Vector3.</summary>
        /// <param name="val"></param>
        public static unsafe Vector3 FromRawApi(this SharpDX.Mathematics.Interop.RawVector3 val)
        {
            return *(Vector3*)&val;
        }

        /// <summary>Converts a SharpDX.Rectangle to Rectangle.</summary>
        /// <param name="val"></param>
        public static unsafe Rectangle FromApi(this SharpDX.Rectangle val)
        {
            return *(Rectangle*)&val;
        }

        /// <summary>Converts a SharpDX.Rectangle to Rectangle.</summary>
        /// <param name="val"></param>
        public static unsafe Rectangle FromRawApi(this SharpDX.Mathematics.Interop.RawRectangle val)
        {
            return *(Rectangle*)&val;
        }

        /// <summary>Converts a SharpDX.Rectangle to Rectangle.</summary>
        /// <param name="val"></param>
        public static unsafe RectangleF FromRawApi(this SharpDX.Mathematics.Interop.RawRectangleF val)
        {
            return *(RectangleF*)&val;
        }

        public static GraphicsFormat FromApi(this SharpDX.DXGI.Format format)
        {
            return (GraphicsFormat)format;
        }
    }
}
