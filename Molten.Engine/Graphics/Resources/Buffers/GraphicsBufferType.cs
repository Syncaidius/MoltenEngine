namespace Molten.Graphics
{
    public enum GraphicsBufferType
    {
        Unknown = 0,

        Vertex = 1,

        Index = 2,

        ByteAddress = 3,

        Structured = 4,

        Staging = 5,

        /// <summary>
        /// Constant or uniform buffer
        /// </summary>
        Constant = 6,
    }
}
