using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.DoublePrecision;

namespace Molten.Graphics.SDF
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="ES">Edge selector type</typeparam>
    /// <typeparam name="DT">Distance Type</typeparam>
    public unsafe class ShapeDistanceChecker
    {
        public Vector2D ShapeCoord;
        public Vector2D SdfCoord;
        public Color3* Msd;
        public bool ProtectedFlag;

        internal ShapeDistanceFinder DistanceFinder;
        internal TextureSliceRef<Color3> Sdf;
        internal double InvRange;
        internal Vector2D TexelSize;
        internal double MinImproveRatio;

        public ShapeDistanceChecker(TextureSliceRef<Color3> pSdf, Shape pShape, SdfProjection pProjection, double pInvRange, double pMinImproveRatio)
        {
            DistanceFinder = new ShapeDistanceFinder(pShape);
            Sdf = pSdf;
            InvRange = pInvRange;
            MinImproveRatio = pMinImproveRatio;

            TexelSize = pProjection.UnprojectVector(new Vector2D(1));
        }

        public ArtifactClassifier Classifier(Vector2D direction, double span)
        {
            return new ArtifactClassifier(this, direction, span);
        }
    }
}
