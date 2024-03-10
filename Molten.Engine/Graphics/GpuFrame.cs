namespace Molten.Graphics;

/// <summary>
/// Contains information and resource provisions for the current frame of a <see cref="GpuDevice"/>.
/// </summary>
public class GpuFrame
{
    /// <summary>
    /// Gets the <see cref="GraphicsBuffer"/> used for staging during the current frame.
    /// </summary>
    public GraphicsBuffer StagingBuffer { get; internal set; }

    /// <summary>
    /// Gets the internal frame ID.
    /// </summary>
    internal ulong FrameID;

    GpuCommandList[] _branches;

    internal GpuFrame(uint initialBranchCount)
    {
        _branches = new GpuCommandList[initialBranchCount];
    }

    public void Track(GpuCommandList cmd)
    {
        if (cmd.BranchIndex == _branches.Length)
            Array.Resize(ref _branches, _branches.Length * 2);

        GpuCommandList last = _branches[cmd.BranchIndex];
        cmd.Previous = last;
        _branches[cmd.BranchIndex] = cmd;
    }

    internal void Reset()
    {
        FrameID = 0;
        BranchCount = 0;

        // Free all command lists in the frame.
        for (int i = 0; i < _branches.Length; i++)
        {
            GpuCommandList q = _branches[i];
            while (q != null)
            {
                GpuCommandList prev = q.Previous;
                q.Free();
                q = prev;
            }
        }

        Array.Clear(_branches);
    }

    internal void Dispose()
    {
        Reset();
        StagingBuffer?.Dispose();
        StagingBuffer = null;
    }

    public uint BranchCount { get; set; }

    public GpuCommandList this[uint index] => _branches[index];
}
