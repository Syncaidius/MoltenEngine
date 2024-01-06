namespace Molten.Windows32;

public enum WndMessageType
{
    /// <summary>
    /// Sent when a window is being destroyed. 
    /// It is sent to the window procedure of the window being destroyed after the window is removed from the screen.
    /// This message is sent first to the window being destroyed and then to the child windows (if any) as they are destroyed. 
    /// During the processing of the message, it can be assumed that all child windows still exist.
    /// </summary>
    /// <remarks>See: https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-destroy</remarks>
    WM_DESTROY = 0x0002,

    GWL_WNDPROC = -4,

    DLGC_WANTALLKEYS = 4,

    /// <summary>
    /// Sent to a window after its size has changed.
    /// </summary>
    WM_SIZE = 0x0005,

    /// <summary>
    /// Sent when a window belonging to a different application than the active window is about to be activated. 
    /// <para>The message is sent to the application whose window is being activated and to the application whose window is being deactivated.</para>
    /// </summary>
    WM_ACTIVATEAPP = 0x001C,

    /// <summary>
    /// Ref: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mouseactivate
    /// </summary>
    WM_MOUSEACTIVATE = 0x0021,

    WM_INPUTLANGCHANGE = 0x0051,

    /// <summary>
    /// The WM_DISPLAYCHANGE message is sent to all windows when the display resolution has changed.
    /// </summary>
    WM_DISPLAYCHANGE = 0x007E,

    /// <summary>
    /// Notifies a window that its nonclient area is being destroyed. 
    /// The DestroyWindow function sends the WM_NCDESTROY message to the window following the WM_DESTROY message.
    /// WM_DESTROY is used to free the allocated memory object associated with the window.
    /// </summary>
    /// <remarks>
    /// See: https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-ncdestroy
    /// </remarks>
    WM_NCDESTROY = 0x0082,

    /// <summary>
    /// Sent to a window in order to determine what part of the window corresponds to a particular screen coordinate. 
    /// This can happen, for example, when the cursor moves, when a mouse button is pressed or released, 
    /// or in response to a call to a function such as WindowFromPoint. If the mouse is not captured, 
    /// the message is sent to the window beneath the cursor. 
    /// Otherwise, the message is sent to the window that has captured the mouse.
    /// </summary>
    /// <remarks>
    /// See: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-nchittest
    /// </remarks>
    WM_NCHITTEST = 0x0084,

    WM_GETDLGCODE = 0x0087,

    WM_KEYDOWN = 0x0100,
    WM_KEYUP = 0x0101,
    WM_CHAR = 0x0102,

    WM_IME_COMPOSITION = 0x10f,

    /// <summary>
    /// A window receives this message when the user chooses a command from the Window menu (formerly known as the system or control menu) 
    /// or when the user chooses the maximize button, minimize button, restore button, or close button.
    /// </summary>
    WM_SYSCOMMAND = 0x0112,

    /// <summary>
    /// Sent when a menu is active and the user presses a key that does not correspond to any mnemonic or accelerator key. 
    /// <para>This message is sent to the window that owns the menu.</para>
    /// </summary>
    WM_MENUCHAR = 0x0120,

    /// <summary>
    /// Ref: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mousemove
    /// </summary>
    WM_MOUSEMOVE = 0x0200,

    /// <summary>
    /// Ref: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-lbuttondown
    /// </summary>
    WM_LBUTTONDOWN = 0x0201,

    /// <summary>
    /// Ref: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-lbuttonup
    /// </summary>
    WM_LBUTTONUP = 0x0202,

    /// <summary>
    /// Ref: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-ncxbuttondblclk
    /// </summary>
    WM_LBUTTONDBLCLK = 0x203,

    /// <summary>
    /// Ref: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-rbuttondown
    /// </summary>
    WM_RBUTTONDOWN = 0x0204,

    /// <summary>
    /// Ref: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-rbuttonup
    /// </summary>
    WM_RBUTTONUP = 0x0205,

    /// <summary>
    /// Ref: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-rbuttondblclk
    /// </summary>
    WM_RBUTTONDBLCLK = 0x0206,

    /// <summary>
    /// Ref: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mbuttondown
    /// </summary>
    WM_MBUTTONDOWN = 0x0207,

    /// <summary>
    /// Ref: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mbuttonup
    /// </summary>
    WM_MBUTTONUP = 0x0208,

    /// <summary>
    /// Posted when the user double-clicks the middle mouse button while the cursor is in the client area of a window.
    /// Ref: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mbuttondblclk
    /// </summary>
    WM_MBUTTONDBLCLK = 0x0209,

    /// <summary>
    /// Ref: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mousewheel
    /// </summary>
    WM_MOUSEWHEEL = 0x020A,

    WM_XBUTTONDOWN = 0x020B,

    WM_XBUTTONUP = 0x020C,

    WM_XBUTTONDBLCLK = 0x020D,

    /// <summary>
    /// Sent to the active window when the mouse's horizontal scroll wheel is tilted or rotated. 
    /// The DefWindowProc function propagates the message to the window's parent.
    /// There should be no internal forwarding of the message, 
    /// since DefWindowProc propagates it up the parent chain until it finds a window that processes it.
    /// Ref: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mousehwheel
    /// </summary>
    WM_MOUSEHWHEEL = 0x020E,

    /// <summary>
    /// Sent to the window that is losing the mouse capture.
    /// Ref: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-capturechanged
    /// </summary>
    WM_CAPTURECHANGED = 0x0215,

    /// <summary>
    /// Notifies applications that a power-management event has occurred.
    /// <para>See: https://learn.microsoft.com/en-us/windows/win32/power/wm-powerbroadcast</para>
    /// </summary>
    WM_POWERBROADCAST = 0x0218,

    WM_IME_SETCONTEXT = 0x0281,

    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mousehover
    /// </summary>
    WM_MOUSEHOVER = 0x02A1,
}
