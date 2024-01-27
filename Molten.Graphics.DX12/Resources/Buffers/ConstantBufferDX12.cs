using Silk.NET.Core.Native;

namespace Molten.Graphics.DX12;

internal unsafe class ConstantBufferDX12 : BufferDX12, IConstantBuffer, IEquatable<ConstantBufferDX12>
{
    internal D3DCBufferType Type;
    internal GraphicsConstantVariable[] Variables;
    internal Dictionary<string, GraphicsConstantVariable> _varLookup;
    byte* _constData;

    internal ConstantBufferDX12(DeviceDX12 device, ConstantBufferInfo info)
        : base(device, 1, info.Size, GraphicsResourceFlags.DenyShaderAccess | GraphicsResourceFlags.CpuWrite, GraphicsBufferType.Constant, 1)
    {
        _varLookup = new Dictionary<string, GraphicsConstantVariable>();
        _constData = (byte*)EngineUtil.Alloc(info.Size);

        // Read sdescription data
        BufferName = info.Name;
        Type = (D3DCBufferType)info.Type;

        Variables = ConstantBufferInfo.BuildBufferVariables(this, info);
        foreach (GraphicsConstantVariable v in Variables)
            _varLookup.Add(v.Name, v);
    }

    public bool Equals(ConstantBufferDX12 other)
    {
        return GraphicsConstantVariable.AreEqual(Variables, other.Variables);
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
            foreach (GraphicsConstantVariable v in Variables)
                v.Write(_constData + v.ByteOffset);

            using (GraphicsStream stream = cmd.MapResource(this, 0, 0, GraphicsMapType.Discard))
                stream.WriteRange(_constData, SizeInBytes);
        }

        base.OnApply(cmd);
    }

    public string BufferName { get; }

    public bool IsDirty { get; set; }

    internal byte* DataPtr => _constData;
}
