using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public static class MsdfRasterization
    {
        public static unsafe void distanceSignCorrection(BitmapRef<float> sdf, MsdfShape shape, MsdfProjection projection, FillRule fillRule) {
            Validation.NPerPixel(sdf, 1);
            Scanline scanline = new Scanline();

            for (int y = 0; y < sdf.Height; ++y) {
                int row = shape.InverseYAxis ? sdf.Height - y - 1 : y;
                shape.scanline(scanline, projection.UnprojectY(y + .5));
                for (int x = 0; x < sdf.Width; ++x) {
                    bool fill = scanline.filled(projection.UnprojectX(x + .5), fillRule);
                    float* sd = sdf[x, row];
                    if ((*sd > 0.5f) != fill)
                        *sd = 1.0f - sd[0];
                }
            }
        }

        static float distVal(float dist, double pxRange, float midValue)
        {
            if (pxRange == 0)
                return (dist > midValue ? 1f : 0);
            return (float)MsdfMath.clamp((dist - midValue) * pxRange + 0.5);
        }

        public static unsafe void renderSDF(BitmapRef<float> output, BitmapRef<float> sdf, double pxRange, float midValue) {
            Validation.NPerPixel(output, 1);
            Validation.NPerPixel(sdf, 1);

            Vector2D scale = new Vector2D((double) sdf.Width/ output.Width, (double)sdf.Height / output.Height);
            pxRange *= (double)(output.Width + output.Height) / (sdf.Width + sdf.Height);
            for (int y = 0; y < output.Height; ++y)
                for (int x = 0; x < output.Width; ++x) {
                    float sd;
                    interpolate(&sd, sdf, scale * new Vector2D(x + .5, y + .5));
                    *output[x, y] = distVal(sd, pxRange, midValue);
                }
        }

        public static unsafe void renderMSDF(BitmapRef<float> output, BitmapRef<float> sdf, double pxRange, float midValue)
        {
            Validation.NPerPixel(output, 3);
            Validation.NPerPixel(sdf, 3);

            Vector2D scale = new Vector2D((double)sdf.Width / output.Width, (double)sdf.Height / output.Height);
            pxRange *= (double)(output.Width + output.Height) / (sdf.Width + sdf.Height);
            for (int y = 0; y < output.Height; ++y)
            {
                for (int x = 0; x < output.Width; ++x)
                {
                    float* sd = stackalloc float[3];
                    interpolate(sd, sdf, scale * new Vector2D(x + 0.5, y + 0.5));
                    output[x, y][0] = distVal(sd[0], pxRange, midValue);
                    output[x, y][1] = distVal(sd[1], pxRange, midValue);
                    output[x, y][2] = distVal(sd[2], pxRange, midValue);
                }
            }
        }

        public unsafe static void multiDistanceSignCorrection(BitmapRef<float> sdf, MsdfShape shape, MsdfProjection projection, FillRule fillRule)
        {
            int w = sdf.Width, h = sdf.Height;
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
                    int row = shape.InverseYAxis ? h - y - 1 : y;
                    shape.scanline(scanline, projection.UnprojectY(y + .5));
                    for (int x = 0; x < w; ++x)
                    {
                        bool fill = scanline.filled(projection.UnprojectX(x + .5), fillRule);
                        float* msd = sdf[x, row];
                        float sd = MsdfMath.median(msd[0], msd[1], msd[2]);
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
                        if (sdf.NPerPixel >= 4 && (msd[3] > .5f) != fill)
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
                        int row = shape.InverseYAxis ? h - y - 1 : y;
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
                                    float* msd = sdf[x, row];
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
        public unsafe static void interpolate(float* output, BitmapRef<float> bitmap, Vector2D pos)
        {
            pos -= .5;
            int l = (int)Math.Floor(pos.X);
            int b = (int)Math.Floor(pos.Y);
            int r = l + 1;
            int t = b + 1;
            double lr = pos.X - l;
            double bt = pos.Y - b;
            l = (int)MsdfMath.clamp(l, bitmap.Width - 1); r = (int)MsdfMath.clamp(r, bitmap.Width - 1);
            b = (int)MsdfMath.clamp(b, bitmap.Height - 1); t = (int)MsdfMath.clamp(t, bitmap.Height - 1);
            for (int i = 0; i < bitmap.NPerPixel; ++i)
                output[i] = MsdfMath.mix(MsdfMath.mix(bitmap[l, b][i], bitmap[r, b][i], lr), MsdfMath.mix(bitmap[l, t][i], bitmap[r, t][i], lr), bt);
        }
    }
}
