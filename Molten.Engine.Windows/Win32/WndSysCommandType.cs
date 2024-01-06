namespace Molten.Windows32;

public enum WndSysCommandType
{
    /// <summary>
    /// Sizes the window.
    /// </summary>
    SC_SIZE = 0xF000,

    /// <summary>
    /// Moves the window.
    /// </summary>
    SC_MOVE = 0xF010,

    /// <summary>
    /// Minimizes the window.
    /// </summary>
    SC_MINIMIZE = 0xF020,

    /// <summary>
    /// Maximizes the window
    /// </summary>
    SC_MAXIMIZE = 0xF030,

    /// <summary>
    /// Moves to the next window.
    /// </summary>
    SC_NEXTWINDOW = 0xF040,

    /// <summary>
    /// Moves to the previous window
    /// </summary>
    SC_PREVWINDOW = 0xF050,

    /// <summary>
    /// Closes the window.
    /// </summary>
    SC_CLOSE = 0xF060,

    /// <summary>
    /// Scrolls the 
    /// </summary>
    SC_VSCROLL = 0xF070,

    /// <summary>
    /// Scrolls vertically.
    /// </summary>
    SC_HSCROLL = 0xF080,

    /// <summary>
    /// Retrieves the window menu as a result of a mouse click.
    /// </summary>
    SC_MOUSEMENU = 0xF090,

    /// <summary>
    /// Retrieves the window menu as a result of a keystroke. For more information, see the Remarks section.
    /// </summary>
    SC_KEYMENU = 0xF100,

    /// <summary>
    /// SC_ARRANGE is undocumented
    /// </summary>
    SC_ARRANGE = 0xF110,

    /// <summary>
    /// Restores the window.
    /// </summary>
    SC_RESTORE = 0xF120,

    /// <summary>
    /// Activates the start menu
    /// </summary>
    SC_TASKLIST = 0xF130,

    /// <summary>
    /// Executes the screen saver application specified in the [boot] section of the System.ini file.
    /// </summary>
    SC_SCREENSAVE = 0xF140,

    /// <summary>
    /// Activates the window associated with the application-specified hot key. The lParam parameter identifies the window to activate.
    /// </summary>
    SC_HOTKEY = 0xF150,

    /// <summary>
    /// Selects the default item; the user double-clicked the window menu.
    /// </summary>
    SC_DEFAULT = 0xF160,

    /// <summary>
    /// Sets the state of the display. This command supports devices that have power-saving features, such as a battery-powered personal computer.
    /// <para>The lParam parameter can have the following values:
    /// <list type="table">
    /// <item>-1 (the display is powering on)</item>
    /// <item>1 (the display is going to low power)</item>
    /// <item>2 (the display is being shut off)</item>
    /// </list></para>
    /// </summary>
    SC_MONITORPOWER = 0xF170,

    /// <summary>
    /// Changes the cursor to a question mark with a pointer. If the user then clicks a control in the dialog box, the control receives a WM_HELP message.
    /// </summary>
    SC_CONTEXTHELP = 0xF180,

    /// <summary>
    /// Separator value.
    /// </summary>
    SC_SEPARATOR = 0xF00F
}
