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
    public class ArtifactClassifier<ES, DT> : BaseArtifactClassifier
        where ES : EdgeSelector<DT>, new()
        where DT : unmanaged
    {
        ShapeDistanceChecker<ES, DT> parent;
        Vector2D direction;

        public ArtifactClassifier(ShapeDistanceChecker<ES, DT> parent, in Vector2D direction, double span) :
            base(span, parent.protectedFlag)
        {
            this.parent = parent;
            this.direction = direction;
        }

        public unsafe bool Evaluate(int N, double t, float m, int flags)
        {
            if ((flags & MSDFErrorCorrection.CLASSIFIER_FLAG_CANDIDATE) == MSDFErrorCorrection.CLASSIFIER_FLAG_CANDIDATE)
            {
                // Skip expensive distance evaluation if the point has already been classified as an artifact by the base classifier.
                if ((flags & MSDFErrorCorrection.CLASSIFIER_FLAG_ARTIFACT) == MSDFErrorCorrection.CLASSIFIER_FLAG_ARTIFACT)
                    return true;

                Vector2D tVector = t * direction;
                float* oldMSD = stackalloc float[N];
                float* newMSD = stackalloc float[3];

                // Compute the color that would be currently interpolated at the artifact candidate's position.
                Vector2D sdfCoord = parent.sdfCoord + tVector;
                MsdfRasterization.Interpolate(oldMSD, parent.sdf, sdfCoord);

                // Compute the color that would be interpolated at the artifact candidate's position if error correction was applied on the current texel.
                double aWeight = (1 - Math.Abs(tVector.X)) * (1 - Math.Abs(tVector.Y));
                float aPSD = MsdfMath.Median(parent.msd[0], parent.msd[1], parent.msd[2]);
                newMSD[0] = (float)(oldMSD[0] + aWeight * (aPSD - parent.msd[0]));
                newMSD[1] = (float)(oldMSD[1] + aWeight * (aPSD - parent.msd[1]));
                newMSD[2] = (float)(oldMSD[2] + aWeight * (aPSD - parent.msd[2]));

                // Compute the evaluated distance (interpolated median) before and after error correction, as well as the exact shape distance.
                float oldPSD = MsdfMath.Median(oldMSD[0], oldMSD[1], oldMSD[2]);
                float newPSD = MsdfMath.Median(newMSD[0], newMSD[1], newMSD[2]);

                Vector2D origin = parent.shapeCoord + tVector * parent.texelSize;
                float refPSD = parent.distanceFinder.getRefPSD(ref origin, parent.invRange); // (float)(parent.invRange * parent.distanceFinder.distance(parent.shapeCoord + tVector * parent.texelSize) + .5);
                
                // Compare the differences of the exact distance and the before and after distances.
                return parent.minImproveRatio * Math.Abs(newPSD - refPSD) < (double)Math.Abs(oldPSD - refPSD);
            }
            return false;
        }
    }
}
