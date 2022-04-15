using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public partial class TextureData
    {
        /// <summary>Represents a slice of texture data. This can either be a mip map level or array element in a texture array (which could still technically a mip-map level of 0).</summary>
        public class Slice
        {
            public byte[] Data;

            public uint Pitch;
            public uint TotalBytes;

            public uint Width;
            public uint Height;

            public void OpenReadStream(Action<BinaryReader> readCallback)
            {
                if (Data == null)
                    throw new NullReferenceException("Data cannot be null when opening a read stream");

                using (MemoryStream stream = new MemoryStream(Data))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                        readCallback(reader);
                }
            }

            public void OpenWriteStream(Action<BinaryWriter> writeCallback)
            {
                if (Data == null)
                    throw new NullReferenceException("Data cannot be null when opening a write stream");

                using (MemoryStream stream = new MemoryStream(Data))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                        writeCallback(writer);
                }
            }

            public MemoryStream OpenStream()
            {
                if (Data == null)
                    throw new NullReferenceException("Data cannot be null when opening a memory stream");
                return new MemoryStream(Data);
            }

            public Slice Clone()
            {
                Slice result = new Slice()
                {
                    Data = new byte[this.Data.Length],
                    Pitch = this.Pitch,
                    TotalBytes = this.TotalBytes,
                    Width = this.Width,
                    Height = this.Height,
                };

                Array.Copy(Data, result.Data, TotalBytes);

                return result;
            }
        }
    }
}
