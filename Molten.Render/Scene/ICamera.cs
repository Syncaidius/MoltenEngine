using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// An interface for scene rendering cameras. These are generally considered as the 'eye' of the scene; The perspective from which the scene will be rendered.
    /// </summary>
    public interface ICamera
    {
        /// <summary>
        /// Gets the view matrix of the camera.
        /// </summary>
        Matrix4F View { get; }

        /// <summary>
        /// Gets the projection matrix of the camera.
        /// </summary>
        Matrix4F Projection { get; }

        /// <summary>
        /// Gets the view-projection matrix of the camera.
        /// </summary>
        Matrix4F ViewProjection { get; }

        /// <summary>
        /// Gets or sets the camera's output surface. That is, the <see cref="IRenderSurface"/> that scenes will be rendered to if their camera was set to the current one.
        /// </summary>
        IRenderSurface OutputSurface { get; set; }

        /// <summary>
        /// Gets or sets the camera's minimum draw distance.
        /// </summary>
        float MinimumDrawDistance { get; set; }

        /// <summary>
        /// Gets or sets the camera's maximum draw distance.
        /// </summary>
        float MaximumDrawDistance { get; set; }

        /// <summary>
        /// Gets or sets the camera's field-of-view (FoV).
        /// </summary>
        float FieldOfView { get; set; }
    }
}
