using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    internal unsafe class ErrorCorrectionConfig
    {
        /// <summary>
        /// Mode of operation
        /// </summary>
        public enum ErrorCorrectMode
        {
            /// Skips error correction pass.
            DISABLED,
            /// Corrects all discontinuities of the distance field regardless if edges are adversely affected.
            INDISCRIMINATE,
            /// Corrects artifacts at edges and other discontinuous distances only if it does not affect edges or corners.
            EDGE_PRIORITY,
            /// Only corrects artifacts at edges.
            EDGE_ONLY
        }

        /// <summary>
        /// Configuration of whether to use an algorithm that computes the exact shape distance at the positions of suspected artifacts. 
        /// This algorithm can be much slower.
        /// </summary>
        public enum DistanceErrorCheckMode
        {
            /// Never computes exact shape distance.
            DO_NOT_CHECK_DISTANCE,

            /// Only computes exact shape distance at edges. Provides a good balance between speed and precision.
            CHECK_DISTANCE_AT_EDGE,

            /// Computes and compares the exact shape distance for each suspected artifact.
            ALWAYS_CHECK_DISTANCE
        }

        /// <summary>
        /// The default value of minDeviationRatio.
        /// </summary>
        public const double defaultMinDeviationRatio = 1.11111111111111111;

        /// <summary>
        /// The default value of minImproveRatio.
        /// </summary>
        public const double defaultMinImproveRatio = 1.11111111111111111;

        public ErrorCorrectMode Mode;

        public DistanceErrorCheckMode DistanceCheckMode;

        /// <summary>
        /// The minimum ratio between the actual and maximum expected distance delta to be considered an error.
        /// </summary>
        public double MinDeviationRatio;

        /// <summary>
        /// The minimum ratio between the pre-correction distance error and the post-correction distance error. Has no effect for DO_NOT_CHECK_DISTANCE.
        /// </summary>
        public double MinImproveRatio;

        public byte* Buffer;

        public ErrorCorrectionConfig(
            ErrorCorrectMode mode = ErrorCorrectMode.EDGE_PRIORITY, 
            DistanceErrorCheckMode distanceCheckMode = DistanceErrorCheckMode.CHECK_DISTANCE_AT_EDGE,
            double minDeviationRatio = defaultMinDeviationRatio,
            double minImproveRatio = defaultMinImproveRatio,
            byte* buffer = null)
        {
            Mode = mode;
            DistanceCheckMode = distanceCheckMode;
            MinDeviationRatio = minDeviationRatio;
            MinImproveRatio = minImproveRatio;
            Buffer = buffer;
        }
    }
}
