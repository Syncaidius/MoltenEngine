using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface ILightData
    {
        /// <summary>
        /// Gets or sets the light color.
        /// </summary>
        Color Color { get; set; }

        /// <summary>
        /// Called when the light is to be removed. Usually this sets a value in the light data to make the GPU/material/renderer skip it during rendering, until it can be replaced with a new light.
        /// </summary>
        void Remove();
    }
}
