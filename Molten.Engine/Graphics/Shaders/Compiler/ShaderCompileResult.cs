namespace Molten.Graphics
{
    public sealed class ShaderCompileResult : EngineObject
    {
        internal Dictionary<ShaderCodeType, List<HlslElement>> ShaderGroups { get; } = new Dictionary<ShaderCodeType, List<HlslElement>>();

        protected override void OnDispose() { }

        internal void AddResult(ShaderCodeType classType, List<HlslElement> result)
        {
            if (result.Count > 0)
            {
                List<HlslElement> group = null;
                if (!ShaderGroups.TryGetValue(classType, out group))
                {
                    group = new List<HlslElement>();
                    ShaderGroups.Add(classType, group);
                }

                group.AddRange(result);
            }
        }

        /// <summary>
        /// Gets a list of <see cref="HlslElement"/> of the specified <see cref="ShaderCodeType"/> which were built successfully.
        /// </summary>
        /// <param name="cType">The shader class type.</param>
        /// <returns></returns>
        public List<HlslElement> this[ShaderCodeType cType]
        {
            get => ShaderGroups[cType];
        }

        /// <summary>
        /// Gets a <see cref="HlslElement"/> of the specified <see cref="ShaderCodeType"/> and index which was built successfully.
        /// </summary>
        /// <param name="cType">The shader class type.</param>
        /// <param name="index">The index of the element.</param>
        /// <returns></returns>
        public HlslElement this[ShaderCodeType cType, int index]
        {
            get
            {
                if (ShaderGroups.TryGetValue(cType, out List<HlslElement> group))
                    return group[index];
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets a <see cref="HlslElement"/> of the specified <see cref="ShaderCodeType"/> and name which was built successfully.
        /// </summary>
        /// <param name="cType">The shader class type.</param>
        /// <param name="shaderName">The name of the shader given to it it via its XML definition.</param>
        /// <returns></returns>
        public HlslElement this[ShaderCodeType cType, string shaderName]
        {
            get
            {
                if (ShaderGroups.TryGetValue(cType, out List<HlslElement> group))
                {
                    foreach (HlslElement shader in group)
                    {
                        if (shader.Name == shaderName)
                            return shader;
                    }
                }

                return null;
            }
        }
    }
}
