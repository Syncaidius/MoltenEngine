namespace Molten.Graphics
{
    public sealed class ShaderCompileResult : EngineObject
    {
        List<HlslShader> _shaders = new List<HlslShader>();
        Dictionary<string, HlslShader> _shadersByName = new Dictionary<string, HlslShader>();

        internal void AddShader(HlslShader shader)
        {
            _shaders.Add(shader);
            _shadersByName.Add(shader.Name.ToLower(), shader);
        }

        protected override void OnDispose() { }

        /// <summary>
        /// Gets a <see cref="HlslGraphicsObject"/> of the specified name which was built successfully.
        /// </summary>
        /// <param name="shaderName">The name of the shader given to it it via its XML definition.</param>
        /// <returns></returns>
        public HlslShader this[string shaderName]
        {
            get
            {
                _shadersByName.TryGetValue(shaderName.ToLower(), out HlslShader shader);
                return shader;
            }
        }

        public HlslShader this[int index] => _shaders[index];
    }
}
