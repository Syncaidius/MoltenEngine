using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
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

    /* NOTES *******
     * TODO Replace N generic of BitmapRef<T, int N>, BitmapConstRef<T, int N> and Bitmap<T, int N>. Store N as a property on these classes instead.
     */
    public class MsdfGenerator
    {
        const double DEFAULT_ANGLE_THRESHOLD = 3;

        public unsafe void Generate(TextureSliceRef<float> output, Shape shape, MsdfProjection projection, double range, MsdfConfig config, SdfMode mode, FillRule fl)
        {
            const string edgeAssignment = null; // "cmywCMYW";

            switch (mode)
            {
                case SdfMode.Sdf:
                    GenerateSDF(output, shape, projection, range);
                    break;

                case SdfMode.Psdf:
                    GeneratePseudoSDF(output, shape, projection, range);
                    break;

                case SdfMode.Msdf:
                    EdgeColouring.edgeColoringSimple(shape, DEFAULT_ANGLE_THRESHOLD, 0);
                    EdgeColouring.parseColoring(shape, edgeAssignment);
                    GenerateMSDF(output, shape, projection, range, config);
                    break;

                case SdfMode.Mtsdf:
                    EdgeColouring.edgeColoringSimple(shape, DEFAULT_ANGLE_THRESHOLD, 0);
                    EdgeColouring.parseColoring(shape, edgeAssignment);
                    GenerateMTSDF(output, shape, projection, range, config);
                    break;
            }

            // Error correction
            switch (mode)
            {
                case SdfMode.Sdf:
                case SdfMode.Psdf:
                    MsdfRasterization.distanceSignCorrection(output, shape, projection, fl);
                    break;

                case SdfMode.Msdf:
                    {
                        var combiner = new ContourCombiner<MultiDistanceSelector, MultiDistance>(shape);
                        MsdfRasterization.multiDistanceSignCorrection(output, shape, projection, fl);
                        ErrorCorrection.MsdfErrorCorrection(combiner, output, shape, projection, range, config);
                        break;
                    }

                case SdfMode.Mtsdf:
                    {
                        var combiner = new ContourCombiner<MultiAndTrueDistanceSelector, MultiAndTrueDistance>(shape);
                        MsdfRasterization.multiDistanceSignCorrection(output, shape, projection, fl);
                        ErrorCorrection.MsdfErrorCorrection(combiner, output, shape, projection, range, config);
                        break;
                    }
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
        private unsafe void GenerateDistanceField<ES, DT>(DistancePixelConversion<DT> distancePixelConvertor, ContourCombiner<ES, DT> combiner,
            TextureSliceRef<float> output, Shape shape, MsdfProjection projection, double range)
            where ES : EdgeSelector<DT>, new()
            where DT : unmanaged
        {
            ShapeDistanceFinder<ES, DT> distanceFinder = new ShapeDistanceFinder<ES, DT>(shape, combiner);

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
            var combiner = new ContourCombiner<TrueDistanceSelector, double>(shape);

            GenerateDistanceField(dpc, combiner, output, shape, projection, range);
        }

        private void GeneratePseudoSDF(TextureSliceRef<float> output, Shape shape, MsdfProjection projection, double range)
        {
            NPerPixel(output, 1);

            var dpc = new DoubleDistancePixelConversion(range);
            var combiner = new ContourCombiner<PseudoDistanceSelector, double>(shape);

            GenerateDistanceField(dpc, combiner, output, shape, projection, range);
        }

        private void GenerateMSDF(TextureSliceRef<float> output, Shape shape, MsdfProjection projection, double range, MsdfConfig config)
        {
            NPerPixel(output, 3);

            var dpc = new MultiDistancePixelConversion(range);
            var combiner = new ContourCombiner<MultiDistanceSelector, MultiDistance>(shape);

            GenerateDistanceField(dpc, combiner, output, shape, projection, range);
            ErrorCorrection.MsdfErrorCorrection(combiner, output, shape, projection, range, config);
        }

        private void GenerateMTSDF(TextureSliceRef<float> output, Shape shape, MsdfProjection projection, double range, MsdfConfig config)
        {
            NPerPixel(output, 4);

            var dpc = new MultiTrueDistancePixelConversion(range);
            var combiner = new ContourCombiner<MultiAndTrueDistanceSelector, MultiAndTrueDistance>(shape);

            GenerateDistanceField(dpc, combiner, output, shape, projection, range);
            ErrorCorrection.MsdfErrorCorrection(combiner, output, shape, projection, range, config);
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
