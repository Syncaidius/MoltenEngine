using SharpDX.Direct3D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Molten.Graphics
{
    public class SpriteBatcherDX11 : SpriteBatcher
    {
        struct Range
        {
            public int Start;
            public int End;

            public ITexture2D Texture;
            public IMaterial Material;
        }

        Range[] _ranges;
        int _curRange;

        internal SpriteBatcherDX11(int initialCapacity = 3000) : base(initialCapacity)
        {
            _ranges = new Range[100];
        }

        internal void Flush(GraphicsPipe pipe, RenderCamera camera, bool depthSort)
        {
            if (NextID == 0)
                return;

            Sort(camera, depthSort);

            _curRange = 0;
            _ranges[_curRange] = new Range()
            {
                Start = 0,
                Texture = Sprites[0].Texture,
                Material = Sprites[0].Material,
            };

            for (int i = 1; i < NextID; i++)
            {
                if(Sprites[i].Texture != _ranges[_curRange].Texture || Sprites[i].Material != _ranges[_curRange].Material)
                {
                    _ranges[_curRange++].End = i - 1;

                    if (_curRange == _ranges.Length)
                        Array.Resize(ref _ranges, _ranges.Length + (_ranges.Length / 2));

                    _ranges[_curRange] = new Range()
                    {
                        Start = i,
                        Texture = Sprites[i].Texture,
                        Material = Sprites[i].Material,
                    };
                }


                /* TODO:
                 * 1) Accumulate ranges as above and track total vertices accumulated so far
                 * 2) once we hit the max capacity, flush accumulated ranges
                 * 3) use a single buffer map() call to upload accumulated ranges
                 * 4) reset range array back to 0
                 * 5) Accumulate the next batch of ranges
                 * 6) Continue until we reach the end of the sprite array.
                 * 
                 * 
                 */
            }
        }
    }
}
