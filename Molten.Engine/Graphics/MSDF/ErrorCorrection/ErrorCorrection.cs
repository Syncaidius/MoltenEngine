using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public static class ErrorCorrection
    {
        public unsafe static void MsdfErrorCorrection<ES, DT>(ContourCombiner<ES, DT> combiner, TextureSliceRef<float> sdf, MsdfShape shape, MsdfProjection projection, double range, MsdfConfig config)
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
    }
}
