using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface ISceneDebugOverlay : ISprite
    {
        /// <summary>
        /// Sets the current scene for scene-specific information in the debug overlay.
        /// </summary>
        /// <param name="data"></param>
        void SetScene(SceneRenderData data);

        /// <summary>
        /// Sets the current debug overlay page.
        /// </summary>
        /// <param name="pageID"></param>
        void SetPage(int pageID);

        /// <summary>
        /// Goes to the next overlay page.
        /// </summary>
        void NextPage();

        /// <summary>
        /// Goes to the previous overlay page.
        /// </summary>
        void PreviousPage();

        /// <summary>
        /// Gets or sets whether the debug overlay is visible.
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SpriteFont"/> used when rendering the overlay.
        /// </summary>
        SpriteFont Font { get; set; }
    }
}
