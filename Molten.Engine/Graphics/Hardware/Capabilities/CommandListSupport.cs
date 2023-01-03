namespace Molten.Graphics
{
    public enum CommandListSupport
    {
        /// <summary>
        /// No support available for command lists/deferred contexts.
        /// </summary>
        None = 0,

        /// <summary>
        /// The hardware doesn't support command lists/deferred contexts and the feature will be emulated.
        /// </summary>
        Emulated = 1,

        /// <summary>
        /// The hardware fully supports DirectX 11 command lists/deferred contexts.
        /// </summary>
        Supported = 2,
    }
}
