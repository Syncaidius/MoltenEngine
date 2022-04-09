using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    internal class MSDFErrorCorrection
    {
        public const double ARTIFACT_T_EPSILON = 0.01;
        public const double PROTECTION_RADIUS_TOLERANCE = 1.001;

        public const int CLASSIFIER_FLAG_CANDIDATE = 0x01;
        public const int CLASSIFIER_FLAG_ARTIFACT = 0x02;

        BitmapRef<byte> stencil;
        MsdfProjection projection;
        double invRange;
        double minDeviationRatio;
        double minImproveRatio;


        /// <summary>
        /// Stencil flags
        /// </summary>
        public enum StencilFlags : byte
        {
            /// Texel marked as potentially causing interpolation errors.
            ERROR = 1,
            /// Texel marked as protected. Protected texels are only given the error flag if they cause inversion artifacts.
            PROTECTED = 2
        };

        public unsafe MSDFErrorCorrection(BitmapRef<byte> pStencil, MsdfProjection pProjection, double range)
        {
            Validation.NPerPixel(stencil, 1);

            stencil = pStencil;
            projection = pProjection;

            invRange = 1 / range;
            minDeviationRatio = ErrorCorrectionConfig.defaultMinDeviationRatio;
            minImproveRatio = ErrorCorrectionConfig.defaultMinImproveRatio;
            EngineUtil.MemSet(stencil.pixels, 0, (nuint)(sizeof(byte) * stencil.Width * stencil.Height));
        }

        public void setMinDeviationRatio(double minDeviationRatio)
        {
            this.minDeviationRatio = minDeviationRatio;
        }

        public void setMinImproveRatio(double minImproveRatio)
        {
            this.minImproveRatio = minImproveRatio;
        }

        public unsafe void protectCorners(MsdfShape shape)
        {
            foreach (Contour contour in shape.Contours)
            {
                if (contour.Edges.Count > 0)
                {
                    EdgeHolder prevEdge = contour.Edges.Last();
                    foreach (EdgeHolder edge in contour.Edges)
                    {
                        EdgeColor commonColor = prevEdge.Segment.Color & edge.Segment.Color;
                        // If the color changes from prevEdge to edge, this is a corner.
                        if ((commonColor & (commonColor - 1)) != (commonColor - 1))
                        {
                            // Find the four texels that envelop the corner and mark them as protected.
                            Vector2D p = projection.Project(edge.Segment.point(0));
                            if (shape.InverseYAxis)
                                p.Y = stencil.Height - p.Y;
                            int l = (int)Math.Floor(p.X - .5);
                            int b = (int)Math.Floor(p.Y - .5);
                            int r = l + 1;
                            int t = b + 1;
                            // Check that the positions are within bounds.
                            if (l < stencil.Width && b < stencil.Height && r >= 0 && t >= 0)
                            {
                                if (l >= 0 && b >= 0)
                                    *stencil[l, b] |= (byte)StencilFlags.PROTECTED;
                                if (r < stencil.Width && b >= 0)
                                    *stencil[r, b] |= (byte)StencilFlags.PROTECTED;
                                if (l >= 0 && t < stencil.Height)
                                    *stencil[l, t] |= (byte)StencilFlags.PROTECTED;
                                if (r < stencil.Width && t < stencil.Height)
                                    *stencil[r, t] |= (byte)StencilFlags.PROTECTED;
                            }
                        }
                        prevEdge = edge;
                    }
                }
            }
        }

        public unsafe int edgeBetweenTexelsChannel(float* a, float* b, int channel)
        {
            // Find interpolation ratio t (0 < t < 1) where an edge is expected (mix(a[channel], b[channel], t) == 0.5).
            double t = (a[channel] - .5) / (a[channel] - b[channel]);
            if (t > 0 && t < 1)
            {
                // Interpolate channel values at t.
                float* c = stackalloc float[3];
                c[0] = MsdfMath.mix(a[0], b[0], t);
                c[1] = MsdfMath.mix(a[1], b[1], t);
                c[2] = MsdfMath.mix(a[2], b[2], t);
                // This is only an edge if the zero-distance channel is the median.
                return MsdfMath.median(c[0], c[1], c[2]) == c[channel] ? 1 : 0;
            }
            return 0;
        }

        public unsafe int edgeBetweenTexels(float* a, float* b)
        {
            return (int)EdgeColor.RED * edgeBetweenTexelsChannel(a, b, 0) +
                (int)EdgeColor.GREEN * edgeBetweenTexelsChannel(a, b, 1) +
                (int)EdgeColor.BLUE * edgeBetweenTexelsChannel(a, b, 2);
        }

        public unsafe void protectExtremeChannels(byte* stencil, float* msd, float m, int mask)
        {
            if ((((EdgeColor)mask & EdgeColor.RED) == EdgeColor.RED && msd[0] != m) ||
                (((EdgeColor)mask & EdgeColor.GREEN) == EdgeColor.GREEN && msd[1] != m) ||
                (((EdgeColor)mask & EdgeColor.BLUE) == EdgeColor.BLUE && msd[2] != m)
            )
            {
                *stencil |= (byte)StencilFlags.PROTECTED;
            }
        }

        public unsafe void protectEdges(BitmapRef<float> sdf)
        {
            float radius;
            // Horizontal texel pairs
            radius = (float)(PROTECTION_RADIUS_TOLERANCE * projection.UnprojectVector(new Vector2D(invRange, 0)).Length());
            for (int y = 0; y < sdf.Height; ++y)
            {
                float* left = sdf[0, y];
                float* right = sdf[1, y];
                for (int x = 0; x < sdf.Width - 1; ++x)
                {
                    float lm = MsdfMath.median(left[0], left[1], left[2]);
                    float rm = MsdfMath.median(right[0], right[1], right[2]);
                    if (Math.Abs(lm - .5f) + Math.Abs(rm - .5f) < radius)
                    {
                        int mask = edgeBetweenTexels(left, right);
                        protectExtremeChannels(stencil[x, y], left, lm, mask);
                        protectExtremeChannels(stencil[x + 1, y], right, rm, mask);
                    }
                    left += sdf.NPerPixel;
                    right += sdf.NPerPixel;
                }
            }
            // Vertical texel pairs
            radius = (float)(PROTECTION_RADIUS_TOLERANCE * projection.UnprojectVector(new Vector2D(0, invRange)).Length());
            for (int y = 0; y < sdf.Height - 1; ++y)
            {
                float* bottom = sdf[0, y];
                float* top = sdf[0, y + 1];
                for (int x = 0; x < sdf.Width; ++x)
                {
                    float bm = MsdfMath.median(bottom[0], bottom[1], bottom[2]);
                    float tm = MsdfMath.median(top[0], top[1], top[2]);
                    if (Math.Abs(bm - .5f) + Math.Abs(tm - .5f) < radius)
                    {
                        int mask = edgeBetweenTexels(bottom, top);
                        protectExtremeChannels(stencil[x, y], bottom, bm, mask);
                        protectExtremeChannels(stencil[x, y + 1], top, tm, mask);
                    }
                    bottom += sdf.NPerPixel;
                    top += sdf.NPerPixel;
                }
            }
            // Diagonal texel pairs
            radius = (float)(PROTECTION_RADIUS_TOLERANCE * projection.UnprojectVector(new Vector2D(invRange)).Length());
            for (int y = 0; y < sdf.Height - 1; ++y)
            {
                float* lb = sdf[0, y];
                float* rb = sdf[1, y];
                float* lt = sdf[0, y + 1];
                float* rt = sdf[1, y + 1];
                for (int x = 0; x < sdf.Width - 1; ++x)
                {
                    float mlb = MsdfMath.median(lb[0], lb[1], lb[2]);
                    float mrb = MsdfMath.median(rb[0], rb[1], rb[2]);
                    float mlt = MsdfMath.median(lt[0], lt[1], lt[2]);
                    float mrt = MsdfMath.median(rt[0], rt[1], rt[2]);
                    if (Math.Abs(mlb - .5f) + Math.Abs(mrt - .5f) < radius)
                    {
                        int mask = edgeBetweenTexels(lb, rt);
                        protectExtremeChannels(stencil[x, y], lb, mlb, mask);
                        protectExtremeChannels(stencil[x + 1, y + 1], rt, mrt, mask);
                    }
                    if (Math.Abs(mrb - .5f) + Math.Abs(mlt - .5f) < radius)
                    {
                        int mask = edgeBetweenTexels(rb, lt);
                        protectExtremeChannels(stencil[x + 1, y], rb, mrb, mask);
                        protectExtremeChannels(stencil[x, y + 1], lt, mlt, mask);
                    }
                    lb += sdf.NPerPixel;
                    rb += sdf.NPerPixel;
                    lt += sdf.NPerPixel;
                    rt += sdf.NPerPixel;
                }
            }
        }

        public unsafe void protectAll()
        {
            byte* end = stencil.pixels + stencil.Width * stencil.Height;
            for (byte* mask = stencil.pixels; mask < end; ++mask)
                *mask |= (byte)StencilFlags.PROTECTED;
        }

        public unsafe float interpolatedMedian(float* a, float* b, double t)
        {
            return MsdfMath.median(
                MsdfMath.mix(a[0], b[0], t),
                MsdfMath.mix(a[1], b[1], t),
                MsdfMath.mix(a[2], b[2], t)
            );
        }

        public unsafe float interpolatedMedian(float* a, float* l, float* q, double t)
        {
            return (float)(MsdfMath.median(
                 t * (t * q[0] + l[0]) + a[0],
                t * (t * q[1] + l[1]) + a[1],
                t * (t * q[2] + l[2]) + a[2]
            ));
        }

        public bool isArtifact(bool isProtected, double axSpan, double bxSpan, float am, float bm, float xm)
        {
            return (
                // For protected texels, only report an artifact if it would cause fill inversion (change between positive and negative distance).
                (!isProtected || (am > .5f && bm > .5f && xm <= .5f) || (am < .5f && bm < .5f && xm >= .5f)) &&
                // This is an artifact if the interpolated median is outside the range of possible values based on its distance from a, b.
                !(xm >= am - axSpan && xm <= am + axSpan && xm >= bm - bxSpan && xm <= bm + bxSpan)
            );
        }

        /// <summary>
        /// Checks if a linear interpolation artifact will occur at a point where two specific color channels are equal - such points have extreme median values.
        /// </summary>
        /// <param name="artifactClassifier"></param>
        /// <param name="am"></param>
        /// <param name="bm"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="dA"></param>
        /// <param name="dB"></param>
        /// <returns></returns>
        public unsafe bool hasLinearArtifactInner(ArtifactClassifier artifactClassifier, float am, float bm, float* a, float* b, float dA, float dB)
        {
            // Find interpolation ratio t (0 < t < 1) where two color channels are equal (mix(dA, dB, t) == 0).
            double t = (double)dA / (dA - dB);
            if (t > ARTIFACT_T_EPSILON && t < 1 - ARTIFACT_T_EPSILON)
            {
                // Interpolate median at t and let the classifier decide if its value indicates an artifact.
                float xm = interpolatedMedian(a, b, t);
                return artifactClassifier.evaluate(t, xm, artifactClassifier.rangeTest(0, 1, t, am, bm, xm));
            }
            return false;
        }

        public unsafe bool hasDiagonalArtifactInner(ArtifactClassifier artifactClassifier,
            float am, float dm, float* a, float* l, float* q,
            float dA, float dBC, float dD, double tEx0, double tEx1)
        {
            // Find interpolation ratios t (0 < t[i] < 1) where two color channels are equal.
            double* t = stackalloc double[2];
            int solutions = EquationSolver.solveQuadratic(t, dD - dBC + dA, dBC - dA - dA, dA);
            for (int i = 0; i < solutions; ++i)
            {
                // Solutions t[i] == 0 and t[i] == 1 are singularities and occur very often because two channels are usually equal at texels.
                if (t[i] > ARTIFACT_T_EPSILON && t[i] < 1 - ARTIFACT_T_EPSILON)
                {
                    // Interpolate median xm at t.
                    float xm = interpolatedMedian(a, l, q, t[i]);
                    // Determine if xm deviates too much from medians of a, d.
                    int rangeFlags = artifactClassifier.rangeTest(0, 1, t[i], am, dm, xm);
                    // Additionally, check xm against the interpolated medians at the local extremes tEx0, tEx1.
                    double* tEnd = stackalloc double[2];
                    float* em = stackalloc float[2];
                    // tEx0
                    if (tEx0 > 0 && tEx0 < 1)
                    {
                        tEnd[0] = 0; tEnd[1] = 1;
                        em[0] = am; em[1] = dm;
                        tEnd[tEx0 > t[i] ? 1 : 0] = tEx0;
                        em[tEx0 > t[i] ? 1 : 0] = interpolatedMedian(a, l, q, tEx0);
                        rangeFlags |= artifactClassifier.rangeTest(tEnd[0], tEnd[1], t[i], am, dm, xm);
                    }
                    // tEx1
                    if (tEx1 > 0 && tEx1 < 1)
                    {
                        tEnd[0] = 0; tEnd[1] = 1;
                        em[0] = am; em[1] = dm;
                        tEnd[tEx1 > t[i] ? 1 : 0] = tEx1;
                        em[tEx1 > t[i] ? 1 : 0] = interpolatedMedian(a, l, q, tEx1);
                        rangeFlags |= artifactClassifier.rangeTest(tEnd[0], tEnd[1], t[i], am, dm, xm);
                    }
                    if (artifactClassifier.evaluate(t[i], xm, rangeFlags))
                        return true;
                }
            }
            return false;
        }

        public unsafe bool hasLinearArtifact(ArtifactClassifier artifactClassifier, float am, float* a, float* b)
        {
            float bm = MsdfMath.median(b[0], b[1], b[2]);
            return (
                // Out of the pair, only report artifacts for the texel further from the edge to minimize side effects.
                Math.Abs(am - .5f) >= Math.Abs(bm - .5f) && (
                    // Check points where each pair of color channels meets.
                    hasLinearArtifactInner(artifactClassifier, am, bm, a, b, a[1] - a[0], b[1] - b[0]) ||
                    hasLinearArtifactInner(artifactClassifier, am, bm, a, b, a[2] - a[1], b[2] - b[1]) ||
                    hasLinearArtifactInner(artifactClassifier, am, bm, a, b, a[0] - a[2], b[0] - b[2])
                )
            );
        }

        public unsafe bool hasDiagonalArtifact(ArtifactClassifier artifactClassifier, float am, float* a, float* b, float* c, float* d) {
            float dm = MsdfMath.median(d[0], d[1], d[2]);
            // Out of the pair, only report artifacts for the texel further from the edge to minimize side effects.
            if (Math.Abs(am - .5f) >= Math.Abs(dm - .5f)) {
                float* abc = stackalloc float[3];
                abc[0] = a[0] - b[0] - c[0];
                abc[1] = a[1] - b[1] - c[1];
                abc[2] = a[2] - b[2] - c[2];
                // Compute the linear terms for bilinear interpolation.
                float* l = stackalloc float[3];
                l[0] = -a[0] - abc[0];
                l[1] = -a[1] - abc[1];
                l[2] = -a[2] - abc[2];

                // Compute the quadratic terms for bilinear interpolation.
                float* q = stackalloc float[3];
                q[0] = d[0] + abc[0];
                q[1] = d[1] + abc[1];
                q[2] = d[2] + abc[2];

                // Compute interpolation ratios tEx (0 < tEx[i] < 1) for the local extremes of each color channel (the derivative 2*q[i]*tEx[i]+l[i] == 0).
                double* tEx = stackalloc double[3];
                tEx[0] = -.5 * l[0] / q[0];
                tEx[1] = -.5 * l[1] / q[1];
                tEx[2] = -.5 * l[2] / q[2];
                // Check points where each pair of color channels meets.
                return (
                    hasDiagonalArtifactInner(artifactClassifier, am, dm, a, l, q, a[1] - a[0], b[1] - b[0] + c[1] - c[0], d[1] - d[0], tEx[0], tEx[1]) ||
                    hasDiagonalArtifactInner(artifactClassifier, am, dm, a, l, q, a[2] - a[1], b[2] - b[1] + c[2] - c[1], d[2] - d[1], tEx[1], tEx[2]) ||
                    hasDiagonalArtifactInner(artifactClassifier, am, dm, a, l, q, a[0] - a[2], b[0] - b[2] + c[0] - c[2], d[0] - d[2], tEx[2], tEx[0])
                );
            }
            return false;
        }

        public unsafe void findErrors(BitmapRef<float> sdf)
        {
            // Compute the expected deltas between values of horizontally, vertically, and diagonally adjacent texels.
            double hSpan = minDeviationRatio * projection.UnprojectVector(new Vector2D(invRange, 0)).Length();
            double vSpan = minDeviationRatio * projection.UnprojectVector(new Vector2D(0, invRange)).Length();
            double dSpan = minDeviationRatio * projection.UnprojectVector(new Vector2D(invRange)).Length();
            // Inspect all texels.
            for (int y = 0; y < sdf.Height; ++y)
            {
                for (int x = 0; x < sdf.Width; ++x)
                {
                    float* c = sdf[x, y];
                    float cm = MsdfMath.median(c[0], c[1], c[2]);
                    bool protectedFlag = ((StencilFlags)(*stencil[x, y]) & StencilFlags.PROTECTED) != 0;
                    float* l = null, b = null, r = null, t = null;

                    // Mark current texel c with the error flag if an artifact occurs when it's interpolated with any of its 8 neighbors.
                    *stencil[x, y] |= (byte)(StencilFlags.ERROR * (
                        (x > 0 && ((l = sdf[x - 1, y]), hasLinearArtifact(new BaseArtifactClassifier(hSpan, protectedFlag), cm, c, l))) ||
                        (y > 0 && ((b = sdf[x, y - 1]), hasLinearArtifact(new BaseArtifactClassifier(vSpan, protectedFlag), cm, c, b))) ||
                        (x < sdf.Width - 1 && ((r = sdf[x + 1, y]), hasLinearArtifact(new BaseArtifactClassifier(hSpan, protectedFlag), cm, c, r))) ||
                        (y < sdf.Height - 1 && ((t = sdf[x, y + 1]), hasLinearArtifact(new BaseArtifactClassifier(vSpan, protectedFlag), cm, c, t))) ||
                        (x > 0 && y > 0 && hasDiagonalArtifact(new BaseArtifactClassifier(dSpan, protectedFlag), cm, c, l, b, sdf[x - 1, y - 1])) ||
                        (x < sdf.Width - 1 && y > 0 && hasDiagonalArtifact(new BaseArtifactClassifier(dSpan, protectedFlag), cm, c, r, b, sdf[x + 1, y - 1])) ||
                        (x > 0 && y < sdf.Height - 1 && hasDiagonalArtifact(new BaseArtifactClassifier(dSpan, protectedFlag), cm, c, l, t, sdf[x - 1, y + 1])) ||
                        (x < sdf.Width - 1 && y < sdf.Height - 1 && hasDiagonalArtifact(new BaseArtifactClassifier(dSpan, protectedFlag), cm, c, r, t, sdf[x + 1, y + 1]))
                    ));
                }
            }
        }

        public unsafe void findErrors<CC>(BitmapRef<float> sdf, MsdfShape shape)
            where CC: ContourCombiner
        {
            // Compute the expected deltas between values of horizontally, vertically, and diagonally adjacent texels.
            double hSpan = minDeviationRatio * projection.UnprojectVector(new Vector2D(invRange, 0)).Length();
            double vSpan = minDeviationRatio * projection.UnprojectVector(new Vector2D(0, invRange)).Length();
            double dSpan = minDeviationRatio * projection.UnprojectVector(new Vector2D(invRange)).Length();
            {
                ShapeDistanceChecker<CC> shapeDistanceChecker = new ShapeDistanceChecker<CC>(sdf, shape, projection, invRange, minImproveRatio);
                bool rightToLeft = false;
                // Inspect all texels.
                for (int y = 0; y < sdf.Height; ++y) {
                    int row = shape.InverseYAxis ? sdf.Height - y - 1 : y;
                    for (int col = 0; col < sdf.Width; ++col) {
                        int x = rightToLeft ? sdf.Width - col - 1 : col;
                        if (((StencilFlags)(*stencil[x, row]) & StencilFlags.ERROR) == StencilFlags.ERROR)
                            continue;
                        float* c = sdf[x, row];
                        shapeDistanceChecker.shapeCoord = projection.Unproject(new Vector2D(x + .5, y + .5));
                        shapeDistanceChecker.sdfCoord = new Vector2D(x + .5, row + .5);
                        shapeDistanceChecker.msd = c;
                        shapeDistanceChecker.protectedFlag = ((StencilFlags)(*stencil[x, row]) & StencilFlags.PROTECTED) != 0;
                        float cm = MsdfMath.median(c[0], c[1], c[2]);
                        float* l = null, b = null, r = null, t = null;
                        // Mark current texel c with the error flag if an artifact occurs when it's interpolated with any of its 8 neighbors.
                        *stencil[x, row] |= (byte)(StencilFlags.ERROR * (
                            (x > 0 && ((l = sdf[x - 1, row]), hasLinearArtifact(shapeDistanceChecker.classifier(new Vector2D(-1, 0), hSpan), cm, c, l))) ||
                            (row > 0 && ((b = sdf[x, row - 1]), hasLinearArtifact(shapeDistanceChecker.classifier(new Vector2D(0, -1), vSpan), cm, c, b))) ||
                            (x < sdf.Width - 1 && ((r = sdf[x + 1, row]), hasLinearArtifact(shapeDistanceChecker.classifier(new Vector2D(+1, 0), hSpan), cm, c, r))) ||
                            (row < sdf.Height - 1 && ((t = sdf[x, row + 1]), hasLinearArtifact(shapeDistanceChecker.classifier(new Vector2D(0, +1), vSpan), cm, c, t))) ||
                            (x > 0 && row > 0 && hasDiagonalArtifact(shapeDistanceChecker.classifier(new Vector2D(-1, -1), dSpan), cm, c, l, b, sdf[x - 1, row - 1])) ||
                            (x < sdf.Width - 1 && row > 0 && hasDiagonalArtifact(shapeDistanceChecker.classifier(new Vector2D(+1, -1), dSpan), cm, c, r, b, sdf[x + 1, row - 1])) ||
                            (x > 0 && row < sdf.Height - 1 && hasDiagonalArtifact(shapeDistanceChecker.classifier(new Vector2D(-1, +1), dSpan), cm, c, l, t, sdf[x - 1, row + 1])) ||
                            (x < sdf.Width - 1 && row < sdf.Height - 1 && hasDiagonalArtifact(shapeDistanceChecker.classifier(new Vector2D(+1, +1), dSpan), cm, c, r, t, sdf[x + 1, row + 1]))
                        ));
                    }
                }
            }
        }

        public unsafe void apply(BitmapRef<float> sdf) {
            int texelCount = sdf.Width * sdf.Height;
            byte* mask = stencil.pixels;
            float* texel = sdf.pixels;
            for (int i = 0; i < texelCount; ++i) {
                if (((StencilFlags)(*mask) & StencilFlags.ERROR) == StencilFlags.ERROR) {
                    // Set all color channels to the median.
                    float m = MsdfMath.median(texel[0], texel[1], texel[2]);
                    texel[0] = m; texel[1] = m; texel[2] = m;
                }
                ++mask;
                texel += sdf.NPerPixel;
            }
        }

        public BitmapRef<byte> getStencil() {
            return stencil;
        }
    }
}
