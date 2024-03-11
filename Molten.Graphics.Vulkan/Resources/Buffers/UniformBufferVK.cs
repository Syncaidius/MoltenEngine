namespace Molten.Graphics.Vulkan;

public unsafe class UniformBufferVK : BufferVK, IConstantBuffer, IEquatable<UniformBufferVK>
{
    internal bool DirtyVariables;

    Dictionary<string, GraphicsConstantVariable> _varLookup;
    byte* _constData;

    internal UniformBufferVK(GpuDevice device, ConstantBufferInfo info) : 
        base(device, GpuBufferType.Constant, GpuResourceFlags.DenyShaderAccess | GpuResourceFlags.CpuWrite, 1, info.Size, 1)
    {
        _varLookup = new Dictionary<string, GraphicsConstantVariable>();
        _constData = (byte*)EngineUtil.Alloc(info.Size);

        // Read sdescription data
        BufferName = info.Name;

        Variables = ConstantBufferInfo.BuildBufferVariables(this, info);
        foreach(GraphicsConstantVariable v in Variables)
            _varLookup.Add(v.Name, v);
    }

    public bool Equals(UniformBufferVK other)
    {
        return GraphicsConstantVariable.AreEqual(Variables, other.Variables);
    }

    protected override void OnApply(GpuCommandQueue cmd)
    {
        // Setting data via shader variabls takes precedent. All standard buffer changes (set/append) will be ignored and wiped.
        if (IsDirty)
        {
            IsDirty = false;

            // Re-write all data to the variable buffer to maintain byte-ordering.
            foreach (GraphicsConstantVariable v in Variables)
                v.Write(_constData + v.ByteOffset);

            using (GpuStream stream = cmd.MapResource(this, 0, 0, GpuMapType.Discard))
                stream.WriteRange(_constData, (uint)SizeInBytes);
        }


        base.OnApply(cmd);
    }

    public string BufferName { get; }

    public bool IsDirty { get; set; }

    public GraphicsConstantVariable[] Variables { get; }
}
