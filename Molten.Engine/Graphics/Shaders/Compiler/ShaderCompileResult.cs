namespace Molten.Graphics
{
    public sealed class ShaderCompileResult : EngineObject
    {
        internal Dictionary<ShaderCodeType, List<HlslGraphicsObject>> ShaderGroups { get; } = new Dictionary<ShaderCodeType, List<HlslGraphicsObject>>();

        protected override void OnDispose() { }

        internal void AddResult(ShaderCodeType classType, List<HlslGraphicsObject> result)
        {
            if (result.Count > 0)
            {
                List<HlslGraphicsObject> group = null;
                if (!ShaderGroups.TryGetValue(classType, out group))
                {
                    group = new List<HlslGraphicsObject>();
                    ShaderGroups.Add(classType, group);
                }

                group.AddRange(result);
            }
        }

        /// <summary>
        /// Gets a list of <see cref="HlslGraphicsObject"/> of the specified <see cref="ShaderCodeType"/> which were built successfully.
        /// </summary>
        /// <param name="cType">The shader class type.</param>
        /// <returns></returns>
        public List<HlslGraphicsObject> this[ShaderCodeType cType]
        {
            get => ShaderGroups[cType];
        }

        /// <summary>
        /// Gets a <see cref="HlslGraphicsObject"/> of the specified <see cref="ShaderCodeType"/> and index which was built successfully.
        /// </summary>
        /// <param name="cType">The shader class type.</param>
        /// <param name="index">The index of the element.</param>
        /// <returns></returns>
        public HlslGraphicsObject this[ShaderCodeType cType, int index]
        {
            get
            {
                if (ShaderGroups.TryGetValue(cType, out List<HlslGraphicsObject> group))
                    return group[index];
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets a <see cref="HlslGraphicsObject"/> of the specified <see cref="ShaderCodeType"/> and name which was built successfully.
        /// </summary>
        /// <param name="cType">The shader class type.</param>
        /// <param name="shaderName">The name of the shader given to it it via its XML definition.</param>
        /// <returns></returns>
        public HlslGraphicsObject this[ShaderCodeType cType, string shaderName]
        {
            get
            {
                if (ShaderGroups.TryGetValue(cType, out List<HlslGraphicsObject> group))
                {
                    foreach (HlslGraphicsObject shader in group)
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
