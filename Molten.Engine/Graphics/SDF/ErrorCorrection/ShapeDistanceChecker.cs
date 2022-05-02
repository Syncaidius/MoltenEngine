using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SDF
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="ES">Edge selector type</typeparam>
    /// <typeparam name="DT">Distance Type</typeparam>
    public unsafe class ShapeDistanceChecker<ES, DT>
        where ES : EdgeSelector<DT>, new()
        where DT : unmanaged
    {
        public Vector2D shapeCoord, sdfCoord;
        public float* msd;
        public bool protectedFlag;

        internal ShapeDistanceFinder<ES, DT> distanceFinder;
        internal TextureSliceRef<float> sdf;
        internal double invRange;
        internal Vector2D texelSize;
        internal double minImproveRatio;

        public ShapeDistanceChecker(TextureSliceRef<float> pSdf, Shape pShape, MsdfProjection pProjection, double pInvRange, double pMinImproveRatio)
        {
            distanceFinder = new ShapeDistanceFinder<ES,DT>(pShape);
            sdf = pSdf;
            invRange = pInvRange;
            minImproveRatio = pMinImproveRatio;

            texelSize = pProjection.UnprojectVector(new Vector2D(1));
        }

        public ArtifactClassifier<ES, DT> Classifier(Vector2D direction, double span)
        {
            return new ArtifactClassifier<ES, DT>(this, direction, span);
        }
    }
}
