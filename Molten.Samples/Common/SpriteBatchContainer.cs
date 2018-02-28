using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    /// <summary>
    /// A helper object for testing or drawing with sprite batch.
    /// </summary>
    public class SpriteBatchContainer : ISprite
    {
        /// <summary>
        /// Called when the renderer draws the object.
        /// </summary>
        public Action<SpriteBatch> OnDraw;

        public void Render(SpriteBatch batch)
        {
            OnDraw?.Invoke(batch);
        }
    }
}
