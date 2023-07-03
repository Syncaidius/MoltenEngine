namespace Molten.Graphics
{
    public abstract class ShaderVariable
    {
        public static T Create<T>(HlslShader shader, string name)
            where T : ShaderVariable, new()
        {
            T v = new T();
            v.Name = name;
            v.Parent = shader;
            v.Initialize();
            return v;
        }

        protected virtual void Initialize() { }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the <see cref="HlslShader"/> that the current <see cref="ShaderVariable"/> belongs to.
        /// </summary>
        public HlslShader Parent { get; private set; }

        /// <summary>
        /// Gets or sets the value of the current <see cref="ShaderVariable"/>.
        /// </summary>
        public abstract object Value { get; set; }
    }
}
