using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public interface ISceneObject
    {
        /// <summary>
        /// Gets or sets the scene the <see cref="ISceneObject"/> is attached to. This is automatically set by the scene it is added to, so it is best to avoid setting this manually.
        /// </summary>
        Scene Scene { get; set; }

        /// <summary>
        /// Gets or sets the scene layer the <see cref="ISceneObject"/> is attached to. This is automatically set by the scene it is added to, so avoid setting this manually where possible.
        /// </summary>
        SceneLayer Layer { get; set; }
    }
}
