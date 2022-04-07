using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    internal static class ErrorCorrection
    {
        public unsafe static void msdfErrorCorrectionInner(BitmapRef<float> sdf, MsdfShape shape, MsdfProjection projection, double range, MSDFGeneratorConfig config)
        {
            if (config.ErrorCorrection.Mode == ErrorCorrectionConfig.ErrorCorrectMode.DISABLED)
                return;

            Bitmap<byte> stencilBuffer = null;
            if (config.ErrorCorrection.Buffer == null)
                stencilBuffer = new Bitmap<byte> (1, sdf.Width, sdf.Height);
            BitmapRef<byte> stencil = new BitmapRef<byte>(1);
            stencil.pixels = config.ErrorCorrection.Buffer != null ? config.ErrorCorrection.Buffer : (stencilBuffer == null ? null : stencilBuffer.Ptr);
            stencil.Width = sdf.Width;
            stencil.Height = sdf.Height;
            MSDFErrorCorrection ec = new MSDFErrorCorrection(stencil, projection, range);
            ec.setMinDeviationRatio(config.ErrorCorrection.MinDeviationRatio);
            ec.setMinImproveRatio(config.ErrorCorrection.MinImproveRatio);
            switch (config.ErrorCorrection.Mode) {
                case ErrorCorrectionConfig.ErrorCorrectMode.DISABLED:
                case ErrorCorrectionConfig.ErrorCorrectMode.INDISCRIMINATE:
                    break;
                case ErrorCorrectionConfig.ErrorCorrectMode.EDGE_PRIORITY:
                    ec.protectCorners(shape);
                    ec.protectEdges<N>(sdf);
                    break;
                case ErrorCorrectionConfig.ErrorCorrectMode.EDGE_ONLY:
                    ec.protectAll();
                    break;
            }
            if (config.ErrorCorrection.DistanceCheckMode == ErrorCorrectionConfig.DistanceErrorCheckMode.DO_NOT_CHECK_DISTANCE || 
                (config.ErrorCorrection.DistanceCheckMode == ErrorCorrectionConfig.DistanceErrorCheckMode.CHECK_DISTANCE_AT_EDGE && 
                config.ErrorCorrection.Mode != ErrorCorrectionConfig.ErrorCorrectMode.EDGE_ONLY)) {
                ec.findErrors<N>(sdf);
                if (config.ErrorCorrection.DistanceCheckMode == ErrorCorrectionConfig.DistanceErrorCheckMode.CHECK_DISTANCE_AT_EDGE)
                    ec.protectAll();
            }
            if (config.ErrorCorrection.DistanceCheckMode == ErrorCorrectionConfig.DistanceErrorCheckMode.ALWAYS_CHECK_DISTANCE || 
                config.ErrorCorrection.DistanceCheckMode == ErrorCorrectionConfig.DistanceErrorCheckMode.CHECK_DISTANCE_AT_EDGE)
            {
                if (config.OverlapSupport)
                    ec.findErrors<OverlappingContourCombiner, N>(sdf, shape);
                else
                    ec.findErrors<SimpleContourCombiner, N>(sdf, shape);
            }
            ec.apply(sdf);
        }

        public static void msdfErrorCorrectionShapeless<T>(BitmapRef<float> sdf, MsdfProjection projection, double range, double minDeviationRatio, bool protectAll)
            where T : struct
        {
            Bitmap<byte> stencilBuffer = new Bitmap<byte>(1, sdf.Width, sdf.Height);
            MSDFErrorCorrection ec = new MSDFErrorCorrection(stencilBuffer, projection, range);
            ec.setMinDeviationRatio(minDeviationRatio);
            if (protectAll)
                ec.protectAll();
            ec.findErrors<N>(sdf);
            ec.apply(sdf);
        }
    }
}
