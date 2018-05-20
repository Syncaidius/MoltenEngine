using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
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

        /// <summary>
        /// Creates a new instance of <see cref="SpriteBatchContainer"/>.
        /// </summary>
        public SpriteBatchContainer() { }

        /// <summary>
        /// Creates a new instance of <see cref="SpriteBatchContainer"/> and sets its callback.
        /// </summary>
        /// <param name="initialCallback">The callback to be set.</param>
        public SpriteBatchContainer(Action<SpriteBatch> initialCallback)
        {
            OnDraw = initialCallback;
        }

        public void Render(SpriteBatch batch)
        {
            OnDraw?.Invoke(batch);
        }
    }
}
