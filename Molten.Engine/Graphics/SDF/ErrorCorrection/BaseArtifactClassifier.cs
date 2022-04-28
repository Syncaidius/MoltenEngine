using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public class BaseArtifactClassifier
    {
        protected double span;
        protected bool protectedFlag;

        public BaseArtifactClassifier(double span, bool protectedFlag)
        {
            this.span = span;
            this.protectedFlag = protectedFlag;
        }

        public int RangeTest(double at, double bt, double xt, float am, float bm, float xm)
        {
            // For protected texels, only consider inversion artifacts (interpolated median has different sign than boundaries). For the rest, it is sufficient that the interpolated median is outside its boundaries.
            if ((am > .5f && bm > .5f && xm <= .5f) || (am < .5f && bm < .5f && xm >= .5f) || (!protectedFlag && MathHelper.Median(am, bm, xm) != xm))
            {
                double axSpan = (xt - at) * span, bxSpan = (bt - xt) * span;
                // Check if the interpolated median's value is in the expected range based on its distance (span) from boundaries a, b.
                if (!(xm >= am - axSpan && xm <= am + axSpan && xm >= bm - bxSpan && xm <= bm + bxSpan))
                    return MSDFErrorCorrection.CLASSIFIER_FLAG_CANDIDATE | MSDFErrorCorrection.CLASSIFIER_FLAG_ARTIFACT;
                return MSDFErrorCorrection.CLASSIFIER_FLAG_CANDIDATE;
            }
            return 0;
        }

        public virtual bool Evaluate(double t, float m, int flags)
        {
            return (flags & 2) != 0;
        }
    }
}
