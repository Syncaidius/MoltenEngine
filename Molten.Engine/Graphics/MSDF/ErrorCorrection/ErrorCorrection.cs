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

            TextureSlice stencilBuffer = null;
            if (config.ErrorCorrection.Buffer == null)
            {
                stencilBuffer = new TextureSlice(sdf.Width, sdf.Height, sdf.Width * sdf.Height)
                {
                    ElementsPerPixel = 1
                };
            }

            byte* sliceData = config.ErrorCorrection.Buffer != null ? config.ErrorCorrection.Buffer : (stencilBuffer == null ? null : stencilBuffer.Data);
            TextureSlice stencil = new TextureSlice(sdf.Width, sdf.Height, sliceData, sdf.Width * sdf.Height)
            {
                ElementsPerPixel = 1
            };

            MSDFErrorCorrection ec = new MSDFErrorCorrection(stencil, projection, range);
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
            TextureSlice stencilBuffer = new TextureSlice(sdf.Width, sdf.Height, sdf.Width * sdf.Height);
            MSDFErrorCorrection ec = new MSDFErrorCorrection(stencilBuffer, projection, range);
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
