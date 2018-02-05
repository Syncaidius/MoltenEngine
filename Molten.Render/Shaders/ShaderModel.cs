using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    [Flags]
    public enum ShaderModel
    {
        /// <summary>Shader model 2.0</summary>
        Model2_0 = 0,

        /// <summary>Shader model 2.0a</summary>
        Model2_0a = 1,

        /// <summary>Shader model 2.0b</summary>
        Model2_0b = 2,

        /// <summary>Shader model 3.0</summary>
        Model3_0 = 4,

        /// <summary>Shader model 4.0</summary>
        Model4_0 = 8,

        /// <summary>Shader model 4.1</summary>
        Model4_1 = 16,

        /// <summary>Shader model 5.0</summary>
        Model5_0 = 32,
    }
}
