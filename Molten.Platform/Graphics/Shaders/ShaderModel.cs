using System;

namespace Molten.Graphics
{
    /// <summary>
    /// Represents shader model tiers.
    /// </summary>
    [Flags]
    public enum ShaderModel : uint
    {
        Model1_1 = 0,

        /// <summary>Shader model 2.0</summary>
        Model2_0 = 1,

        /// <summary>Shader model 2.0a</summary>
        Model2_0a = 1 << 1,

        /// <summary>Shader model 2.0b</summary>
        Model2_0b = 1 << 2,

        /// <summary>Shader model 3.0</summary>
        Model3_0 = 1 << 3,

        /// <summary>Shader model 4.0</summary>
        Model4_0 = 1 << 4,

        /// <summary>Shader model 4.1</summary>
        Model4_1 = 1 << 5,

        /// <summary>Shader model 5.0</summary>
        Model5_0 = 1 << 6,

        /// <summary>
        /// Shader model 5.1
        /// </summary>
        Model5_1 = 1 << 7,

        /// <summary>
        /// Shader model 6.0. See: https://github.com/microsoft/DirectXShaderCompiler/wiki/Shader-Model-6.0
        /// </summary>
        Model6_0 = 1 << 8,

        /// <summary>
        /// Shader model 6.1. See: https://github.com/microsoft/DirectXShaderCompiler/wiki/Shader-Model-6.1
        /// </summary>
        Model6_1 = 1 << 9,

        /// <summary>
        /// Shader model 6.2. See: https://github.com/microsoft/DirectXShaderCompiler/wiki/Shader-Model-6.2
        /// </summary>
        Model6_2 = 1 << 10,

        /// <summary>
        /// Shader model 6.3. See: https://github.com/microsoft/DirectXShaderCompiler/wiki/Shader-Model-6.3
        /// </summary>
        Model6_3 = 1 << 11,

        /// <summary>
        /// Shader model 6.4. See: https://github.com/microsoft/DirectXShaderCompiler/wiki/Shader-Model-6.4
        /// </summary>
        Model6_4 = 1 << 12,

        /// <summary>
        /// Shader model 6.5. See: https://github.com/microsoft/DirectXShaderCompiler/wiki/Shader-Model-6.5
        /// </summary>
        Model6_5 = 1 << 13,

        /// <summary>
        /// Shader model 6.6. See: https://github.com/microsoft/DirectXShaderCompiler/wiki/Shader-Model-6.6
        /// </summary>
        Model6_6 = 1 << 14
    }
}
