using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    /* NOTES *******
     * TODO Replace N generic of BitmapRef<T, int N>, BitmapConstRef<T, int N> and Bitmap<T, int N>. Store N as a property on these classes instead.
     */
    public class MsdfGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="ES">Edge selector type</typeparam>
        /// <typeparam name="DT">Distance Type</typeparam>
        /// <typeparam name="EC">Edge cache Type</typeparam>
        /// <param name="output"></param>
        /// <param name="shape"></param>
        /// <param name="projection"></param>
        private unsafe void generateDistanceField<ES, DT, EC>(DistancePixelConversion<DT> distancePixelConversion, ContourCombiner<ES, DT, EC> combiner, BitmapRef<float> output, MsdfShape shape, MsdfProjection projection, double range)
            where ES : EdgeSelector<DT, EC>, new()
            where DT : unmanaged
            where EC : unmanaged
        {
            ShapeDistanceFinder<ES, DT, EC> distanceFinder = new ShapeDistanceFinder<ES, DT, EC>(shape, combiner);

            bool rightToLeft = false;
            for (int y = 0; y < output.Height; ++y)
            {
                int row = shape.InverseYAxis ? output.Height - y - 1 : y;
                for (int col = 0; col < output.Width; ++col)
                {
                    int x = rightToLeft ? output.Width - col - 1 : col;
                    Vector2D p = projection.Unproject(new Vector2D(x + .5, y + .5));
                    DT distance = distanceFinder.distance(p);
                    distancePixelConversion.Convert(output[x,row], distance);
                }
                rightToLeft = !rightToLeft;
            }
        }

        public void generateSDF(BitmapRef<float> output, MsdfShape shape, MsdfProjection projection, double range, GeneratorConfig config)
        {
            Validation.NPerPixel(output, 1);
            var dpc = new DoubleDistancePixelConversion(range);

            if (config.OverlapSupport)
            {
                var combiner = new OverlappingContourCombiner<TrueDistanceSelector, double, TrueDistanceSelector.EdgeCache>(shape);
                generateDistanceField(dpc, combiner, output, shape, projection, range);
            }
            else
            {
                var combiner = new SimpleContourCombiner<TrueDistanceSelector, double, TrueDistanceSelector.EdgeCache>(shape);
                generateDistanceField(dpc, combiner, output, shape, projection, range);
            }
        }

        public void generatePseudoSDF(BitmapRef<float> output, MsdfShape shape, MsdfProjection projection, double range, GeneratorConfig config)
        {
            Validation.NPerPixel(output, 1);
            var dpc = new DoubleDistancePixelConversion(range);

            if (config.OverlapSupport)
            {
                var combiner = new OverlappingContourCombiner<PseudoDistanceSelector, double, PseudoDistanceSelectorBase.EdgeCache>(shape);
                generateDistanceField(dpc, combiner, output, shape, projection, range);
            }
            else
            {
                var combiner = new SimpleContourCombiner<PseudoDistanceSelector, double, PseudoDistanceSelectorBase.EdgeCache>(shape);
                generateDistanceField(dpc, combiner, output, shape, projection, range);
            }
        }

        public void generateMSDF(BitmapRef<float> output, MsdfShape shape, MsdfProjection projection, double range, MSDFGeneratorConfig config)
        {
            Validation.NPerPixel(output, 3);
            var dpc = new MultiDistancePixelConversion(range);

            if (config.OverlapSupport)
            {
                var combiner = new OverlappingContourCombiner<MultiDistanceSelector, MultiDistance, PseudoDistanceSelectorBase.EdgeCache>(shape);
                generateDistanceField(dpc, combiner, output, shape, projection, range);
                ErrorCorrection.msdfErrorCorrection(combiner, output, shape, projection, range, config);
            }
            else
            {
                var combiner = new SimpleContourCombiner<MultiDistanceSelector, MultiDistance, PseudoDistanceSelectorBase.EdgeCache>(shape);
                generateDistanceField(dpc, combiner, output, shape, projection, range);
                ErrorCorrection.msdfErrorCorrection(combiner, output, shape, projection, range, config);
            }
        }

        public void generateMTSDF(BitmapRef<float> output, MsdfShape shape, MsdfProjection projection, double range, MSDFGeneratorConfig config)
        {
            Validation.NPerPixel(output, 4);
            var dpc = new MultiTrueDistancePixelConversion(range);
            if (config.OverlapSupport)
            {
                var combiner = new OverlappingContourCombiner<MultiAndTrueDistanceSelector, MultiAndTrueDistance, PseudoDistanceSelectorBase.EdgeCache>(shape);
                generateDistanceField(dpc, combiner, output, shape, projection, range);
                ErrorCorrection.msdfErrorCorrection(combiner, output, shape, projection, range, config);
            }
            else
            {
                var combiner = new SimpleContourCombiner<MultiAndTrueDistanceSelector, MultiAndTrueDistance, PseudoDistanceSelectorBase.EdgeCache>(shape);
                generateDistanceField(dpc, combiner, output, shape, projection, range);
                ErrorCorrection.msdfErrorCorrection(combiner, output, shape, projection, range, config);
            }
        }

        // Legacy API

        public void generateSDF(BitmapRef<float> output, MsdfShape shape, double range, Vector2D scale, Vector2D translate, bool overlapSupport)
        {
            generateSDF(output, shape, new MsdfProjection(scale, translate), range, new GeneratorConfig(overlapSupport));
        }

        public void generatePseudoSDF(BitmapRef<float> output, MsdfShape shape, double range, Vector2D scale, Vector2D translate, bool overlapSupport)
        {
            generatePseudoSDF(output, shape, new MsdfProjection(scale, translate), range, new GeneratorConfig(overlapSupport));
        }

        public void generateMSDF(BitmapRef<float> output, MsdfShape shape, double range, Vector2D scale, Vector2D translate, ErrorCorrectionConfig errorCorrectionConfig, bool overlapSupport)
        {
            generateMSDF(output, shape, new MsdfProjection(scale, translate), range, new MSDFGeneratorConfig(overlapSupport, errorCorrectionConfig));
        }

        void generateMTSDF(BitmapRef<float> output, MsdfShape shape, double range, Vector2D scale, Vector2D translate, ErrorCorrectionConfig errorCorrectionConfig, bool overlapSupport)
        {
            generateMTSDF(output, shape, new MsdfProjection(scale, translate), range, new MSDFGeneratorConfig(overlapSupport, errorCorrectionConfig));
        }

        // Legacy version
        public unsafe void generateSDF_legacy(BitmapRef<float> output, MsdfShape shape, double range, Vector2D scale, Vector2D translate)
        {
            if (output.NPerPixel != 1)
                throw new IndexOutOfRangeException("A BitmapRef of 1 component-per-pixel is expected");

            for (int y = 0; y < output.Height; ++y)
            {
                int row = shape.InverseYAxis ? output.Height - y - 1 : y;
                for (int x = 0; x < output.Width; ++x)
                {
                    double dummy;
                    Vector2D p = new Vector2D(x + .5, y + .5) / scale - translate;
                    SignedDistance minDistance = new SignedDistance();
                    foreach (Contour contour in shape.Contours)
                    {
                        foreach (EdgeSegment edge in contour.Edges)
                        {
                            SignedDistance distance = edge.signedDistance(p, out dummy);
                            if (distance < minDistance)
                                minDistance = distance;
                        } 
                    }

                    *output[x, row] = (float)(minDistance.Distance / range + .5);
                }
            }
        }

        public unsafe void generatePseudoSDF_legacy(BitmapRef<float> output, MsdfShape shape, double range, Vector2D scale, Vector2D translate)
        {
            if (output.NPerPixel != 1)
                throw new IndexOutOfRangeException("A BitmapRef of 1 component-per-pixel is expected");

            for (int y = 0; y < output.Height; ++y)
            {
                int row = shape.InverseYAxis ? output.Height - y - 1 : y;
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
                            SignedDistance distance = edge.signedDistance(p, out param);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                nearEdge = edge; // TODO do we clone here?
                                nearParam = param;
                            }
                        }
                    }

                    if (nearEdge != null)
                        nearEdge.distanceToPseudoDistance(ref minDistance, p, nearParam);

                    *output[x, row] = (float)(minDistance.Distance / range + .5);
                }
            }
        }

        public unsafe void generateMSDF_legacy(BitmapRef<float> output, MsdfShape shape, double range, Vector2D scale, Vector2D translate, ErrorCorrectionConfig errorCorrectionConfig)
        {
            if (output.NPerPixel != 3)
                throw new IndexOutOfRangeException("A BitmapRef of 3 component-per-pixel is expected");

            for (int y = 0; y < output.Height; ++y)
            {
                int row = shape.InverseYAxis ? output.Height - y - 1 : y;
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
                            SignedDistance distance = edge.signedDistance(p, out param);
                            if ((edge.Color & EdgeColor.RED) == EdgeColor.RED && distance < r.minDistance)
                            {
                                r.minDistance = distance;
                                r.nearEdge = edge; // TODO clone here?
                                r.nearParam = param;
                            }
                            if ((edge.Color & EdgeColor.GREEN) == EdgeColor.GREEN && distance < g.minDistance)
                            {
                                g.minDistance = distance;
                                g.nearEdge = edge; // TODO clone here?
                                g.nearParam = param;
                            }
                            if ((edge.Color & EdgeColor.BLUE) == EdgeColor.BLUE && distance < b.minDistance)
                            {
                                b.minDistance = distance;
                                b.nearEdge = edge; // TODO clone here?
                                b.nearParam = param;
                            }
                        }
                    }

                    if (r.nearEdge != null)
                        r.nearEdge.distanceToPseudoDistance(ref r.minDistance, p, r.nearParam);
                    if (g.nearEdge != null)
                        g.nearEdge.distanceToPseudoDistance(ref g.minDistance, p, g.nearParam);
                    if (b.nearEdge != null)
                        b.nearEdge.distanceToPseudoDistance(ref b.minDistance, p, b.nearParam);
                    output[x, row][0] = (float)(r.minDistance.Distance / range + .5);
                    output[x, row][1] = (float)(g.minDistance.Distance / range + .5);
                    output[x, row][2] = (float)(b.minDistance.Distance / range + .5);
                }
            }

            errorCorrectionConfig.DistanceCheckMode = ErrorCorrectionConfig.DistanceErrorCheckMode.DO_NOT_CHECK_DISTANCE;
            var combiner = new OverlappingContourCombiner<MultiDistanceSelector, MultiDistance, PseudoDistanceSelectorBase.EdgeCache>(shape);
            ErrorCorrection.msdfErrorCorrection(combiner, output, shape, new MsdfProjection(scale, translate), range, new MSDFGeneratorConfig(false, errorCorrectionConfig));
        }

        public unsafe void generateMTSDF_legacy(BitmapRef<float> output, MsdfShape shape, double range, Vector2D scale, Vector2D translate, ErrorCorrectionConfig errorCorrectionConfig)
        {
            if (output.NPerPixel != 4)
                throw new IndexOutOfRangeException("A BitmapRef of 4 components-per-pixel is expected");

            for (int y = 0; y < output.Height; ++y)
            {
                int row = shape.InverseYAxis ? output.Height - y - 1 : y;
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
                            SignedDistance distance = edge.signedDistance(p, out param);
                            if (distance < minDistance)
                                minDistance = distance;

                            if ((edge.Color & EdgeColor.RED) == EdgeColor.RED && distance < r.minDistance)
                            {
                                r.minDistance = distance;
                                r.nearEdge = edge; // TODO .clone() here?
                                r.nearParam = param;
                            }
                            if ((edge.Color & EdgeColor.GREEN) == EdgeColor.GREEN && distance < g.minDistance)
                            {
                                g.minDistance = distance;
                                g.nearEdge = edge; // TODO clone here?
                                g.nearParam = param;
                            }
                            if ((edge.Color & EdgeColor.BLUE) == EdgeColor.BLUE && distance < b.minDistance)
                            {
                                b.minDistance = distance;
                                b.nearEdge = edge; // TODO clone here?
                                b.nearParam = param;
                            }
                        }

                        if (r.nearEdge != null)
                            r.nearEdge.distanceToPseudoDistance(ref r.minDistance, p, r.nearParam);
                        if (g.nearEdge != null)
                            g.nearEdge.distanceToPseudoDistance(ref g.minDistance, p, g.nearParam);
                        if (b.nearEdge != null)
                            b.nearEdge.distanceToPseudoDistance(ref b.minDistance, p, b.nearParam);

                        output[x, row][0] = (float)(r.minDistance.Distance / range + .5);
                        output[x, row][1] = (float)(g.minDistance.Distance / range + .5);
                        output[x, row][2] = (float)(b.minDistance.Distance / range + .5);
                        output[x, row][3] = (float)(minDistance.Distance / range + .5);
                    }
                }

                errorCorrectionConfig.DistanceCheckMode = ErrorCorrectionConfig.DistanceErrorCheckMode.DO_NOT_CHECK_DISTANCE;
                var combiner = new OverlappingContourCombiner<MultiAndTrueDistanceSelector, MultiAndTrueDistance, PseudoDistanceSelectorBase.EdgeCache>(shape);
                ErrorCorrection.msdfErrorCorrection(combiner, output, shape, new MsdfProjection(scale, translate), range, new MSDFGeneratorConfig(false, errorCorrectionConfig));
            }
        }
    }
}
