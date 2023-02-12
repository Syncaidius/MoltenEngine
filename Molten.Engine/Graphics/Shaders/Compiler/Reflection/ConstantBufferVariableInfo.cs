namespace Molten.Graphics
{
    public unsafe class ConstantBufferVariableInfo
    {
        /// <summary>
        /// A pointer to the default value for initializing the variable.
        /// </summary>
        public void* DefaultValue;

        /// <summary>
        /// The offset from the start of the parent constant-buffer, in bytes.
        /// </summary>
        public uint StartOffset;

        /// <summary>
        /// The size of the variable value in bytes.
        /// </summary>
        public uint Size;

        /// <summary>
        /// The offset from the start of the variable to the beginning of the sampler.
        /// </summary>
        public uint StartSampler;

        /// <summary>
        /// The size of the sampler, in bytes.
        /// </summary>
        public uint SamplerSize;

        /// <summary>
        /// The offset from the start of the variable to the beginning of the texture
        /// </summary>
        public uint StartTexture;

        /// <summary>
        /// The size of the texture in bytes.
        /// </summary>
        public uint TextureSize;

        /// <summary>
        /// The name of the variable.
        /// </summary>
        public string Name;

        /// <summary>
        /// A combination of <see cref="ShaderVariableFlags"/> values that are combined by using a bitwise OR operation. The resulting value identifies shader-variable properties.
        /// </summary>
        public ShaderVariableFlags Flags;

        /// <summary>
        /// Gets info representing the underlying data-type of the variable.
        /// </summary>
        public ShaderTypeInfo Type { get; } = new ShaderTypeInfo();
    }
}
