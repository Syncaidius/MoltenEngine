using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public static class ErrorCorrection
    {
        public unsafe static void MsdfErrorCorrectionInner<ES, DT>(ContourCombiner<ES, DT> combiner, TextureSliceRef<float> sdf, MsdfShape shape, MsdfProjection projection, double range, MsdfConfig config)
            where ES : EdgeSelector<DT>, new()
            where DT : unmanaged
        {
            if (config.Mode == MsdfConfig.ErrorCorrectMode.DISABLED)
                return;

            MSDFErrorCorrection ec = new MSDFErrorCorrection(sdf, projection, range);
            ec.SetMinDeviationRatio(config.MinDeviationRatio);
            ec.SetMinImproveRatio(config.MinImproveRatio);
            switch (config.Mode)
            {
                case MsdfConfig.ErrorCorrectMode.DISABLED:
                case MsdfConfig.ErrorCorrectMode.INDISCRIMINATE:
                    break;
                case MsdfConfig.ErrorCorrectMode.EDGE_PRIORITY:
                    ec.ProtectCorners(shape);
                    ec.ProtectEdges(sdf);
                    break;
                case MsdfConfig.ErrorCorrectMode.EDGE_ONLY:
                    ec.ProtectAll();
                    break;
            }
            if (config.DistanceCheckMode == MsdfConfig.DistanceErrorCheckMode.DO_NOT_CHECK_DISTANCE ||
                (config.DistanceCheckMode == MsdfConfig.DistanceErrorCheckMode.CHECK_DISTANCE_AT_EDGE &&
                config.Mode != MsdfConfig.ErrorCorrectMode.EDGE_ONLY))
            {
                ec.FindErrors(sdf);
                if (config.DistanceCheckMode == MsdfConfig.DistanceErrorCheckMode.CHECK_DISTANCE_AT_EDGE)
                    ec.ProtectAll();
            }

            if (config.DistanceCheckMode == MsdfConfig.DistanceErrorCheckMode.ALWAYS_CHECK_DISTANCE ||
                config.DistanceCheckMode == MsdfConfig.DistanceErrorCheckMode.CHECK_DISTANCE_AT_EDGE)
            {
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

        public static void MsdfErrorCorrection<ES, DT>(ContourCombiner<ES, DT> combiner, TextureSliceRef<float> sdf, MsdfShape shape, MsdfProjection projection, double range, MsdfConfig config)
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
