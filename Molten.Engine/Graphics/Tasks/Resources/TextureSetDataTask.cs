namespace Molten.Graphics;
internal unsafe class TextureSetDataTask : GpuResourceTask<GpuTexture>
{
    public TextureData Data;
    internal bool Discard;
    internal uint DestArrayIndex;
    internal uint DestLevelIndex;
    internal uint ArrayCount;
    internal uint LevelCount;
    internal uint ArrayStartIndex;
    internal uint LevelStartIndex;

    public override void ClearForPool()
    {
        // Clear all fields back to default values
        Data = null;
        Discard = false;
        DestArrayIndex = 0;
        DestLevelIndex = 0;
        ArrayCount = 0;
        LevelCount = 0;
        ArrayStartIndex = 0;
        LevelStartIndex = 0;
    }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GpuCommandList cmd)
    {
        Resource.SetDataImmediate(cmd, Data, LevelStartIndex, ArrayStartIndex, LevelCount, ArrayCount, DestLevelIndex, DestArrayIndex, Discard);        
        return true;
    }
}
