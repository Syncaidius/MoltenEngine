﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface IMaterial : IShader
    {
        /// <summary>Gets a pass at the specified index.</summary>
        /// <param name="index">The identifier.</param>
        /// <returns></returns>
        IMaterialPass GetPass(int index);

        /// <summary>Gets a pass with the specified name.</summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        IMaterialPass GetPass(string name);

        /// <summary>
        /// sets the default resource to be used when an object does not provide it's own resource for a particular slot.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="slot">The slot number.</param>
        void SetDefaultResource(IShaderResource resource, int slot);

        IShaderResource GetDefaultResource(int slot);

        /// <summary>Gets the number of passes in the material.</summary>
        /// <value>
        /// The pass count.
        /// </value>
        int PassCount { get; }

    }
}
