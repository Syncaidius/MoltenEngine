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
        const double DEFAULT_ANGLE_THRESHOLD = 3;

        public unsafe TextureSliceRef<float> Generate(uint pWidth, uint pHeight, Shape shape, MsdfProjection projection, double pxRange, SdfMode mode, FillRule fl)
        {
            if (pWidth == 0 || pHeight == 0)
                throw new Exception("Texture slice width and height must be at least 1 pixel");

            MsdfConfig config = new MsdfConfig()
            {
                DistanceCheckMode = MsdfConfig.DistanceErrorCheckMode.CHECK_DISTANCE_AT_EDGE,
                Mode = MsdfConfig.ErrorCorrectMode.EDGE_PRIORITY
            };

            double range = pxRange / Math.Min(projection.Scale.X, projection.Scale.Y);
            uint nPerPixel = GetNPerPixel(mode);
            const string edgeAssignment = null; // "cmywCMYW";
            MsdfConfig postGenConfig = new MsdfConfig(config);

            config.Mode = MsdfConfig.ErrorCorrectMode.DISABLED;
            postGenConfig.DistanceCheckMode = MsdfConfig.DistanceErrorCheckMode.DO_NOT_CHECK_DISTANCE;

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
                    MsdfRasterization.DistanceSignCorrection(sdfRef, shape, projection, fl);
                    break;

                case SdfMode.Msdf:
                    {
                        MsdfRasterization.MultiDistanceSignCorrection(sdfRef, shape, projection, fl);
                        ErrorCorrection.MsdfErrorCorrection<MultiDistanceSelector, MultiDistance>(sdfRef, shape, projection, range, postGenConfig);
                        break;
                    }

                case SdfMode.Mtsdf:
                    {
                        MsdfRasterization.MultiDistanceSignCorrection(sdfRef, shape, projection, fl);
                        ErrorCorrection.MsdfErrorCorrection<MultiAndTrueDistanceSelector, MultiAndTrueDistance>(sdfRef, shape, projection, range, postGenConfig);
                        break;
                    }
            }

            return sdfRef;
        }

        public unsafe TextureSliceRef<float> Rasterize(uint width, uint height, TextureSliceRef<float> sdf, MsdfProjection projection, double pxRange)
        {
            if (width == 0 || height == 0)
                throw new Exception("Width and height must be at least 1 pixel");

            MsdfRasterization.Simulate8bit(sdf);

            uint numBytes = width * height * sizeof(float);
            TextureSlice output = new TextureSlice(width, height, numBytes)
            {
                ElementsPerPixel = 1,
            };

            TextureSliceRef<float> outRef = output.GetReference<float>();

            double range = pxRange / Math.Min(projection.Scale.X, projection.Scale.Y);
            double avgScale = (projection.Scale.X + projection.Scale.Y) / 2;
            MsdfRasterization.RenderSDF(outRef, sdf, avgScale * range, 0.5f);

            return outRef;
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

        private void GenerateMSDF(TextureSliceRef<float> output, Shape shape, MsdfProjection projection, double range, MsdfConfig config)
        {
            NPerPixel(output, 3);

            var dpc = new MultiDistancePixelConversion(range);

            GenerateDistanceField<MultiDistanceSelector, MultiDistance>(dpc, output, shape, projection, range);
            ErrorCorrection.MsdfErrorCorrection<MultiDistanceSelector, MultiDistance>(output, shape, projection, range, config);
        }

        private void GenerateMTSDF(TextureSliceRef<float> output, Shape shape, MsdfProjection projection, double range, MsdfConfig config)
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
    }
}
