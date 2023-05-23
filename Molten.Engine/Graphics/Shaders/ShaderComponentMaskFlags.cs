namespace Molten.Graphics
{
    /// <summary>	
    /// No documentation.	
    /// </summary>	
    /// <unmanaged>D3D11_REGISTER_COMPONENT_MASK_FLAG</unmanaged>
    [Flags]
    public enum ShaderComponentMaskFlags : byte
    {
        /// <summary>	
        /// None.	
        /// </summary>	
        /// <unmanaged>None</unmanaged>
        None = 0,

        /// <summary>	
        /// No documentation.	
        /// </summary>	
        /// <unmanaged>D3D11_REGISTER_COMPONENT_MASK_COMPONENT_X</unmanaged>
        ComponentX = 1,

        /// <summary>	
        /// No documentation.	
        /// </summary>	
        /// <unmanaged>D3D11_REGISTER_COMPONENT_MASK_COMPONENT_Y</unmanaged>
        ComponentY = 1 << 1,

        /// <summary>	
        /// No documentation.	
        /// </summary>	
        /// <unmanaged>D3D11_REGISTER_COMPONENT_MASK_COMPONENT_Z</unmanaged>
        ComponentZ = 1 << 2,

        /// <summary>	
        /// No documentation.	
        /// </summary>	
        /// <unmanaged>D3D11_REGISTER_COMPONENT_MASK_COMPONENT_W</unmanaged>
        ComponentW = 1 << 3,

        /// <summary>	
        /// No documentation.	
        /// </summary>	
        /// <unmanaged>D3D11_REGISTER_COMPONENT_MASK_ALL</unmanaged>
        All = ComponentX | ComponentY | ComponentZ | ComponentW,
    }
}
