using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    internal class ShapeDistanceChecker<CC, T>
        where CC : ContourCombiner<T>
        where T : struct
    {
        public Vector2D shapeCoord, sdfCoord;
        public float* msd;
        public bool protectedFlag;

        ShapeDistanceFinder<ContourCombiner<PseudoDistanceSelector>> distanceFinder;
        BitmapConstRef<float, N> sdf;
        double invRange;
        Vector2D texelSize;
        double minImproveRatio;

        public ShapeDistanceChecker(BitmapConstRef<float, N> pSdf, MsdfShape pShape, MsdfProjection pProjection, double pInvRange, double pMinImproveRatio)
        {
            distanceFinder = pShape;
            sdf = pSdf;
            invRange = pInvRange;
            minImproveRatio = pMinImproveRatio;

            texelSize = pProjection.UnprojectVector(new Vector2D(1));
        }

        public ArtifactClassifier<CC, T> classifier(Vector2D direction, double span)
        {
            return new ArtifactClassifier<CC, T>(this, direction, span);
        }
    }
}
