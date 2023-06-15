using System.Text;
using Molten.Utility;
using Silk.NET.Core.Native;

namespace Molten.Graphics.DX11
{
    internal unsafe class ConstantBufferDX11 : BufferDX11, IConstantBuffer
    {
        internal D3DCBufferType Type;
        internal GraphicsConstantVariable[] Variables;
        internal Dictionary<string, GraphicsConstantVariable> _varLookup;
        internal int Hash;
        byte* _constData;

        internal ConstantBufferDX11(DeviceDX11 device, ConstantBufferInfo info)
            : base(device, GraphicsBufferType.Constant, GraphicsResourceFlags.NoShaderAccess | GraphicsResourceFlags.CpuWrite, GraphicsFormat.Unknown, 1, info.Size, null, 0)
        {
            _varLookup = new Dictionary<string, GraphicsConstantVariable>();
            _constData = (byte*)EngineUtil.Alloc(info.Size);

            // Read sdescription data
            BufferName = info.Name;
            Type = (D3DCBufferType)info.Type;

            string hashString = BufferName;
            uint variableCount = (uint)info.Variables.Count;
            Variables = new GraphicsConstantVariable[variableCount];

            // Read all variables from the constant buffer
            for (int c = 0; c < info.Variables.Count; c++)
            {
                ConstantBufferVariableInfo variable = info.Variables[c];
                GraphicsConstantVariable sv = CreateConstantVariable(variable, variable.Name);

                // Throw exception if the variable type is unsupported.
                if (sv == null) // TODO remove this exception!
                    throw new NotSupportedException("Shader pipeline does not support HLSL variables of type: " + variable.Type.Type + " -- " + variable.Type.Class);

                sv.ByteOffset = variable.StartOffset;

                if(variable.DefaultValue != null)
                    sv.ValueFromPtr(variable.DefaultValue);

                _varLookup.Add(sv.Name, sv);
                Variables[c] = sv;

                // Append name to hash.
                hashString += sv.Name;
            }

            // Generate hash for comparing constant buffers.
            byte[] hashData = StringHelper.GetBytes(hashString, Encoding.Unicode);
            Hash = HashHelper.ComputeFNV(hashData);
            Desc.ByteWidth = info.Size;
        }

        protected override void OnGraphicsRelease()
        {
            EngineUtil.Free(ref _constData);
            base.OnGraphicsRelease();
        }

        protected override void OnApply(GraphicsQueue cmd)
        {
            // Setting data via shader variabls takes precedent. All standard buffer changes (set/append) will be ignored and wiped.
            if (IsDirty)
            {
                IsDirty = false;

                // Re-write all data to the variable buffer to maintain byte-ordering.
                foreach(GraphicsConstantVariable v in Variables)
                    v.Write(_constData + v.ByteOffset);

                using (GraphicsStream stream = cmd.MapResource(this, 0, 0, GraphicsMapType.Discard))
                    stream.WriteRange(_constData, Desc.ByteWidth);
            }
            else
            {
                ApplyChanges(cmd);
            }
        }

        public string BufferName { get; }

        public bool IsDirty { get; set; }

        internal byte* DataPtr => _constData;
    }
}
