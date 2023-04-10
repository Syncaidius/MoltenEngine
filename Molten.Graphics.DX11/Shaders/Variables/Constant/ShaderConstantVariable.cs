namespace Molten.Graphics.DX11
{
    public unsafe abstract class ShaderConstantVariable : ShaderVariable, IDisposable
    {
        /// <summary>Gets the byte offset of the variable.</summary>
        internal uint ByteOffset;

        /// <summary>The size of the variable's data in bytes.</summary>
        internal uint SizeOf;

        internal ShaderConstantVariable(ShaderConstantBuffer parent, string name)
        {
            ParentBuffer = parent;
            Name = name;
        }

        public abstract void ValueFromPtr(void* ptr);

        public abstract void Dispose();

        /// <summary>Marks the parent buffer as dirty.</summary>
        protected void DirtyParent()
        {
            ParentBuffer.DirtyVariables = true;
        }

        /// <summary>Called when the variable's value needs to be written to a buffer.</summary>
        /// <param name="stream">The data stream to write the value(s) into.</param>
        internal abstract void Write(byte* pDest);

        /// <summary>Gets the shader buffer which owns the variable.</summary>
        internal ShaderConstantBuffer ParentBuffer { get; private set; }
    }
}
