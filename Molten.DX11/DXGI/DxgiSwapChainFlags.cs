using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Dxgi
{
    [Flags]
    public enum DxgiSwapChainFlags : uint
    {
        None = 0,

        Nonprerotated = 0x1,

        ///<summary>Set this flag to enable an application to switch modes by calling IDXGISwapChain::ResizeTarget.
        ///     When switching from windowed to full-screen mode, the display mode (or monitor
        ///     resolution) will be changed to match the dimensions of the application window.</summary>    
        AllowModeSwitch = 0x2,

        /// <summary>
        ///     Set this flag to enable an application to render using GDI on a swap chain or
        ///     a surface. This will allow the application to call IDXGISurface1::GetDC on the
        ///     0th back buffer or a surface.</summary>
        GdiCompatible = 0x4,

        ///<summary>
        ///     Set this flag to indicate that the swap chain might contain protected content;
        ///     therefore, the operating system supports the creation of the swap chain only
        ///     when driver and hardware protection is used. If the driver and hardware do not
        ///     support content protection, the call to create a resource for the swap chain
        ///     fails. Direct3D 11:??This enumeration value is supported starting with Windows?8.</summary> 
        RestrictedContent = 0x8,


        ///     <summary>Set this flag to indicate that shared resources that are created within the swap
        ///     chain must be protected by using the driver?s mechanism for restricting access
        ///     to shared surfaces. Direct3D 11:??This enumeration value is supported starting
        ///     with Windows?8.</summary>
        RestrictSharedResourceDriver = 0x10,
        

        /// <summary>
        ///     Set this flag to restrict presented content to the local displays. Therefore,
        ///     the presented content is not accessible via remote accessing or through the desktop
        ///     duplication APIs. This flag supports the window content protection features of
        ///     Windows. Applications can use this flag to protect their own onscreen window
        ///     content from being captured or copied through a specific set of public operating
        ///     system features and APIs. If you use this flag with windowed (System.IntPtr or
        ///     IWindow) swap chains where another process created the System.IntPtr, the owner
        ///     of the System.IntPtr must use the SetWindowDisplayAffinity function appropriately
        ///     in order to allow calls to IDXGISwapChain::Present or IDXGISwapChain1::Present1
        ///     to succeed. Direct3D 11:??This enumeration value is supported starting with Windows?8.
        ///</summary>
        DisplayOnly = 0x20,
        ///
        /// Summary:
        ///     Set this flag to create a waitable object you can use to ensure rendering does
        ///     not begin while a frame is still being presented. When this flag is used, the
        ///     swapchain's latency must be set with the IDXGISwapChain2::SetMaximumFrameLatency
        ///     API instead of IDXGIDevice1::SetMaximumFrameLatency. Note??This enumeration value
        ///     is supported starting with Windows?8.1.
        FrameLatencyWaitAbleObject = 0x40,


        ///<summary>     Set this flag to create a swap chain in the foreground layer for multi-plane
        ///     rendering. This flag can only be used with CoreWindow swap chains, which are
        ///     created with CreateSwapChainForCoreWindow. Apps should not create foreground
        ///     swap chains if IDXGIOutput2::SupportsOverlays indicates that hardware support
        ///     for overlays is not available. Note that IDXGISwapChain::ResizeBuffers cannot
        ///     be used to add or remove this flag. Note??This enumeration value is supported
        ///     starting with Windows?8.1.</summary>    
        ForegroundLayer = 0x80,

        /// <summary>
        ///     Set this flag to create a swap chain for full-screen video. Note??This enumeration
        ///     value is supported starting with Windows?8.1.
        /// </summary>
        FullScreenVideo = 0x100,


        ///     <summary>Set this flag to create a swap chain for YUV video. Note??This enumeration value
        ///     is supported starting with Windows?8.1.
        ///     </summary>
        YuvVideo = 0x200,


        ///     <summary>Indicates that the swap chain should be created such that all underlying resources
        ///     can be protected by the hardware. Resource creation will fail if hardware content
        ///     protection is not supported. This flag has the following restrictions: This flag
        ///     can only be used with swap effect DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL. Note??Creating
        ///     a swap chain using this flag does not automatically guarantee that hardware protection
        ///     will be enabled for the underlying allocation. Some implementations require that
        ///     the DRM components are first initialized prior to any guarantees of protection.
        ///     ? Note??This enumeration value is supported starting with Windows?10.</summary>
        HwProtected = 0x400,


        ///<summary>
        ///Tearing support is a requirement to enable displays that support variable refresh
        ///     rates to function properly when the application presents a swap chain tied to
        ///     a full screen borderless window. Win32 apps can already achieve tearing in fullscreen
        ///     exclusive mode by calling SetFullscreenState(TRUE), but the recommended approach
        ///     for Win32 developers is to use this tearing flag instead. 
        ///     
        /// This flag requires the use of a DXGI_SWAP_EFFECT_FLIP_* swap effect.
        /// </summary>
        /// <remarks>
        /// To check for hardware
        ///     support of this feature, refer to IDXGIFactory5::CheckFeatureSupport. For usage
        ///     information refer to IDXGISwapChain::Present and the DXGI_PRESENT flags.
        /// </remarks>
        AllowTearing = 0x800,

        /// <summary>
        /// No MSDN documentation
        /// </summary>
        RestrictedToAllHolographicDisplayS = 0x1000,
    }
}
