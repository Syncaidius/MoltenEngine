namespace Molten.Graphics
{
    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <unmanaged>D3D11_REGISTER_COMPONENT_MASK_FLAG</unmanaged>
    [Flags]
    internal enum RegisterComponentMaskFlags : byte
    {

        /// <summary>	
        /// No documentation.	
        /// </summary>	
        /// <unmanaged>D3D11_REGISTER_COMPONENT_MASK_ALL</unmanaged>
        All = 15,

        /// <summary>	
        /// No documentation.	
        /// </summary>	
        /// <unmanaged>D3D11_REGISTER_COMPONENT_MASK_COMPONENT_W</unmanaged>
        ComponentW = 8,

        /// <summary>	
        /// No documentation.	
        /// </summary>	
        /// <unmanaged>D3D11_REGISTER_COMPONENT_MASK_COMPONENT_X</unmanaged>
        ComponentX = 1,

        /// <summary>	
        /// No documentation.	
        /// </summary>	
        /// <unmanaged>D3D11_REGISTER_COMPONENT_MASK_COMPONENT_Y</unmanaged>
        ComponentY = 2,

        /// <summary>	
        /// No documentation.	
        /// </summary>	
        /// <unmanaged>D3D11_REGISTER_COMPONENT_MASK_COMPONENT_Z</unmanaged>
        ComponentZ = 4,

        /// <summary>	
        /// None.	
        /// </summary>	
        /// <unmanaged>None</unmanaged>
        None = 0,
    }
}
