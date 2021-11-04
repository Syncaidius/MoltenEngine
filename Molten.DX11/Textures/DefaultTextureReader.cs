//using SharpDX;
//using SharpDX.Direct3D11;
//using SharpDX.DXGI;
//using SharpDX.WIC;
//using Molten.IO;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;

//namespace Molten.Graphics
//{
//    public class DefaultTextureReader : TextureReader
//    {
//        GraphicsDevice _device;
//        Bitmap _bmp;
//        int _stride;
//        int _byteSize;

//        internal DefaultTextureReader(GraphicsDevice device)
//        {
//            _device = device;
//        }

//        public override void Read(BinaryReader reader)
//        {
//            using (BitmapDecoder bitmapDecoder = new BitmapDecoder(_device.WICFactory, reader.BaseStream, DecodeOptions.CacheOnLoad))
//            {
//                using (FormatConverter formatConverter = new FormatConverter(_device.WICFactory))
//                {
//                    formatConverter.Initialize(
//                        bitmapDecoder.GetFrame(0),
//                        SharpDX.WIC.PixelFormat.Format32bppRGBA,
//                        SharpDX.WIC.BitmapDitherType.None,
//                        null,
//                        0.0,
//                        SharpDX.WIC.BitmapPaletteType.Custom);

//                    _bmp = new Bitmap(_device.WICFactory, formatConverter, BitmapCreateCacheOption.CacheOnLoad);

//                    // Allocate DataStream to receive the WIC image pixels
//                    _stride = _bmp.Size.Width * 4;
//                    _byteSize = _bmp.Size.Height * _stride;

//                    using (var buffer = new SharpDX.DataStream(_byteSize, true, true))
//                    {
//                        // Copy the content of the WIC to the buffer
//                        _bmp.CopyPixels(_stride, buffer);

//                        byte[] data = new byte[_byteSize];
//                        buffer.Read(data, 0, _byteSize);

//                        LevelData = new TextureData.Slice[1] {
//                                new TextureData.Slice() {
//                                    Data = data,
//                                    Pitch = _stride,
//                                    TotalBytes = _byteSize,
//                                    Width = _bmp.Size.Width,
//                                    Height = _bmp.Size.Height,
//                                }
//                            };
//                    }
//                }
//            }
//        }

//        public override TextureData GetData()
//        {
//            return new TextureData()
//            {
//                Format = Format.R8G8B8A8_UNorm.FromApi(),
//                Width = _bmp.Size.Width,
//                Height = _bmp.Size.Height,
//                Flags = TextureFlags.None,
//                ArraySize = 1,
//                MipMapCount = 1,
//                Levels = LevelData,
//            };
//        }

//        public override void Dispose()
//        {
//            _bmp?.Dispose();
//            base.Dispose();
//        }
//    }
//}
