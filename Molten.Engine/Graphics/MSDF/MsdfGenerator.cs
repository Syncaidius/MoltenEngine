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
        public unsafe void Generate(TextureSliceRef<float> output, MsdfShape shape, MsdfProjection projection, double range, MsdfConfig config, SdfMode mode, FillRule fl, bool legacy)
        {
            if (legacy)
            {
                switch (mode)
                {
                    case SdfMode.Sdf:
                        GenerateSDF_Legacy(output, shape, range, projection.Scale, projection.Translate);
                        break;

                    case SdfMode.Psdf:
                        GenerateSDF_Legacy(output, shape, range, projection.Scale, projection.Translate);
                        break;

                    case SdfMode.Msdf:
                        GenerateMSDF_Legacy(output, shape, range, projection.Scale, projection.Translate, config);
                        break;

                    case SdfMode.Mtsdf:
                        GenerateMTSDF_Legacy(output, shape, range, projection.Scale, projection.Translate, config);
                        break;
                }
            }
            else
            {
                switch (mode)
                {
                    case SdfMode.Sdf:
                        GenerateSDF(output, shape, projection, range);
                        break;

                    case SdfMode.Psdf: 
                        GeneratePseudoSDF(output, shape, projection, range); 
                        break;

                    case SdfMode.Msdf: 
                        GenerateMSDF(output, shape, projection, range, config); 
                        break;

                    case SdfMode.Mtsdf: 
                        GenerateMTSDF(output, shape, projection, range, config); 
                        break;
                }
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
            TextureSliceRef<float> output, MsdfShape shape, MsdfProjection projection, double range)
            where ES : EdgeSelector<DT>, new()
            where DT : unmanaged
        {
            ShapeDistanceFinder<ES, DT> distanceFinder = new ShapeDistanceFinder<ES, DT>(shape, combiner);

            bool rightToLeft = false;
            for (int y = 0; y < output.Height; ++y)
            {
                int row = (int)(shape.InverseYAxis ? output.Height - y - 1 : y);
                for (int col = 0; col < output.Width; ++col)
                {
                    int x = (int)(rightToLeft ? output.Width - col - 1 : col);
                    Vector2D p = projection.Unproject(new Vector2D(x + .5, y + .5));
                    DT distance = distanceFinder.distance(ref p);
                    distancePixelConvertor.Convert(output[x,row], distance);
                }
                rightToLeft = !rightToLeft;
            }
        }

        private void GenerateSDF(TextureSliceRef<float> output, MsdfShape shape, MsdfProjection projection, double range)
        {
            Validation.NPerPixel(output, 1);

            var dpc = new DoubleDistancePixelConversion(range);
            var combiner = new ContourCombiner<TrueDistanceSelector, double>(shape);

            GenerateDistanceField(dpc, combiner, output, shape, projection, range);
        }

        private void GeneratePseudoSDF(TextureSliceRef<float> output, MsdfShape shape, MsdfProjection projection, double range)
        {
            Validation.NPerPixel(output, 1);

            var dpc = new DoubleDistancePixelConversion(range);
            var combiner = new ContourCombiner<PseudoDistanceSelector, double>(shape);

            GenerateDistanceField(dpc, combiner, output, shape, projection, range);
        }

        private void GenerateMSDF(TextureSliceRef<float> output, MsdfShape shape, MsdfProjection projection, double range, MsdfConfig config)
        {
            Validation.NPerPixel(output, 3);

            var dpc = new MultiDistancePixelConversion(range);
            var combiner = new ContourCombiner<MultiDistanceSelector, MultiDistance>(shape);

            GenerateDistanceField(dpc, combiner, output, shape, projection, range);
            ErrorCorrection.MsdfErrorCorrection(combiner, output, shape, projection, range, config);
        }

        private void GenerateMTSDF(TextureSliceRef<float> output, MsdfShape shape, MsdfProjection projection, double range, MsdfConfig config)
        {
            Validation.NPerPixel(output, 4);

            var dpc = new MultiTrueDistancePixelConversion(range);
            var combiner = new ContourCombiner<MultiAndTrueDistanceSelector, MultiAndTrueDistance>(shape);

            GenerateDistanceField(dpc, combiner, output, shape, projection, range);
            ErrorCorrection.MsdfErrorCorrection(combiner, output, shape, projection, range, config);
        }

        // Legacy version
        private unsafe void GenerateSDF_Legacy(TextureSliceRef<float> output, MsdfShape shape, double range, Vector2D scale, Vector2D translate)
        {
            if (output.ElementsPerPixel != 1)
                throw new IndexOutOfRangeException("A BitmapRef of 1 component-per-pixel is expected");

            for (int y = 0; y < output.Height; ++y)
            {
                int row = (int)(shape.InverseYAxis ? output.Height - y - 1 : y);
                for (int x = 0; x < output.Width; ++x)
                {
                    double dummy;
                    Vector2D p = new Vector2D(x + .5, y + .5) / scale - translate;
                    SignedDistance minDistance = new SignedDistance();
                    foreach (Contour contour in shape.Contours)
                    {
                        foreach (EdgeSegment edge in contour.Edges)
                        {
                            SignedDistance distance = edge.SignedDistance(p, out dummy);
                            if (distance < minDistance)
                                minDistance = distance;
                        } 
                    }

                    *output[x, row] = (float)(minDistance.Distance / range + .5);
                }
            }
        }

        private unsafe void GeneratePseudoSDF_Legacy(TextureSliceRef<float> output, MsdfShape shape, double range, Vector2D scale, Vector2D translate)
        {
            if (output.ElementsPerPixel != 1)
                throw new IndexOutOfRangeException("A BitmapRef of 1 component-per-pixel is expected");

            for (int y = 0; y < output.Height; ++y)
            {
                int row = (int)(shape.InverseYAxis ? output.Height - y - 1 : y);
                for (int x = 0; x < output.Width; ++x)
                {
                    Vector2D p = new Vector2D(x + .5, y + .5) / scale - translate;
                    SignedDistance minDistance = new SignedDistance();
                    EdgeSegment nearEdge = null;
                    double nearParam = 0;
                    foreach (Contour contour in shape.Contours)
                    {
                        foreach (EdgeSegment edge in contour.Edges)
                        {
                            double param;
                            SignedDistance distance = edge.SignedDistance(p, out param);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                nearEdge = edge; // TODO do we clone here?
                                nearParam = param;
                            }
                        }
                    }

                    if (nearEdge != null)
                        nearEdge.DistanceToPseudoDistance(ref minDistance, p, nearParam);

                    *output[x, row] = (float)(minDistance.Distance / range + .5);
                }
            }
        }

        private unsafe void GenerateMSDF_Legacy(TextureSliceRef<float> output, MsdfShape shape, double range, Vector2D scale, Vector2D translate, MsdfConfig config)
        {
            if (output.ElementsPerPixel != 3)
                throw new IndexOutOfRangeException("A BitmapRef of 3 component-per-pixel is expected");

            for (int y = 0; y < output.Height; ++y)
            {
                int row = (int)(shape.InverseYAxis ? output.Height - y - 1 : y);
                for (int x = 0; x < output.Width; ++x)
                {
                    Vector2D p = new Vector2D(x + .5, y + .5) / scale - translate;

                    EdgeParams r, g, b;
                    r.minDistance = g.minDistance = b.minDistance = new SignedDistance();
                    r.nearEdge = g.nearEdge = b.nearEdge = null;
                    r.nearParam = g.nearParam = b.nearParam = 0;

                    foreach (Contour contour in shape.Contours)
                    {
                        foreach (EdgeSegment edge in contour.Edges)
                        {
                            double param;
                            SignedDistance distance = edge.SignedDistance(p, out param);
                            if ((edge.Color & EdgeColor.Red) == EdgeColor.Red && distance < r.minDistance)
                            {
                                r.minDistance = distance;
                                r.nearEdge = edge; // TODO clone here?
                                r.nearParam = param;
                            }

                            if ((edge.Color & EdgeColor.Green) == EdgeColor.Green && distance < g.minDistance)
                            {
                                g.minDistance = distance;
                                g.nearEdge = edge; // TODO clone here?
                                g.nearParam = param;
                            }

                            if ((edge.Color & EdgeColor.Blue) == EdgeColor.Blue && distance < b.minDistance)
                            {
                                b.minDistance = distance;
                                b.nearEdge = edge; // TODO clone here?
                                b.nearParam = param;
                            }
                        }
                    }

                    if (r.nearEdge != null)
                        r.nearEdge.DistanceToPseudoDistance(ref r.minDistance, p, r.nearParam);

                    if (g.nearEdge != null)
                        g.nearEdge.DistanceToPseudoDistance(ref g.minDistance, p, g.nearParam);

                    if (b.nearEdge != null)
                        b.nearEdge.DistanceToPseudoDistance(ref b.minDistance, p, b.nearParam);

                    output[x, row][0] = (float)(r.minDistance.Distance / range + .5);
                    output[x, row][1] = (float)(g.minDistance.Distance / range + .5);
                    output[x, row][2] = (float)(b.minDistance.Distance / range + .5);
                }
            }

            config.DistanceCheckMode = MsdfConfig.DistanceErrorCheckMode.DO_NOT_CHECK_DISTANCE;
            var combiner = new ContourCombiner<MultiDistanceSelector, MultiDistance>(shape);
            ErrorCorrection.MsdfErrorCorrection(combiner, output, shape, new MsdfProjection(scale, translate), range, config);
        }

        private unsafe void GenerateMTSDF_Legacy(TextureSliceRef<float> output, MsdfShape shape, double range, Vector2D scale, Vector2D translate, MsdfConfig config)
        {
            if (output.ElementsPerPixel != 4)
                throw new IndexOutOfRangeException("A BitmapRef of 4 components-per-pixel is expected");

            for (int y = 0; y < output.Height; ++y)
            {
                int row = (int)(shape.InverseYAxis ? output.Height - y - 1 : y);
                for (int x = 0; x < output.Width; ++x)
                {
                    Vector2D p = new Vector2D(x + .5, y + .5) / scale - translate;

                    SignedDistance minDistance = new SignedDistance();
                    EdgeParams r, g, b;
                    r.minDistance = g.minDistance = b.minDistance = new SignedDistance();
                    r.nearEdge = g.nearEdge = b.nearEdge = null;
                    r.nearParam = g.nearParam = b.nearParam = 0;

                    foreach (Contour contour in shape.Contours)
                    {
                        foreach (EdgeSegment edge in contour.Edges)
                        {
                            double param = 0;
                            SignedDistance distance = edge.SignedDistance(p, out param);
                            if (distance < minDistance)
                                minDistance = distance;

                            if ((edge.Color & EdgeColor.Red) == EdgeColor.Red && distance < r.minDistance)
                            {
                                r.minDistance = distance;
                                r.nearEdge = edge; // TODO .clone() here?
                                r.nearParam = param;
                            }

                            if ((edge.Color & EdgeColor.Green) == EdgeColor.Green && distance < g.minDistance)
                            {
                                g.minDistance = distance;
                                g.nearEdge = edge; // TODO clone here?
                                g.nearParam = param;
                            }

                            if ((edge.Color & EdgeColor.Blue) == EdgeColor.Blue && distance < b.minDistance)
                            {
                                b.minDistance = distance;
                                b.nearEdge = edge; // TODO clone here?
                                b.nearParam = param;
                            }
                        }

                        if (r.nearEdge != null)
                            r.nearEdge.DistanceToPseudoDistance(ref r.minDistance, p, r.nearParam);
                        if (g.nearEdge != null)
                            g.nearEdge.DistanceToPseudoDistance(ref g.minDistance, p, g.nearParam);
                        if (b.nearEdge != null)
                            b.nearEdge.DistanceToPseudoDistance(ref b.minDistance, p, b.nearParam);

                        output[x, row][0] = (float)(r.minDistance.Distance / range + .5);
                        output[x, row][1] = (float)(g.minDistance.Distance / range + .5);
                        output[x, row][2] = (float)(b.minDistance.Distance / range + .5);
                        output[x, row][3] = (float)(minDistance.Distance / range + .5);
                    }
                }

                config.DistanceCheckMode = MsdfConfig.DistanceErrorCheckMode.DO_NOT_CHECK_DISTANCE;
                var combiner = new ContourCombiner<MultiAndTrueDistanceSelector, MultiAndTrueDistance>(shape);
                ErrorCorrection.MsdfErrorCorrection(combiner, output, shape, new MsdfProjection(scale, translate), range, config);
            }
        }
    }
}
