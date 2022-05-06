using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SDF
{
    public enum SdfMode
    {
        /// <summary>
        /// Signed-distance field.
        /// </summary>
        Sdf = 0,

        /// <summary>
        /// Pseudo signed-distance field.
        /// </summary>
        Psdf = 1,

        /// <summary>
        /// Multi-channel signed-distance field.
        /// </summary>
        Msdf = 2,

        /// <summary>
        /// Multi-channel, true signed-distance field.
        /// </summary>
        Mtsdf = 3,
    }

    /// <summary>
    /// A utility class for generating signed-distance-field (SDF) textures. Also capable of rasterizing them to an output texture.
    /// </summary>
    public class SdfGenerator
    {
        internal const double DEFAULT_ANGLE_THRESHOLD = 3;
        internal const double MSDFGEN_CORNER_DOT_EPSILON = 0.000001;
        internal const double MSDFGEN_DECONVERGENCE_FACTOR = 0.000001;

        public unsafe TextureSliceRef<float> Generate(uint pWidth, uint pHeight, Shape shape, MsdfProjection projection, double pxRange, SdfMode mode, FillRule fl)
        {
            if (pWidth == 0 || pHeight == 0)
                throw new Exception("Texture slice width and height must be at least 1 pixel");

            SdfConfig config = new SdfConfig()
            {
                DistanceCheckMode = SdfConfig.DistanceErrorCheckMode.CHECK_DISTANCE_AT_EDGE,
                Mode = SdfConfig.ErrorCorrectMode.EDGE_PRIORITY
            };

            double range = pxRange / Math.Min(projection.Scale.X, projection.Scale.Y);
            uint nPerPixel = GetNPerPixel(mode);
            const string edgeAssignment = null; // "cmywCMYW";
            SdfConfig postGenConfig = new SdfConfig(config);

            config.Mode = SdfConfig.ErrorCorrectMode.DISABLED;
            postGenConfig.DistanceCheckMode = SdfConfig.DistanceErrorCheckMode.DO_NOT_CHECK_DISTANCE;

            uint numBytes = pWidth * pHeight * nPerPixel * sizeof(float);
            TextureSlice sdf = new TextureSlice(pWidth, pHeight, numBytes)
            {
                ElementsPerPixel = nPerPixel,
            };

            TextureSliceRef<float> sdfRef = sdf.GetReference<float>();

            switch (mode)
            {
                case SdfMode.Sdf:
                    GenerateSDF(sdfRef, shape, projection, range);
                    break;

                case SdfMode.Psdf:
                    GeneratePseudoSDF(sdfRef, shape, projection, range);
                    break;

                case SdfMode.Msdf:
                    EdgeColouring.edgeColoringSimple(shape, DEFAULT_ANGLE_THRESHOLD, 0);
                    EdgeColouring.parseColoring(shape, edgeAssignment);
                    GenerateMSDF(sdfRef, shape, projection, range, config);
                    break;

                case SdfMode.Mtsdf:
                    EdgeColouring.edgeColoringSimple(shape, DEFAULT_ANGLE_THRESHOLD, 0);
                    EdgeColouring.parseColoring(shape, edgeAssignment);
                    GenerateMTSDF(sdfRef, shape, projection, range, config);
                    break;
            }

            // Error correction
            switch (mode)
            {
                case SdfMode.Sdf:
                case SdfMode.Psdf:
                    ErrorCorrection.DistanceSignCorrection(sdfRef, shape, projection, fl);
                    break;

                case SdfMode.Msdf:
                    {
                        ErrorCorrection.MultiDistanceSignCorrection(sdfRef, shape, projection, fl);
                        ErrorCorrection.MsdfErrorCorrection<MultiDistanceSelector, MultiDistance>(sdfRef, shape, projection, range, postGenConfig);
                        break;
                    }

                case SdfMode.Mtsdf:
                    {
                        ErrorCorrection.MultiDistanceSignCorrection(sdfRef, shape, projection, fl);
                        ErrorCorrection.MsdfErrorCorrection<MultiAndTrueDistanceSelector, MultiAndTrueDistance>(sdfRef, shape, projection, range, postGenConfig);
                        break;
                    }
            }

            return sdfRef;
        }

        public unsafe ITexture2D ConvertToTexture(RenderService renderer, TextureSliceRef<float> src)
        {
            uint rowPitch = (src.Width * (uint)sizeof(Color));
            Color[] finalData = new Color[src.Width * src.Height];
            ITexture2D tex = renderer.Resources.CreateTexture2D(new Texture2DProperties()
            {
                Width = src.Width,
                Height = src.Height,
                Format = GraphicsFormat.R8G8B8A8_UNorm
            });

            switch (src.ElementsPerPixel)
            {
                case 1: // SDF or PSDF is one-channel. The render result of all SDF modes are also generally greyscale/white/black.
                    for (int i = 0; i < finalData.Length; i++)
                    {
                        byte c = (byte)(255 * src[i]);
                        finalData[i] = new Color()
                        {
                            R = c,
                            G = c,
                            B = c,
                            A = c,
                        };
                    }
                    break;

                case 3: // MSDF - 3 RGB 32-bit
                    for (uint i = 0; i < finalData.Length; i++)
                    {
                        uint p = i * src.ElementsPerPixel;

                        finalData[i] = new Color()
                        {
                            R = (byte)(255 * src[p]),
                            G = (byte)(255 * src[p + 1]),
                            B = (byte)(255 * src[p + 2]),
                            A = 255,
                        };
                    }

                    break;

                case 4: // MTSDF - 3 RGB 32-bit
                    for (uint i = 0; i < finalData.Length; i++)
                    {
                        uint p = i * src.ElementsPerPixel;

                        finalData[i] = new Color()
                        {
                            R = (byte)(255 * src[p]),
                            G = (byte)(255 * src[p + 1]),
                            B = (byte)(255 * src[p + 2]),
                            A = (byte)(255 * src[p + 3]),
                        };
                    }

                    break;
            }

            tex.SetData(0, finalData, 0, (uint)finalData.Length, rowPitch);
            return tex;
        }

        private uint GetNPerPixel(SdfMode mode)
        {
            // Get elements per pixel
            switch (mode)
            {
                default:
                case SdfMode.Psdf:
                case SdfMode.Sdf:
                    return 1;

                case SdfMode.Msdf:
                    return 3;

                case SdfMode.Mtsdf:
                    return 4;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="ES">Edge selector type</typeparam>
        /// <typeparam name="DT">Distance Type</typeparam>
        /// <typeparam name="EC">Edge cache Type</typeparam>
        /// <param name="output"></param>
        /// <param name="shape"></param>
        /// <param name="projection"></param>
        private unsafe void GenerateDistanceField<ES, DT>(DistancePixelConversion<DT> distancePixelConvertor,
            TextureSliceRef<float> output, Shape shape, MsdfProjection projection, double range)
            where ES : EdgeSelector<DT>, new()
            where DT : unmanaged
        {
            ShapeDistanceFinder<ES, DT> distanceFinder = new ShapeDistanceFinder<ES, DT>(shape);

            bool rightToLeft = false;
            for (int y = 0; y < output.Height; ++y)
            {
                for (int col = 0; col < output.Width; ++col)
                {
                    int x = (int)(rightToLeft ? output.Width - col - 1 : col);
                    Vector2D p = projection.Unproject(new Vector2D(x + .5, y + .5));
                    DT distance = distanceFinder.distance(ref p);
                    distancePixelConvertor.Convert(output[x, y], distance);
                }
                rightToLeft = !rightToLeft;
            }
        }

        private void GenerateSDF(TextureSliceRef<float> output, Shape shape, MsdfProjection projection, double range)
        {
            NPerPixel(output, 1);

            var dpc = new DoubleDistancePixelConversion(range);
            GenerateDistanceField<TrueDistanceSelector, double>(dpc, output, shape, projection, range);
        }

        private void GeneratePseudoSDF(TextureSliceRef<float> output, Shape shape, MsdfProjection projection, double range)
        {
            NPerPixel(output, 1);

            var dpc = new DoubleDistancePixelConversion(range);
            GenerateDistanceField<PseudoDistanceSelector, double>(dpc, output, shape, projection, range);
        }

        private void GenerateMSDF(TextureSliceRef<float> output, Shape shape, MsdfProjection projection, double range, SdfConfig config)
        {
            NPerPixel(output, 3);

            var dpc = new MultiDistancePixelConversion(range);

            GenerateDistanceField<MultiDistanceSelector, MultiDistance>(dpc, output, shape, projection, range);
            ErrorCorrection.MsdfErrorCorrection<MultiDistanceSelector, MultiDistance>(output, shape, projection, range, config);
        }

        private void GenerateMTSDF(TextureSliceRef<float> output, Shape shape, MsdfProjection projection, double range, SdfConfig config)
        {
            NPerPixel(output, 4);

            var dpc = new MultiTrueDistancePixelConversion(range);
            GenerateDistanceField<MultiAndTrueDistanceSelector, MultiAndTrueDistance>(dpc, output, shape, projection, range);
            ErrorCorrection.MsdfErrorCorrection<MultiAndTrueDistanceSelector, MultiAndTrueDistance>(output, shape, projection, range, config);
        }

        internal static void NPerPixel<T>(TextureSliceRef<T> bitmap, int expectedN) where T : unmanaged
        {
            if (bitmap.ElementsPerPixel != expectedN)
                throw new IndexOutOfRangeException($"A {nameof(TextureSliceRef<T>)} of {expectedN} component{(expectedN > 1 ? "s" : "")}-per-pixel is expected, not {bitmap.ElementsPerPixel}.");
        }

        internal static void NPerPixel<T>(TextureSlice bitmap, int expectedN) where T : unmanaged
        {
            if (bitmap.ElementsPerPixel != expectedN)
                throw new IndexOutOfRangeException($"A {nameof(TextureSliceRef<T>)} of {expectedN} component{(expectedN > 1 ? "s" : "")}-per-pixel is expected, not {bitmap.ElementsPerPixel}.");
        }

        internal static void DeconvergeCubicEdge(Shape.CubicEdge edge, int param, double amount)
        {
            Vector2D dir = edge.GetDirection(param);
            Vector2D normal = dir.GetOrthonormal();
            double h = Vector2D.Dot(edge.GetDirectionChange(param) - dir, normal);
            switch (param)
            {
                case 0:
                    edge.p[1] += amount * (dir + Math.Sign(h) * Math.Sqrt(Math.Abs(h)) * normal);
                    break;
                case 1:
                    edge.p[2] -= amount * (dir - Math.Sign(h) * Math.Sqrt(Math.Abs(h)) * normal);
                    break;
            }
        }

        internal static void DeconvergeEdge(Shape.Edge edge, int param)
        {
            {
                if (edge is Shape.QuadraticEdge quadratic)
                    edge = quadratic.ConvertToCubic();
            }
            {
                if (edge is Shape.CubicEdge cubic)
                    DeconvergeCubicEdge(cubic, param, MSDFGEN_DECONVERGENCE_FACTOR);
            }
        }

        /// <summary>
        /// Normalizes the shape geometry for distance field generation.
        /// </summary>
        public void Normalize(Shape shape)
        {
            foreach (Shape.Contour contour in shape.Contours)
            {
                if (contour.Edges.Count == 1)
                {
                    Shape.Edge[] parts = new Shape.Edge[3];

                    contour.Edges[0].SplitInThirds(ref parts[0], ref parts[1], ref parts[2]);
                    contour.Edges.Clear();
                    contour.Edges.Add(parts[0]);
                    contour.Edges.Add(parts[1]);
                    contour.Edges.Add(parts[2]);
                }
                else
                {
                    Shape.Edge prevEdge = contour.Edges.Last();

                    foreach (Shape.Edge edge in contour.Edges)
                    {
                        Vector2D prevDir = prevEdge.GetDirection(1).GetNormalized();
                        Vector2D curDir = edge.GetDirection(0).GetNormalized();

                        if (Vector2D.Dot(prevDir, curDir) < SdfGenerator.MSDFGEN_CORNER_DOT_EPSILON - 1)
                        {
                            SdfGenerator.DeconvergeEdge(prevEdge, 1);
                            SdfGenerator.DeconvergeEdge(edge, 0);
                        }

                        prevEdge = edge;
                    }
                }
            }
        }

        public unsafe void Simulate8bit(TextureSliceRef<float> bitmap)
        {
            float* end = bitmap.Data + bitmap.ElementsPerPixel * bitmap.Width * bitmap.Height;
            for (float* p = bitmap.Data; p < end; ++p)
                *p = PixelByteToFloat(PixelFloatToByte(*p));
        }

        private static byte PixelFloatToByte(float x)
        {
            return (byte)(MathHelper.Clamp(256f * x, 0, 255f));
        }

        private static float PixelByteToFloat(byte x)
        {
            return 1f / 255f * x;
        }
    }
}
