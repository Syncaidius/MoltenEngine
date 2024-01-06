using Molten.Shapes;

namespace Molten.Graphics.SDF;

public static class ErrorCorrection
{
    /// <summary>
    /// Outputs the scanline that intersects the shape at y.
    /// </summary>
    /// <param name=""></param>
    /// <param name=""></param>
    /// <param name="y"></param>
    private static unsafe void CalcScanelineIntersections(Shape shape, Scanline line, double y)
    {
        List<Scanline.Intersection> intersections = new List<Scanline.Intersection>();
        double* x = stackalloc double[3];
        int* dy = stackalloc int[3];

        foreach (Contour contour in shape.Contours)
        {
            foreach (Edge edge in contour.Edges)
            {
                int n = edge.ScanlineIntersections(x, dy, y);
                for (int i = 0; i < n; ++i)
                {
                    Scanline.Intersection intersection = new Scanline.Intersection(x[i], dy[i]);
                    intersections.Add(intersection);
                }
            }
        }

        line.SetIntersections(intersections);
    }

    public unsafe static void MsdfErrorCorrection(TextureSliceRef<Color3> sdf, Shape shape, SdfProjection projection, double range, SdfConfig config)
    {
        if (config.Mode == SdfConfig.ErrorCorrectMode.DISABLED)
            return;

        SdfErrorCorrection ec = new SdfErrorCorrection(sdf, projection, range);
        ec.SetMinDeviationRatio(config.MinDeviationRatio);
        ec.SetMinImproveRatio(config.MinImproveRatio);
        switch (config.Mode)
        {
            case SdfConfig.ErrorCorrectMode.DISABLED:
            case SdfConfig.ErrorCorrectMode.INDISCRIMINATE:
                break;
            case SdfConfig.ErrorCorrectMode.EDGE_PRIORITY:
                ec.ProtectCorners(shape);
                ec.ProtectEdges(sdf);
                break;
            case SdfConfig.ErrorCorrectMode.EDGE_ONLY:
                ec.ProtectAll();
                break;
        }
        if (config.DistanceCheckMode == SdfConfig.DistanceErrorCheckMode.DO_NOT_CHECK_DISTANCE ||
            (config.DistanceCheckMode == SdfConfig.DistanceErrorCheckMode.CHECK_DISTANCE_AT_EDGE &&
            config.Mode != SdfConfig.ErrorCorrectMode.EDGE_ONLY))
        {
            ec.FindErrors(sdf);
            if (config.DistanceCheckMode == SdfConfig.DistanceErrorCheckMode.CHECK_DISTANCE_AT_EDGE)
                ec.ProtectAll();
        }

        if (config.DistanceCheckMode == SdfConfig.DistanceErrorCheckMode.ALWAYS_CHECK_DISTANCE ||
            config.DistanceCheckMode == SdfConfig.DistanceErrorCheckMode.CHECK_DISTANCE_AT_EDGE)
        {
            ec.FindErrors(sdf, shape);
        }
        ec.Apply(sdf);
    }

    internal unsafe static void MultiDistanceSignCorrection(TextureSliceRef<Color3> sdf, Shape shape, SdfProjection projection, FillRule fillRule)
    {
        uint w = sdf.Width;
        uint h = sdf.Height;

        if ((w * h) == 0)
            return;

        Scanline scanline = new Scanline();
        bool ambiguous = false;
        sbyte[] matchMap = new sbyte[w * h];

        fixed (sbyte* ptrMap = matchMap)
        {
            sbyte* match = ptrMap;

            for (int y = 0; y < h; ++y)
            {
                CalcScanelineIntersections(shape, scanline, projection.UnprojectY(y + .5));

                for (int x = 0; x < w; ++x)
                {
                    bool fill = scanline.Filled(projection.UnprojectX(x + .5), fillRule);
                    Color3* msd = sdf[x, y];
                    float sd = MathHelper.Median(msd->R, msd->G, msd->B);

                    if (sd == .5f)
                    {
                        ambiguous = true;
                    }
                    else if ((sd > .5f) != fill)
                    {
                        msd->R = 1.0f - msd->R;
                        msd->G = 1.0f - msd->G;
                        msd->B = 1.0f - msd->B;
                        *match = -1;
                    }
                    else
                    {
                        *match = 1;
                    }

                    ++match;
                }
            }

            // This step is necessary to avoid artifacts when whole shape is inverted
            if (ambiguous)
            {
                match = &ptrMap[0];
                for (int y = 0; y < h; ++y)
                {
                    for (int x = 0; x < w; ++x)
                    {
                        if (*match == 0)
                        {
                            int neighborMatch = 0;
                            if (x > 0) neighborMatch += *(match - 1);
                            if (x < w - 1) neighborMatch += *(match + 1);
                            if (y > 0) neighborMatch += *(match - w);
                            if (y < h - 1) neighborMatch += *(match + w);
                            if (neighborMatch < 0)
                            {
                                Color3* msd = sdf[x, y];
                                msd->R = 1.0f - msd->R;
                                msd->G = 1.0f - msd->G;
                                msd->B = 1.0f - msd->B;
                            }
                        }
                        ++match;
                    }
                }
            }
        }
    }
}
