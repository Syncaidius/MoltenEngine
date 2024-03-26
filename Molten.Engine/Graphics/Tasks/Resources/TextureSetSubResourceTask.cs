namespace Molten.Graphics;

public unsafe class TextureSetSubResourceTask : GpuResourceTask<GpuTexture>
{
    public uint MipLevel;

    public byte* Data;

    public uint StartIndex;

    public uint Pitch;

    public uint ArrayIndex;

    public bool Discard;

    public uint NumElements { get; private set; }

    public uint NumBytes { get; private set; }

    public uint Stride { get; private set; }

    public ResourceRegion? Region;

    public void Initialize(void* data, uint stride, uint startIndex, uint numElements)
    {
        Stride = stride;
        NumElements = numElements;
        NumBytes = Stride * NumElements;
        Data = (byte*)EngineUtil.Alloc(NumBytes);

        void* ptrStart = (byte*)data + startIndex;
        Buffer.MemoryCopy(ptrStart, Data, NumBytes, NumBytes);
    }

    public override void ClearForPool()
    {
        MipLevel = 0;
        StartIndex = 0;
        Pitch = 0;
        ArrayIndex = 0;
        Discard = false;
        NumElements = 0;
        NumBytes = 0;
        Stride = 0;
        Region = null;

        EngineUtil.Free(ref Data);
    }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GpuCommandList cmd)
    {
        Resource.SetSubResourceDataImmediate(cmd, MipLevel, Data, StartIndex, NumElements, Pitch, ArrayIndex, Discard, Region);
        EngineUtil.Free(ref Data);
        return true;
    }
}
