using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A implementation of <see cref="ISpriteBatch"/> which does not draw anything. Instead draw calls are cached until the cache is reset."/></summary>
    public class SpriteBatchCache : SpriteBatchBase, ISpriteBatch
    {
        public SpriteBatchCache() { }
    }
}
