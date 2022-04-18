using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public static class ErrorCorrection
    {
        public unsafe static void MsdfErrorCorrectionInner<ES, DT>(ContourCombiner<ES, DT> combiner, TextureSliceRef<float> sdf, MsdfShape shape, MsdfProjection projection, double range, MSDFGeneratorConfig config)
            where ES : EdgeSelector<DT>, new()
            where DT : unmanaged
        {
            if (config.ErrorCorrection.Mode == ErrorCorrectionConfig.ErrorCorrectMode.DISABLED)
                return;

            MSDFErrorCorrection ec = new MSDFErrorCorrection(sdf, projection, range);
            ec.SetMinDeviationRatio(config.ErrorCorrection.MinDeviationRatio);
            ec.SetMinImproveRatio(config.ErrorCorrection.MinImproveRatio);
            switch (config.ErrorCorrection.Mode)
            {
                case ErrorCorrectionConfig.ErrorCorrectMode.DISABLED:
                case ErrorCorrectionConfig.ErrorCorrectMode.INDISCRIMINATE:
                    break;
                case ErrorCorrectionConfig.ErrorCorrectMode.EDGE_PRIORITY:
                    ec.ProtectCorners(shape);
                    ec.ProtectEdges(sdf);
                    break;
                case ErrorCorrectionConfig.ErrorCorrectMode.EDGE_ONLY:
                    ec.ProtectAll();
                    break;
            }
            if (config.ErrorCorrection.DistanceCheckMode == ErrorCorrectionConfig.DistanceErrorCheckMode.DO_NOT_CHECK_DISTANCE ||
                (config.ErrorCorrection.DistanceCheckMode == ErrorCorrectionConfig.DistanceErrorCheckMode.CHECK_DISTANCE_AT_EDGE &&
                config.ErrorCorrection.Mode != ErrorCorrectionConfig.ErrorCorrectMode.EDGE_ONLY))
            {
                ec.FindErrors(sdf);
                if (config.ErrorCorrection.DistanceCheckMode == ErrorCorrectionConfig.DistanceErrorCheckMode.CHECK_DISTANCE_AT_EDGE)
                    ec.ProtectAll();
            }
            if (config.ErrorCorrection.DistanceCheckMode == ErrorCorrectionConfig.DistanceErrorCheckMode.ALWAYS_CHECK_DISTANCE ||
                config.ErrorCorrection.DistanceCheckMode == ErrorCorrectionConfig.DistanceErrorCheckMode.CHECK_DISTANCE_AT_EDGE)
            {
                if (config.OverlapSupport)
                    ec.FindErrors(combiner, sdf, shape);
                else
                    ec.FindErrors(combiner, sdf, shape);
            }
            ec.Apply(sdf);
        }

        public static void MsdfErrorCorrectionShapeless(TextureSliceRef<float> sdf, MsdfProjection projection, double range, double minDeviationRatio, bool protectAll)
        {
            MSDFErrorCorrection ec = new MSDFErrorCorrection(sdf, projection, range);
            ec.SetMinDeviationRatio(minDeviationRatio);

            if (protectAll)
                ec.ProtectAll();

            ec.FindErrors(sdf);
            ec.Apply(sdf);
        }

        public static void MsdfErrorCorrection<ES, DT>(ContourCombiner<ES, DT> combiner, TextureSliceRef<float> sdf, MsdfShape shape, MsdfProjection projection, double range, MSDFGeneratorConfig config)
            where ES : EdgeSelector<DT>, new()
            where DT : unmanaged
        {
            MsdfErrorCorrectionInner(combiner, sdf, shape, projection, range, config);
        }

        public static void MsdfFastDistanceErrorCorrection(TextureSliceRef<float> sdf, MsdfProjection projection, double range, double minDeviationRatio)
        {
            MsdfErrorCorrectionShapeless(sdf, projection, range, minDeviationRatio, false);
        }

        public static void MsdfFastEdgeErrorCorrection(TextureSliceRef<float> sdf, MsdfProjection projection, double range, double minDeviationRatio)
        {
            MsdfErrorCorrectionShapeless(sdf, projection, range, minDeviationRatio, true);
        }
    }
}
