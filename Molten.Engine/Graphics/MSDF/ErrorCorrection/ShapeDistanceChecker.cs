using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="ES">Edge selector type</typeparam>
    /// <typeparam name="DT">Distance Type</typeparam>
    public unsafe class ShapeDistanceChecker<ES, DT, EC>
        where ES : EdgeSelector<DT, EC>, new()
        where DT : unmanaged
        where EC : unmanaged
    {
        public Vector2D shapeCoord, sdfCoord;
        public float* msd;
        public bool protectedFlag;

        internal ShapeDistanceFinder<ES, DT, EC> distanceFinder;
        internal BitmapRef<float> sdf;
        internal double invRange;
        internal Vector2D texelSize;
        internal double minImproveRatio;

        public ShapeDistanceChecker(BitmapRef<float> pSdf, MsdfShape pShape, MsdfProjection pProjection, double pInvRange, double pMinImproveRatio)
        {
            distanceFinder = new ShapeDistanceFinder<ContourCombiner<PseudoDistanceSelector>>(pShape);
            sdf = pSdf;
            invRange = pInvRange;
            minImproveRatio = pMinImproveRatio;

            texelSize = pProjection.UnprojectVector(new Vector2D(1));
        }

        public ArtifactClassifier<ES, DT, EC> classifier(Vector2D direction, double span)
        {
            return new ArtifactClassifier<ES, DT, EC>(this, direction, span);
        }
    }
}
