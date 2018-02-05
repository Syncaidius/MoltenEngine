using SharpDX;
using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    public abstract class ShaderConstantVariable : IShaderValue
    {
        ShaderConstantBuffer _parent;

        /// <summary>Gets the byte offset of the variable.</summary>
        internal int ByteOffset;

        /// <summary>The size of the variable's data in bytes.</summary>
        internal int SizeOf;

        internal ShaderConstantVariable(ShaderConstantBuffer parent)
        {
            _parent = parent;
        }

        /// <summary>Marks the parent buffer as dirty.</summary>
        protected void DirtyParent()
        {
            _parent.DirtyVariables = true;
        }

        /// <summary>Gets or sets the value of the variable.</summary>
        public abstract object Value { get; set; }

        /// <summary>Called when the variable's value needs to be written to a buffer.</summary>
        /// <param name="stream">The data stream to write the value(s) into.</param>
        internal abstract void Write(DataStream stream);

        /// <summary>Gets the shader buffer which owns the variable.</summary>
        internal ShaderConstantBuffer ParentBuffer { get { return _parent; } }

        public IShader Parent => _parent.Parent;

        public string Name { get; set; }
    }
}
