﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    public class MsdfGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="R">BitmapRef type</typeparam>
        /// <typeparam name="D">Distance Type</typeparam>
        /// <typeparam name="CC">Contour combiner</typeparam>
        /// <param name="output"></param>
        /// <param name="shape"></param>
        /// <param name="projection"></param>
        private void GenerateDistanceField<R, D, CC>(DistancePixelConversion<R, D> distancePixelConversion, BitmapRef<R> output, MsdfShape shape, MsdfProjection projection)
            where R : struct
            where D : struct
            where CC : ContourCombiner<D>
        {
            ShapeDistanceFinder<CC> distanceFinder = new ShapeDistanceFinder<CC>(shape);
            bool rightToLeft = false;
            for (int y = 0; y < output.Height; ++y)
            {
                int row = shape.InverseYAxis ? output.Height - y - 1 : y;
                for (int col = 0; col < output.Width; ++col)
                {
                    int x = rightToLeft ? output.Width - col - 1 : col;
                    Vector2D p = projection.Unproject(new Vector2D(x + .5, y + .5));
                    D distance = distanceFinder.distance(p);
                    output[x, row] = distancePixelConversion.Convert(distance);
                }
                rightToLeft = !rightToLeft;
            }
        }

        void generateSDF(BitmapRef<float> output, MsdfShape shape, MsdfProjection projection, double range, GeneratorConfig config)
        {
            if (config.OverlapSupport)
                generateDistanceField<OverlappingContourCombiner<TrueDistanceSelector>>(output, shape, projection, range);
            else
                generateDistanceField<SimpleContourCombiner<TrueDistanceSelector>>(output, shape, projection, range);
        }

        void generatePseudoSDF(BitmapRef<float> output, MsdfShape shape, MsdfProjection projection, double range, GeneratorConfig config)
        {
            if (config.OverlapSupport)
                generateDistanceField<OverlappingContourCombiner<PseudoDistanceSelector>>(output, shape, projection, range);
            else
                generateDistanceField<SimpleContourCombiner<PseudoDistanceSelector>>(output, shape, projection, range);
        }

        void generateMSDF(BitmapRef<Color3> output, MsdfShape shape, MsdfProjection projection, double range, MSDFGeneratorConfig config)
        {
            if (config.OverlapSupport)
                generateDistanceField<OverlappingContourCombiner<MultiDistanceSelector>>(output, shape, projection, range);
            else
                generateDistanceField<SimpleContourCombiner<MultiDistanceSelector>>(output, shape, projection, range);
            msdfErrorCorrection(output, shape, projection, range, config);
        }

        void generateMTSDF(BitmapRef<Color4> output, MsdfShape shape, MsdfProjection projection, double range, MSDFGeneratorConfig config)
        {
            if (config.OverlapSupport)
                generateDistanceField<OverlappingContourCombiner<MultiAndTrueDistanceSelector>>(output, shape, projection, range);
            else
                generateDistanceField<SimpleContourCombiner<MultiAndTrueDistanceSelector>>(output, shape, projection, range);
            msdfErrorCorrection(output, shape, projection, range, config);
        }

        // Legacy API

        void generateSDF(BitmapRef<float> output, MsdfShape shape, double range, Vector2D scale, Vector2D translate, bool overlapSupport)
        {
            generateSDF(output, shape, new MsdfProjection(scale, translate), range, new GeneratorConfig(overlapSupport));
        }

        void generatePseudoSDF(BitmapRef<float> output, MsdfShape shape, double range, Vector2D scale, Vector2D translate, bool overlapSupport)
        {
            generatePseudoSDF(output, shape, new MsdfProjection(scale, translate), range, new GeneratorConfig(overlapSupport));
        }

        void generateMSDF(BitmapRef<Color3> output, MsdfShape shape, double range, Vector2D scale, Vector2D translate, ErrorCorrectionConfig errorCorrectionConfig, bool overlapSupport)
        {
            generateMSDF(output, shape, new MsdfProjection(scale, translate), range, new MSDFGeneratorConfig(overlapSupport, errorCorrectionConfig));
        }

        void generateMTSDF(BitmapRef<Color4> output, MsdfShape shape, double range, Vector2D scale, Vector2D translate, ErrorCorrectionConfig errorCorrectionConfig, bool overlapSupport)
        {
            generateMTSDF(output, shape, new MsdfProjection(scale, translate), range, new MSDFGeneratorConfig(overlapSupport, errorCorrectionConfig));
        }

        // Legacy version
        void generateSDF_legacy(BitmapRef<float> output, MsdfShape shape, double range, Vector2D scale, Vector2D translate)
        {
            for (int y = 0; y < output.Height; ++y)
            {
                int row = shape.InverseYAxis ? output.Height - y - 1 : y;
                for (int x = 0; x < output.Width; ++x)
                {
                    double dummy;
                    Vector2D p = new Vector2D(x + .5, y + .5) / scale - translate;
                    SignedDistance minDistance = new SignedDistance();
                    for (std::vector<Contour>::const_iterator contour = shape.contours.begin(); contour != shape.contours.end(); ++contour)
                        for (std::vector<EdgeHolder>::const_iterator edge = contour->edges.begin(); edge != contour->edges.end(); ++edge)
                        {
                            SignedDistance distance = (*edge)->signedDistance(p, dummy);
                            if (distance < minDistance)
                                minDistance = distance;
                        }
                    output[x, row] = (float)(minDistance.Distance / range + .5);
                }
            }
        }

        void generatePseudoSDF_legacy(BitmapRef<float> output, MsdfShape shape, double range, Vector2D scale, Vector2D translate)
        {
            for (int y = 0; y < output.Height; ++y)
            {
                int row = shape.InverseYAxis ? output.Height - y - 1 : y;
                for (int x = 0; x < output.Width; ++x)
                {
                    Vector2D p = new Vector2D(x + .5, y + .5) / scale - translate;
                    SignedDistance minDistance = new SignedDistance();
                    EdgeHolder* nearEdge = null;
                    double nearParam = 0;
                    for (std::vector<Contour>::const_iterator contour = shape.contours.begin(); contour != shape.contours.end(); ++contour)
                        for (std::vector<EdgeHolder>::const_iterator edge = contour->edges.begin(); edge != contour->edges.end(); ++edge)
                        {
                            double param;
                            SignedDistance distance = (*edge)->signedDistance(p, param);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                nearEdge = &*edge;
                                nearParam = param;
                            }
                        }
                    if (nearEdge)
                        (*nearEdge)->distanceToPseudoDistance(minDistance, p, nearParam);
                    output[x, row] = (float)(minDistance.Distance / range + .5);
                }
            }
        }

        void generateMSDF_legacy(BitmapRef<Color3> output, MsdfShape shape, double range, Vector2D scale, Vector2D translate, ErrorCorrectionConfig errorCorrectionConfig)
        {
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

                    for (std::vector<Contour>::const_iterator contour = shape.contours.begin(); contour != shape.contours.end(); ++contour) {
                        for (std::vector<EdgeHolder>::const_iterator edge = contour->edges.begin(); edge != contour->edges.end(); ++edge)
                        {
                            double param;
                            SignedDistance distance = (*edge)->signedDistance(p, param);
                            if ((*edge)->color & EdgeColor.RED && distance < r.minDistance)
                            {
                                r.minDistance = distance;
                                r.nearEdge = &*edge;
                                r.nearParam = param;
                            }
                            if ((*edge)->color & EdgeColor.GREEN && distance < g.minDistance)
                            {
                                g.minDistance = distance;
                                g.nearEdge = &*edge;
                                g.nearParam = param;
                            }
                            if ((*edge)->color & EdgeColor.BLUE && distance < b.minDistance)
                            {
                                b.minDistance = distance;
                                b.nearEdge = &*edge;
                                b.nearParam = param;
                            }
                        }
                    }

                    if (r.nearEdge)
                        (*r.nearEdge)->distanceToPseudoDistance(r.minDistance, p, r.nearParam);
                    if (g.nearEdge)
                        (*g.nearEdge)->distanceToPseudoDistance(g.minDistance, p, g.nearParam);
                    if (b.nearEdge)
                        (*b.nearEdge)->distanceToPseudoDistance(b.minDistance, p, b.nearParam);
                    output[x, row][0] = (float)(r.minDistance.Distance / range + .5);
                    output[x, row][1] = (float)(g.minDistance.Distance / range + .5);
                    output[x, row][2] = (float)(b.minDistance.Distance / range + .5);
                }
            }

            errorCorrectionConfig.DistanceCheckMode = ErrorCorrectionConfig.DistanceErrorCheckMode.DO_NOT_CHECK_DISTANCE;
            msdfErrorCorrection(output, shape, new MsdfProjection(scale, translate), range, new MSDFGeneratorConfig(false, errorCorrectionConfig));
        }

        void generateMTSDF_legacy(BitmapRef<Color4> output, MsdfShape shape, double range, Vector2D scale, Vector2D translate, ErrorCorrectionConfig errorCorrectionConfig)
        {
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

                    for (std::vector<Contour>::const_iterator contour = shape.contours.begin(); contour != shape.contours.end(); ++contour)
                        for (std::vector<EdgeHolder>::const_iterator edge = contour->edges.begin(); edge != contour->edges.end(); ++edge)
                        {
                            double param = 0;
                            SignedDistance distance = (*edge)->signedDistance(p, param);
                            if (distance < minDistance)
                                minDistance = distance;
                            if ((*edge)->color & EdgeColor.RED && distance < r.minDistance)
                            {
                                r.minDistance = distance;
                                r.nearEdge = &*edge;
                                r.nearParam = param;
                            }
                            if ((*edge)->color & EdgeColor.GREEN && distance < g.minDistance)
                            {
                                g.minDistance = distance;
                                g.nearEdge = &*edge;
                                g.nearParam = param;
                            }
                            if ((*edge)->color & EdgeColor.BLUE && distance < b.minDistance)
                            {
                                b.minDistance = distance;
                                b.nearEdge = &*edge;
                                b.nearParam = param;
                            }
                        }

                    if (r.nearEdge != null)
                        (*r.nearEdge)->distanceToPseudoDistance(r.minDistance, p, r.nearParam);
                    if (g.nearEdge)
                        (*g.nearEdge)->distanceToPseudoDistance(g.minDistance, p, g.nearParam);
                    if (b.nearEdge)
                        (*b.nearEdge)->distanceToPseudoDistance(b.minDistance, p, b.nearParam);
                    output[x, row][0] = (float)(r.minDistance.Distance / range + .5);
                    output[x, row][1] = (float)(g.minDistance.Distance / range + .5);
                    output[x, row][2] = (float)(b.minDistance.Distance / range + .5);
                    output[x, row][3] = (float)(minDistance.Distance / range + .5);
                }
            }

            errorCorrectionConfig.DistanceCheckMode = ErrorCorrectionConfig.DistanceErrorCheckMode.DO_NOT_CHECK_DISTANCE;
            msdfErrorCorrection(output, shape, new MsdfProjection(scale, translate), range, new MSDFGeneratorConfig(false, errorCorrectionConfig));
        }
    }
}
