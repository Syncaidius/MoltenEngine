using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A implementation of <see cref="SpriteBatch"/> which does not draw anything. Instead draw calls are stored until the cache is reset."/></summary>
    public class SpriteBatchCache : SpriteBatch
    {
        public SpriteBatchCache()
        {
            Reset();
        }

        protected override void Reset()
        {
            base.Reset();
            ConfigureNewClip(new Rectangle(0, 0, int.MaxValue, int.MaxValue), false);
        }
    }
}
