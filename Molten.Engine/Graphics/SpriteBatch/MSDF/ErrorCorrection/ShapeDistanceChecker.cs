using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="ES">Edge selector type</typeparam>
    /// <typeparam name="DT">Distance Type</typeparam>
    internal unsafe class ShapeDistanceChecker<ES, DT, EC>
        where ES : EdgeSelector<DT, EC>, new()
        where DT : unmanaged
        where EC : unmanaged
    {
        public Vector2D shapeCoord, sdfCoord;
        public float* msd;
        public bool protectedFlag;

        ShapeDistanceFinder<ES, DT, EC> distanceFinder;
        BitmapRef<float> sdf;
        double invRange;
        Vector2D texelSize;
        double minImproveRatio;

        public ShapeDistanceChecker(BitmapRef<float> pSdf, MsdfShape pShape, MsdfProjection pProjection, double pInvRange, double pMinImproveRatio)
        {
            distanceFinder = new ShapeDistanceFinder<ContourCombiner<PseudoDistanceSelector>>(pShape);
            sdf = pSdf;
            invRange = pInvRange;
            minImproveRatio = pMinImproveRatio;

            texelSize = pProjection.UnprojectVector(new Vector2D(1));
        }

        public ArtifactClassifier<ES, DT> classifier(Vector2D direction, double span)
        {
            return new ArtifactClassifier<ES, DT>(this, direction, span);
        }
    }
}
