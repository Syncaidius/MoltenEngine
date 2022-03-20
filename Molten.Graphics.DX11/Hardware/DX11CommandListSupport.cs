namespace Molten.Graphics
{
    internal enum DX11CommandListSupport
    {
        /// <summary>
        /// No support available for DirectX 11 command lists.
        /// </summary>
        None = 0,

        /// <summary>
        /// The hardware doesn't support DirectX 11 command lists and the feature will be emulated.
        /// </summary>
        Emulated = 1,

        /// <summary>
        /// The hardware fully supports DirectX 11 command lists.
        /// </summary>
        Supported = 2,
    }
}
