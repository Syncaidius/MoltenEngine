namespace Molten.Graphics.SDF
{
    public class BaseArtifactClassifier
    {
        protected double Span { get; set; }
        protected bool ProtectedFlag { get; set; }

        public BaseArtifactClassifier(double span, bool protectedFlag)
        {
            Span = span;
            ProtectedFlag = protectedFlag;
        }

        public int RangeTest(double at, double bt, double xt, float am, float bm, float xm)
        {
            // For protected texels, only consider inversion artifacts (interpolated median has different sign than boundaries). For the rest, it is sufficient that the interpolated median is outside its boundaries.
            if ((am > .5f && bm > .5f && xm <= .5f) || (am < .5f && bm < .5f && xm >= .5f) || (!ProtectedFlag && MathHelper.Median(am, bm, xm) != xm))
            {
                double axSpan = (xt - at) * Span, bxSpan = (bt - xt) * Span;
                // Check if the interpolated median's value is in the expected range based on its distance (span) from boundaries a, b.
                if (!(xm >= am - axSpan && xm <= am + axSpan && xm >= bm - bxSpan && xm <= bm + bxSpan))
                    return SdfErrorCorrection.CLASSIFIER_FLAG_CANDIDATE | SdfErrorCorrection.CLASSIFIER_FLAG_ARTIFACT;
                return SdfErrorCorrection.CLASSIFIER_FLAG_CANDIDATE;
            }
            return 0;
        }

        public virtual bool Evaluate(double t, float m, int flags)
        {
            return (flags & 2) != 0;
        }
    }
}
