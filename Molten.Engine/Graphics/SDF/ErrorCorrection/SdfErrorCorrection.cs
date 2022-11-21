using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.DoublePrecision;

namespace Molten.Graphics.SDF
{
    internal class SdfErrorCorrection
    {
        public const double ARTIFACT_T_EPSILON = 0.01;
        public const double PROTECTION_RADIUS_TOLERANCE = 1.001;

        public const int CLASSIFIER_FLAG_CANDIDATE = 0x01;
        public const int CLASSIFIER_FLAG_ARTIFACT = 0x02;

        TextureSlice _stencilSlice;
        TextureSliceRef<byte> _stencil;
        SdfProjection _projection;
        double _invRange;
        double _minDeviationRatio;
        double _minImproveRatio;

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

        public unsafe SdfErrorCorrection(TextureSliceRef<Color3> sdf, SdfProjection pProjection, double range)
        {
            _stencilSlice = new TextureSlice(sdf.Width, sdf.Height, sdf.Width * sdf.Height);
            _stencil = _stencilSlice.GetReference<byte>();
            _projection = pProjection;

            _invRange = 1 / range;
            _minDeviationRatio = SdfConfig.defaultMinDeviationRatio;
            _minImproveRatio = SdfConfig.defaultMinImproveRatio;
            EngineUtil.MemSet(_stencil.Data, 0, (nuint)(sizeof(byte) * _stencil.Width * _stencil.Height));
        }

        public void SetMinDeviationRatio(double minDeviationRatio)
        {
            this._minDeviationRatio = minDeviationRatio;
        }

        public void SetMinImproveRatio(double minImproveRatio)
        {
            this._minImproveRatio = minImproveRatio;
        }

        public unsafe void ProtectCorners(Shape shape)
        {
            foreach (Shape.Contour contour in shape.Contours)
            {
                if (contour.Edges.Count > 0)
                {
                    Shape.Edge prevEdge = contour.Edges.Last();
                    foreach (Shape.Edge edge in contour.Edges)
                    {
                        EdgeColor commonColor = prevEdge.Color & edge.Color;
                        // If the color changes from prevEdge to edge, this is a corner.
                        if ((commonColor & (commonColor - 1)) != (commonColor - 1))
                        {
                            // Find the four texels that envelop the corner and mark them as protected.
                            Vector2D p = _projection.Project(edge.Point(0));

                            int l = (int)Math.Floor(p.X - .5);
                            int b = (int)Math.Floor(p.Y - .5);
                            int r = l + 1;
                            int t = b + 1;
                            // Check that the positions are within bounds.
                            if (l < _stencil.Width && b < _stencil.Height && r >= 0 && t >= 0)
                            {
                                if (l >= 0 && b >= 0)
                                    *_stencil[l, b] |= (byte)StencilFlags.PROTECTED;
                                if (r < _stencil.Width && b >= 0)
                                    *_stencil[r, b] |= (byte)StencilFlags.PROTECTED;
                                if (l >= 0 && t < _stencil.Height)
                                    *_stencil[l, t] |= (byte)StencilFlags.PROTECTED;
                                if (r < _stencil.Width && t < _stencil.Height)
                                    *_stencil[r, t] |= (byte)StencilFlags.PROTECTED;
                            }
                        }
                        prevEdge = edge;
                    }
                }
            }
        }

        public unsafe int EdgeBetweenTexelsChannel(ref Color3 a, ref Color3 b, int channel)
        {
            // Find interpolation ratio t (0 < t < 1) where an edge is expected (mix(a[channel], b[channel], t) == 0.5).
            double t = (a[channel] - .5) / (a[channel] - b[channel]);
            if (t > 0 && t < 1)
            {
                // Interpolate channel values at t.
                Color3 c = new Color3()
                {
                    R = MathHelper.Lerp(a.R, b.R, t),
                    G = MathHelper.Lerp(a.G, b.G, t),
                    B = MathHelper.Lerp(a.B, b.B, t)
                };

                // This is only an edge if the zero-distance channel is the median.
                return MathHelper.Median(c.R, c.G, c.B) == c[channel] ? 1 : 0;
            }

            return 0;
        }

        public int EdgeBetweenTexels(ref Color3 a, ref Color3 b)
        {
            return (int)EdgeColor.Red * EdgeBetweenTexelsChannel(ref a, ref b, 0) +
                (int)EdgeColor.Green * EdgeBetweenTexelsChannel(ref a, ref b, 1) +
                (int)EdgeColor.Blue * EdgeBetweenTexelsChannel(ref a, ref b, 2);
        }

        public unsafe void ProtectExtremeChannels(byte* stencil, Color3* msd, float m, int mask)
        {
            if ((((EdgeColor)mask & EdgeColor.Red) == EdgeColor.Red && msd->R != m) ||
                (((EdgeColor)mask & EdgeColor.Green) == EdgeColor.Green && msd->G != m) ||
                (((EdgeColor)mask & EdgeColor.Blue) == EdgeColor.Blue && msd->B != m)
            )
            {
                *stencil |= (byte)StencilFlags.PROTECTED;
            }
        }

        public unsafe void ProtectEdges(TextureSliceRef<Color3> sdf)
        {
            // Horizontal texel pairs
            float radius = (float)(PROTECTION_RADIUS_TOLERANCE * _projection.UnprojectVector(new Vector2D(_invRange, 0)).Length());

            for (int y = 0; y < sdf.Height; ++y)
            {
                Color3* left = sdf[0, y];
                Color3* right = sdf[1, y];

                for (int x = 0; x < sdf.Width - 1; ++x)
                {
                    float lm = MathHelper.Median(left->R, left->G, left->B);
                    float rm = MathHelper.Median(right->R, right->G, right->B);

                    if (Math.Abs(lm - .5f) + Math.Abs(rm - .5f) < radius)
                    {
                        int mask = EdgeBetweenTexels(ref *left, ref *right);
                        ProtectExtremeChannels(_stencil[x, y], left, lm, mask);
                        ProtectExtremeChannels(_stencil[x + 1, y], right, rm, mask);
                    }

                    left++;
                    right++;
                }
            }

            // Vertical texel pairs
            radius = (float)(PROTECTION_RADIUS_TOLERANCE * _projection.UnprojectVector(new Vector2D(0, _invRange)).Length());
            for (int y = 0; y < sdf.Height - 1; ++y)
            {
                Color3* bottom = sdf[0, y];
                Color3* top = sdf[0, y + 1];

                for (int x = 0; x < sdf.Width; ++x)
                {
                    float bm = MathHelper.Median(bottom->R, bottom->G, bottom->B);
                    float tm = MathHelper.Median(top->R, top->G, top->B);

                    if (Math.Abs(bm - .5f) + Math.Abs(tm - .5f) < radius)
                    {
                        int mask = EdgeBetweenTexels(ref *bottom, ref *top);
                        ProtectExtremeChannels(_stencil[x, y], bottom, bm, mask);
                        ProtectExtremeChannels(_stencil[x, y + 1], top, tm, mask);
                    }

                    bottom++;
                    top++;
                }
            }

            // Diagonal texel pairs
            radius = (float)(PROTECTION_RADIUS_TOLERANCE * _projection.UnprojectVector(new Vector2D(_invRange)).Length());
            for (int y = 0; y < sdf.Height - 1; ++y)
            {
                Color3* lb = sdf[0, y];
                Color3* rb = sdf[1, y];
                Color3* lt = sdf[0, y + 1];
                Color3* rt = sdf[1, y + 1];

                for (int x = 0; x < sdf.Width - 1; ++x)
                {
                    float mlb = MathHelper.Median(lb->R, lb->G, lb->B);
                    float mrb = MathHelper.Median(rb->R, rb->G, rb->B);
                    float mlt = MathHelper.Median(lt->R, lt->G, lt->B);
                    float mrt = MathHelper.Median(rt->R, rt->G, rt->B);

                    if (Math.Abs(mlb - .5f) + Math.Abs(mrt - .5f) < radius)
                    {
                        int mask = EdgeBetweenTexels(ref *lb, ref *rt);
                        ProtectExtremeChannels(_stencil[x, y], lb, mlb, mask);
                        ProtectExtremeChannels(_stencil[x + 1, y + 1], rt, mrt, mask);
                    }

                    if (Math.Abs(mrb - .5f) + Math.Abs(mlt - .5f) < radius)
                    {
                        int mask = EdgeBetweenTexels(ref *rb, ref *lt);
                        ProtectExtremeChannels(_stencil[x + 1, y], rb, mrb, mask);
                        ProtectExtremeChannels(_stencil[x, y + 1], lt, mlt, mask);
                    }

                    lb++;
                    rb++;
                    lt++;
                    rt++;
                }
            }
        }

        public unsafe void ProtectAll()
        {
            byte* end = _stencil.Data + _stencil.Width * _stencil.Height;
            for (byte* mask = _stencil.Data; mask < end; ++mask)
                *mask |= (byte)StencilFlags.PROTECTED;
        }

        public unsafe float InterpolatedMedian(Color3* a, Color3* b, double t)
        {
            return MathHelper.Median(
                MathHelper.Lerp(a->R, b->R, t),
                MathHelper.Lerp(a->G, b->G, t),
                MathHelper.Lerp(a->B, b->B, t)
            );
        }

        public unsafe float InterpolatedMedian(Color3* a, Color3* l, Color3* q, double t)
        {
            return (float)(MathHelper.Median(
                 t * (t * q->R + l->R) + a->R,
                t * (t * q->G + l->G) + a->G,
                t * (t * q->B + l->B) + a->B
            ));
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
        private unsafe bool HasLinearArtifactInner(BaseArtifactClassifier artifactClassifier, float am, float bm, Color3* a, Color3* b, float dA, float dB)
        {
            // Find interpolation ratio t (0 < t < 1) where two color channels are equal (mix(dA, dB, t) == 0).
            double t = (double)dA / (dA - dB);
            if (t > ARTIFACT_T_EPSILON && t < 1 - ARTIFACT_T_EPSILON)
            {
                // Interpolate median at t and let the classifier decide if its value indicates an artifact.
                float xm = InterpolatedMedian(a, b, t);
                return artifactClassifier.Evaluate(t, xm, artifactClassifier.RangeTest(0, 1, t, am, bm, xm));
            }
            return false;
        }

        private unsafe bool HasDiagonalArtifactInner(BaseArtifactClassifier artifactClassifier,
            float am, float dm, Color3* a, Color3* l, Color3* q,
            float dA, float dBC, float dD, double tEx0, double tEx1)
        {
            // Find interpolation ratios t (0 < t[i] < 1) where two color channels are equal.
            double* t = stackalloc double[2];
            int solutions = SignedDistanceSolver.SolveQuadratic(t, dD - dBC + dA, dBC - dA - dA, dA);
            for (int i = 0; i < solutions; ++i)
            {
                // Solutions t[i] == 0 and t[i] == 1 are singularities and occur very often because two channels are usually equal at texels.
                if (t[i] > ARTIFACT_T_EPSILON && t[i] < 1 - ARTIFACT_T_EPSILON)
                {
                    // Interpolate median xm at t.
                    float xm = InterpolatedMedian(a, l, q, t[i]);
                    // Determine if xm deviates too much from medians of a, d.
                    int rangeFlags = artifactClassifier.RangeTest(0, 1, t[i], am, dm, xm);
                    // Additionally, check xm against the interpolated medians at the local extremes tEx0, tEx1.
                    double* tEnd = stackalloc double[2];
                    float* em = stackalloc float[2];
                    // tEx0
                    if (tEx0 > 0 && tEx0 < 1)
                    {
                        tEnd[0] = 0; tEnd[1] = 1;
                        em[0] = am; em[1] = dm;
                        tEnd[tEx0 > t[i] ? 1 : 0] = tEx0;
                        em[tEx0 > t[i] ? 1 : 0] = InterpolatedMedian(a, l, q, tEx0);
                        rangeFlags |= artifactClassifier.RangeTest(tEnd[0], tEnd[1], t[i], am, dm, xm);
                    }
                    // tEx1
                    if (tEx1 > 0 && tEx1 < 1)
                    {
                        tEnd[0] = 0; tEnd[1] = 1;
                        em[0] = am; em[1] = dm;
                        tEnd[tEx1 > t[i] ? 1 : 0] = tEx1;
                        em[tEx1 > t[i] ? 1 : 0] = InterpolatedMedian(a, l, q, tEx1);
                        rangeFlags |= artifactClassifier.RangeTest(tEnd[0], tEnd[1], t[i], am, dm, xm);
                    }
                    if (artifactClassifier.Evaluate(t[i], xm, rangeFlags))
                        return true;
                }
            }
            return false;
        }

        public unsafe bool HasLinearArtifact(BaseArtifactClassifier artifactClassifier, float am, Color3* a, Color3* b)
        {
            float bm = MathHelper.Median(b->R, b->G, b->B);
            return (
                // Out of the pair, only report artifacts for the texel further from the edge to minimize side effects.
                Math.Abs(am - .5f) >= Math.Abs(bm - .5f) && (
                    // Check points where each pair of color channels meets.
                    HasLinearArtifactInner(artifactClassifier, am, bm, a, b, a->G - a->R, b->G - b->R) ||
                    HasLinearArtifactInner(artifactClassifier, am, bm, a, b, a->B - a->G, b->B - b->G) ||
                    HasLinearArtifactInner(artifactClassifier, am, bm, a, b, a->R - a->B, b->R - b->B)
                )
            );
        }

        public unsafe bool HasDiagonalArtifact(BaseArtifactClassifier artifactClassifier, float am, Color3* a, Color3* b, Color3* c, Color3* d)
        {
            float dm = MathHelper.Median(d->R, d->G, d->B);

            // Out of the pair, only report artifacts for the texel further from the edge to minimize side effects.
            if (Math.Abs(am - .5f) >= Math.Abs(dm - .5f))
            {
                Color3 abc = new Color3()
                {
                    R = a->R - b->R - c->R,
                    G = a->G - b->G - c->G,
                    B = a->B - b->B - c->B
                };

                // Compute the linear terms for bilinear interpolation.
                Color3 l = new Color3()
                {
                    R = -a->R - abc.R,
                    G = -a->G - abc.G,
                    B = -a->B - abc.B
                };

                // Compute the quadratic terms for bilinear interpolation.
                Color3 q = new Color3()
                {
                    R = d->R + abc.R,
                    G = d->G + abc.G,
                    B = d->B + abc.B
                };

                // Compute interpolation ratios tEx (0 < tEx[i] < 1) for the local extremes of each color channel (the derivative 2*q[i]*tEx[i]+l[i] == 0).
                Color3D tEx = new Color3D()
                {
                    R = -.5 * l.R / q.R,
                    G = -.5 * l.G / q.G,
                    B = -.5 * l.B / q.B
                };
                

                // Check points where each pair of color channels meets.
                return (
                    HasDiagonalArtifactInner(artifactClassifier, am, dm, a, &l, &q, a->G - a->R, b->G - b->R + c->G - c->R, d->G - d->R, tEx.R, tEx.G) ||
                    HasDiagonalArtifactInner(artifactClassifier, am, dm, a, &l, &q, a->B - a->G, b->B - b->G + c->B - c->G, d->B - d->G, tEx.G, tEx.B) ||
                    HasDiagonalArtifactInner(artifactClassifier, am, dm, a, &l, &q, a->R - a->B, b->R - b->B + c->R - c->B, d->R - d->B, tEx.B, tEx.R)
                );
            }
            return false;
        }

        public unsafe void FindErrors(TextureSliceRef<Color3> sdf)
        {
            // Compute the expected deltas between values of horizontally, vertically, and diagonally adjacent texels.
            double hSpan = _minDeviationRatio * _projection.UnprojectVector(new Vector2D(_invRange, 0)).Length();
            double vSpan = _minDeviationRatio * _projection.UnprojectVector(new Vector2D(0, _invRange)).Length();
            double dSpan = _minDeviationRatio * _projection.UnprojectVector(new Vector2D(_invRange)).Length();
            // Inspect all texels.
            for (int y = 0; y < sdf.Height; ++y)
            {
                for (int x = 0; x < sdf.Width; ++x)
                {
                    Color3* c = sdf[x, y];
                    float cm = MathHelper.Median(c->R, c->G, c->B);
                    bool protectedFlag = ((StencilFlags)(*_stencil[x, y]) & StencilFlags.PROTECTED) != 0;
                    Color3* l = sdf[x - 1, y];
                    Color3* b = sdf[x, y - 1];
                    Color3* r = sdf[x + 1, y];
                    Color3* t = sdf[x, y + 1];

                    // Mark current texel c with the error flag if an artifact occurs when it's interpolated with any of its 8 neighbors.
                    *_stencil[x, y] |= (byte)((int)StencilFlags.ERROR * ((
                        (x > 0 && HasLinearArtifact(new BaseArtifactClassifier(hSpan, protectedFlag), cm, c, l)) ||
                        (y > 0 && HasLinearArtifact(new BaseArtifactClassifier(vSpan, protectedFlag), cm, c, b)) ||
                        (x < sdf.Width - 1 && HasLinearArtifact(new BaseArtifactClassifier(hSpan, protectedFlag), cm, c, r)) ||
                        (y < sdf.Height - 1 && HasLinearArtifact(new BaseArtifactClassifier(vSpan, protectedFlag), cm, c, t)) ||
                        (x > 0 && y > 0 && HasDiagonalArtifact(new BaseArtifactClassifier(dSpan, protectedFlag), cm, c, l, b, sdf[x - 1, y - 1])) ||
                        (x < sdf.Width - 1 && y > 0 && HasDiagonalArtifact(new BaseArtifactClassifier(dSpan, protectedFlag), cm, c, r, b, sdf[x + 1, y - 1])) ||
                        (x > 0 && y < sdf.Height - 1 && HasDiagonalArtifact(new BaseArtifactClassifier(dSpan, protectedFlag), cm, c, l, t, sdf[x - 1, y + 1])) ||
                        (x < sdf.Width - 1 && y < sdf.Height - 1 && HasDiagonalArtifact(new BaseArtifactClassifier(dSpan, protectedFlag), cm, c, r, t, sdf[x + 1, y + 1]))
                    ) ? 1 : 0));
                }
            }
        }

        public unsafe void FindErrors(TextureSliceRef<Color3> sdf, Shape shape)
        {
            // Compute the expected deltas between values of horizontally, vertically, and diagonally adjacent texels.
            double hSpan = _minDeviationRatio * _projection.UnprojectVector(new Vector2D(_invRange, 0)).Length();
            double vSpan = _minDeviationRatio * _projection.UnprojectVector(new Vector2D(0, _invRange)).Length();
            double dSpan = _minDeviationRatio * _projection.UnprojectVector(new Vector2D(_invRange)).Length();
            {
                ShapeDistanceChecker shapeDistanceChecker = new ShapeDistanceChecker(sdf, shape, _projection, _invRange, _minImproveRatio);
                bool rightToLeft = false;

                // Inspect all texels.
                for (int y = 0; y < sdf.Height; ++y)
                {
                    for (int col = 0; col < sdf.Width; ++col)
                    {
                        int x = (int)(rightToLeft ? sdf.Width - col - 1 : col);

                        if (((StencilFlags)(*_stencil[x, y]) & StencilFlags.ERROR) == StencilFlags.ERROR)
                            continue;

                        Color3* c = sdf[x, y];
                        shapeDistanceChecker.ShapeCoord = _projection.Unproject(new Vector2D(x + .5, y + .5));
                        shapeDistanceChecker.SdfCoord = new Vector2D(x + .5, y + .5);
                        shapeDistanceChecker.Msd = c;
                        shapeDistanceChecker.ProtectedFlag = ((StencilFlags)(*_stencil[x, y]) & StencilFlags.PROTECTED) != 0;
                        float cm = MathHelper.Median(c->R, c->G, c->B);

                        Color3* l = sdf[x - 1, y];
                        Color3* b = sdf[x, y - 1];
                        Color3* r = sdf[x + 1, y];
                        Color3* t = sdf[x, y + 1];

                        // Mark current texel c with the error flag if an artifact occurs when it's interpolated with any of its 8 neighbors.
                        *_stencil[x, y] |= (byte)((int)StencilFlags.ERROR * ((
                            (x > 0 && HasLinearArtifact(shapeDistanceChecker.Classifier(new Vector2D(-1, 0), hSpan), cm, c, l)) ||
                            (y > 0 && HasLinearArtifact(shapeDistanceChecker.Classifier(new Vector2D(0, -1), vSpan), cm, c, b)) ||
                            (x < sdf.Width - 1 && HasLinearArtifact(shapeDistanceChecker.Classifier(new Vector2D(+1, 0), hSpan), cm, c, r)) ||
                            (y < sdf.Height - 1 && HasLinearArtifact(shapeDistanceChecker.Classifier(new Vector2D(0, +1), vSpan), cm, c, t)) ||
                            (x > 0 && y > 0 && HasDiagonalArtifact(shapeDistanceChecker.Classifier(new Vector2D(-1, -1), dSpan), cm, c, l, b, sdf[x - 1, y - 1])) ||
                            (x < sdf.Width - 1 && y > 0 && HasDiagonalArtifact(shapeDistanceChecker.Classifier(new Vector2D(+1, -1), dSpan), cm, c, r, b, sdf[x + 1, y - 1])) ||
                            (x > 0 && y < sdf.Height - 1 && HasDiagonalArtifact(shapeDistanceChecker.Classifier(new Vector2D(-1, +1), dSpan), cm, c, l, t, sdf[x - 1, y + 1])) ||
                            (x < sdf.Width - 1 && y < sdf.Height - 1 && HasDiagonalArtifact(shapeDistanceChecker.Classifier(new Vector2D(+1, +1), dSpan), cm, c, r, t, sdf[x + 1, y + 1]))
                        ) ? 1 : 0));
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="output"></param>
        /// <param name="bitmap"></param>
        /// <param name="pos"></param>
        internal unsafe static void Interpolate(Color3* output, TextureSliceRef<Color3> bitmap, Vector2D pos)
        {
            pos -= .5;
            int l = (int)Math.Floor(pos.X);
            int b = (int)Math.Floor(pos.Y);
            int r = l + 1;
            int t = b + 1;
            double lr = pos.X - l;
            double bt = pos.Y - b;

            l = int.Clamp(l, 0, (int)bitmap.Width - 1);
            r = int.Clamp(r, 0, (int)bitmap.Width - 1);
            b = int.Clamp(b, 0, (int)bitmap.Height - 1);
            t = int.Clamp(t, 0, (int)bitmap.Height - 1);

            for (int i = 0; i < 3; ++i)
            {
                float start = MathHelper.Lerp((*bitmap[l, b])[i], (*bitmap[r, b])[i], lr);
                float end = MathHelper.Lerp((*bitmap[l, t])[i], (*bitmap[r, t])[i], lr);
                (*output)[i] = MathHelper.Lerp(start, end, bt);
            }
        }

        public unsafe void Apply(TextureSliceRef<Color3> sdf)
        {
            uint texelCount = sdf.Width * sdf.Height;
            byte* mask = _stencil.Data;
            Color3* texel = sdf.Data;

            for (int i = 0; i < texelCount; ++i)
            {
                if (((StencilFlags)(*mask) & StencilFlags.ERROR) == StencilFlags.ERROR)
                {
                    // Set all color channels to the median.
                    float m = MathHelper.Median(texel->R, texel->G, texel->B);
                    *texel = new Color3(m);
                }

                ++mask;
                texel++;
            }
        }
    }
}
