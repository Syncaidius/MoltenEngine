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
    public class ArtifactClassifier : BaseArtifactClassifier
    {
        ShapeDistanceChecker _parent;
        Vector2D _direction;

        public ArtifactClassifier(ShapeDistanceChecker parent, in Vector2D direction, double span) :
            base(span, parent.ProtectedFlag)
        {
            _parent = parent;
            _direction = direction;
        }

        public override unsafe bool Evaluate(double t, float m, int flags)
        {
            if ((flags & SdfErrorCorrection.CLASSIFIER_FLAG_CANDIDATE) == SdfErrorCorrection.CLASSIFIER_FLAG_CANDIDATE)
            {
                // Skip expensive distance evaluation if the point has already been classified as an artifact by the base classifier.
                if ((flags & SdfErrorCorrection.CLASSIFIER_FLAG_ARTIFACT) == SdfErrorCorrection.CLASSIFIER_FLAG_ARTIFACT)
                    return true;

                Vector2D tVector = t * _direction;
                Color3 oldMSD = new Color3();

                // Compute the color that would be currently interpolated at the artifact candidate's position.
                Vector2D sdfCoord = _parent.SdfCoord + tVector;
                SdfErrorCorrection.Interpolate(&oldMSD, _parent.Sdf, sdfCoord);

                // Compute the color that would be interpolated at the artifact candidate's position if error correction was applied on the current texel.
                double aWeight = (1 - Math.Abs(tVector.X)) * (1 - Math.Abs(tVector.Y));
                float aPSD = MathHelper.Median(_parent.Msd->R, _parent.Msd->G, _parent.Msd->B);

                Color3 newMSD = new Color3()
                {
                    R = (float)(oldMSD.R + aWeight * (aPSD - _parent.Msd->R)),
                    G = (float)(oldMSD.G + aWeight * (aPSD - _parent.Msd->G)),
                    B = (float)(oldMSD.B + aWeight * (aPSD - _parent.Msd->B))
                };

                // Compute the evaluated distance (interpolated median) before and after error correction, as well as the exact shape distance.
                float oldPSD = MathHelper.Median(oldMSD.R, oldMSD.G, oldMSD.B);
                float newPSD = MathHelper.Median(newMSD.R, newMSD.G, newMSD.B);

                Vector2D origin = _parent.ShapeCoord + tVector * _parent.TexelSize;
                float refPSD = _parent.DistanceFinder.getRefPSD(ref origin, _parent.InvRange); // (float)(parent.invRange * parent.distanceFinder.distance(parent.shapeCoord + tVector * parent.texelSize) + .5);
                
                // Compare the differences of the exact distance and the before and after distances.
                return _parent.MinImproveRatio * Math.Abs(newPSD - refPSD) < (double)Math.Abs(oldPSD - refPSD);
            }
            return false;
        }
    }
}
