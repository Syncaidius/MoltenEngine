//using SharpDX;
//using SharpDX.Direct3D11;
//using SharpDX.DXGI;
//using SharpDX.WIC;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using MapFlags = SharpDX.Direct3D11.MapFlags;

//namespace Molten.Graphics
//{
//    public class PngTextureWriter : TextureWriter
//    {
//        GraphicsDevice _device;
//        bool _enableAlpha;

//        internal PngTextureWriter(GraphicsDevice device, bool enableAlpha)
//        {
//            _enableAlpha = enableAlpha;
//            _device = device;
//        }

//        public override void WriteData(Stream stream, TextureData data)
//        {
//            if (data.IsCompressed)
//            {
//                // Create a copy, so the original is left untouched.
//                data = data.Clone();
//                data.Decompress();
//            }

//            int targetSlice = data.HighestMipMap;
//            int totalBytes = data.Levels[targetSlice].TotalBytes;
//            bool requiresModification = !_enableAlpha || (data.Format == GraphicsFormat.B8G8R8A8_UNorm);

//            byte[] dataArray = null;

//            // If alpha isn't allowed, set all to 255 before continuing.
//            if (requiresModification)
//            {
//                // Create a copy of the data so the original is not altered
//                dataArray = new byte[totalBytes];
//                Array.Copy(data.Levels[targetSlice].Data, dataArray, totalBytes);

//                int totalPixels = data.Width * data.Height;

//                // Set all pixels to full opacity.
//                if (!_enableAlpha)
//                {
//                    for (int i = 0; i < totalPixels; i++)
//                    {
//                        int b = (i * 4) + 3;
//                        dataArray[b] = 255;
//                    }
//                }

//                // Translate BGRA into RGBA
//                if (data.Format == GraphicsFormat.B8G8R8A8_UNorm)
//                {
//                    for (int i = 0; i < totalPixels; i++)
//                    {
//                        int pos = (i * 4);
//                        byte b = dataArray[pos];
//                        byte r = dataArray[pos + 2];

//                        dataArray[pos] = r;
//                        dataArray[pos + 2] = b;
//                    }
//                }
//            }
//            else
//            {
//                dataArray = data.Levels[targetSlice].Data;
//            }

//            // Only save the first level of the first array slice.
//            TextureData.Slice level = data.Levels[data.HighestMipMap];

//            Guid bmpFormat = WICFormatConverter.Convert(data.Format);

//            EngineInterop.PinObject(level.Data, (dataPtr) =>
//            {
//                DataRectangle dataRect = new DataRectangle()
//                {
//                    DataPointer = dataPtr,
//                    Pitch = level.Pitch,
//                };

//                Bitmap bmp = new Bitmap(
//                    _device.WICFactory,
//                    level.Width,
//                    level.Height,
//                    bmpFormat,
//                    dataRect);

//                // Encode into stream
//                using (PngBitmapEncoder encoder = new PngBitmapEncoder(_device.WICFactory, stream))
//                {
//                    using (BitmapFrameEncode bitmapFrameEncode = new BitmapFrameEncode(encoder))
//                    {
//                        bitmapFrameEncode.Initialize();
//                        bitmapFrameEncode.SetSize(bmp.Size.Width, bmp.Size.Height);
//                        var pixelFormat = PixelFormat.FormatDontCare;
//                        bitmapFrameEncode.SetPixelFormat(ref pixelFormat);
//                        bitmapFrameEncode.WriteSource(bmp);
//                        bitmapFrameEncode.Commit();
//                        encoder.Commit();
//                    }
//                }
//            });
//        }

//        protected override void OnDispose()
//        {
            
//        }
//    }
//}
