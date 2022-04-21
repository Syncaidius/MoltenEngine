using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public static class MsdfRasterization
    {
        public static unsafe void distanceSignCorrection(TextureSliceRef<float> sdf, ContourShape shape, MsdfProjection projection, FillRule fillRule)
        {
            Validation.NPerPixel(sdf, 1);
            Scanline scanline = new Scanline();

            for (int y = 0; y < sdf.Height; ++y)
            {
                MsdfShapeProcessing.scanline(shape, scanline, projection.UnprojectY(y + .5));
                for (int x = 0; x < sdf.Width; ++x)
                {
                    bool fill = scanline.Filled(projection.UnprojectX(x + .5), fillRule);
                    float* sd = sdf[x, y];
                    if ((*sd > 0.5f) != fill)
                        *sd = 1.0f - sd[0];
                }
            }
        }

        static float distVal(float dist, double pxRange, float midValue)
        {
            if (pxRange == 0)
                return (dist > midValue ? 1f : 0);
            return (float)MathHelperDP.Clamp((dist - midValue) * pxRange + 0.5);
        }

        public static unsafe void RenderSDF(TextureSliceRef<float> output, TextureSliceRef<float> sdf, double pxRange, float midValue)
        {
            Vector2D scale = new Vector2D((double)sdf.Width / output.Width, (double)sdf.Height / output.Height);
            pxRange *= (double)(output.Width + output.Height) / (sdf.Width + sdf.Height);
            for (int y = 0; y < output.Height; ++y)
            {
                for (int x = 0; x < output.Width; ++x)
                {
                    float* sd = stackalloc float[(int)sdf.ElementsPerPixel];
                    Interpolate(sd, sdf, scale * new Vector2D(x + 0.5, y + 0.5));
                    for (uint i = 0; i < output.ElementsPerPixel; i++)
                        output[x, y][i] = distVal(sd[i], pxRange, midValue);
                }
            }
        }

        public unsafe static void multiDistanceSignCorrection(TextureSliceRef<float> sdf, ContourShape shape, MsdfProjection projection, FillRule fillRule)
        {
            uint w = sdf.Width, h = sdf.Height;
            if ((w * h) == 0)
                return;
            Scanline scanline = new Scanline();
            bool ambiguous = false;
            sbyte[] matchMap = new sbyte[w * h];

            fixed (sbyte* ptrMap = matchMap)
            {
                sbyte* match = ptrMap;

                for (int y = 0; y < h; ++y)
                {
                    MsdfShapeProcessing.scanline(shape, scanline, projection.UnprojectY(y + .5));
                    for (int x = 0; x < w; ++x)
                    {
                        bool fill = scanline.Filled(projection.UnprojectX(x + .5), fillRule);
                        float* msd = sdf[x, y];
                        float sd = MathHelper.Median(msd[0], msd[1], msd[2]);
                        if (sd == .5f)
                            ambiguous = true;
                        else if ((sd > .5f) != fill)
                        {
                            msd[0] = 1.0f - msd[0];
                            msd[1] = 1.0f - msd[1];
                            msd[2] = 1.0f - msd[2];
                            *match = (sbyte)-1;
                        }
                        else
                            *match = (sbyte)1;
                        if (sdf.ElementsPerPixel >= 4 && (msd[3] > .5f) != fill)
                            msd[3] = 1.0f - msd[3];
                        ++match;
                    }
                }
                // This step is necessary to avoid artifacts when whole shape is inverted
                if (ambiguous)
                {
                    match = &ptrMap[0];
                    for (int y = 0; y < h; ++y)
                    {
                        for (int x = 0; x < w; ++x)
                        {
                            if (*match == 0)
                            {
                                int neighborMatch = 0;
                                if (x > 0) neighborMatch += *(match - 1);
                                if (x < w - 1) neighborMatch += *(match + 1);
                                if (y > 0) neighborMatch += *(match - w);
                                if (y < h - 1) neighborMatch += *(match + w);
                                if (neighborMatch < 0)
                                {
                                    float* msd = sdf[x, y];
                                    msd[0] = 1.0f - msd[0];
                                    msd[1] = 1.0f - msd[1];
                                    msd[2] = 1.0f - msd[2];
                                }
                            }
                            ++match;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="output"></param>
        /// <param name="bitmap"></param>
        /// <param name="pos"></param>
        public unsafe static void Interpolate(float* output, TextureSliceRef<float> bitmap, Vector2D pos)
        {
            pos -= .5;
            int l = (int)Math.Floor(pos.X);
            int b = (int)Math.Floor(pos.Y);
            int r = l + 1;
            int t = b + 1;
            double lr = pos.X - l;
            double bt = pos.Y - b;

            l = (int)MathHelper.Clamp(l, 0, bitmap.Width - 1); 
            r = (int)MathHelper.Clamp(r, 0, bitmap.Width - 1);
            b = (int)MathHelper.Clamp(b, 0, bitmap.Height - 1); 
            t = (int)MathHelper.Clamp(t, 0, bitmap.Height - 1);

            for (int i = 0; i < bitmap.ElementsPerPixel; ++i)
            {
                float start = MathHelper.Lerp(bitmap[l, b][i], bitmap[r, b][i], lr);
                float end = MathHelper.Lerp(bitmap[l, t][i], bitmap[r, t][i], lr);
                output[i] = MathHelper.Lerp(start, end, bt);
            }
        }
    }
}
