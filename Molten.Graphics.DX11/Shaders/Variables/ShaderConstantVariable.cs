namespace Molten.Graphics
{
    public unsafe abstract class ShaderConstantVariable : IShaderValue, IDisposable
    {
        /// <summary>Gets the byte offset of the variable.</summary>
        internal uint ByteOffset;

        /// <summary>The size of the variable's data in bytes.</summary>
        internal uint SizeOf;

        internal ShaderConstantVariable(ShaderConstantBuffer parent)
        {
            ParentBuffer = parent;
        }

        public abstract void Dispose();

        /// <summary>Marks the parent buffer as dirty.</summary>
        protected void DirtyParent()
        {
            ParentBuffer.DirtyVariables = true;
        }

        /// <summary>Gets or sets the value of the variable.</summary>
        public abstract object Value { get; set; }

        /// <summary>Called when the variable's value needs to be written to a buffer.</summary>
        /// <param name="stream">The data stream to write the value(s) into.</param>
        internal abstract void Write(byte* pDest);

        /// <summary>Gets the shader buffer which owns the variable.</summary>
        internal ShaderConstantBuffer ParentBuffer { get; private set; }

        public string Name { get; set; }


    }
}
