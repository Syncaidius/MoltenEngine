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

//namespace Molten.Graphics
//{
//    public class BmpTextureWriter : TextureWriter
//    {
//        GraphicsDevice _device;

//        internal BmpTextureWriter(GraphicsDevice device)
//        {
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
//                using (BmpBitmapEncoder encoder = new BmpBitmapEncoder(_device.WICFactory, stream))
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
