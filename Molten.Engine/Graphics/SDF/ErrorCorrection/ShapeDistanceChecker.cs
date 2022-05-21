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
    public unsafe class ShapeDistanceChecker
    {
        public Vector2D shapeCoord, sdfCoord;
        public Color3* msd;
        public bool protectedFlag;

        internal ShapeDistanceFinder distanceFinder;
        internal TextureSliceRef<Color3> sdf;
        internal double invRange;
        internal Vector2D texelSize;
        internal double minImproveRatio;

        public ShapeDistanceChecker(TextureSliceRef<Color3> pSdf, Shape pShape, SdfProjection pProjection, double pInvRange, double pMinImproveRatio)
        {
            distanceFinder = new ShapeDistanceFinder(pShape);
            sdf = pSdf;
            invRange = pInvRange;
            minImproveRatio = pMinImproveRatio;

            texelSize = pProjection.UnprojectVector(new Vector2D(1));
        }

        public ArtifactClassifier Classifier(Vector2D direction, double span)
        {
            return new ArtifactClassifier(this, direction, span);
        }
    }
}
