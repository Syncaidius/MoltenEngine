﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public enum GraphicsApi
    {
        /// <summary>
        /// The graphics API is either unsupported or not-known.
        /// </summary>
        Unsupported = 0,

        /// <summary>
        /// DirectX 11.0.
        /// </summary>
        DirectX11_0 = 1,

        /// <summary>
        /// DirectX 11.1 Runtime. Covers DirectX 11.0 to 11.4 feature-sets.
        /// </summary>
        DirectX11_1 = 2,

        /// <summary>
        /// DirectX 12.0.
        /// </summary>
        DirectX12_0 = 3,

        /// <summary>
        /// Vulkan 1.0.
        /// </summary>
        Vulkan1_0 = 32,

        /// <summary>
        /// Vulkan 1.1.
        /// </summary>
        Vulkan1_1 = 33,

        /// <summary>
        /// Vulkan 1.2.
        /// </summary>
        Vulkan1_2 = 34,

        /// <summary>
        /// Vulkan 1.3.
        /// </summary>
        Vulkan1_3 = 35,
    }
}