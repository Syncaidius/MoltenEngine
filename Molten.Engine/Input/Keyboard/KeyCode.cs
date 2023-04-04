namespace Molten.Input
{
    /// <summary>
    /// Represents a key on a keyboard.
    /// </summary>
    public enum KeyCode
    {
        /// <summary>
        /// The "Cancel" key.
        /// </summary>
        Cancel = 0x03,

        /// <summary>
        /// The "Backspace" key.
        /// </summary>
        Backspace = 0x08,

        /// <summary>
        /// The "Clear" key.
        /// </summary>
        Clear = 0x0C,

        /// <summary>
        /// The "Return" key.
        /// </summary>
        Return = 0x0D,

        /// <summary>
        /// The "Shift" key.
        /// </summary>
        Shift = 0x10,

        /// <summary>
        /// The "Control" key.
        /// </summary>
        Control = 0x11,

        /// <summary>
        /// The "Menu" key.
        /// </summary>
        Menu = 0x12,

        /// <summary>
        /// The "Pause" key.
        /// </summary>
        Pause = 0x13,

        /// <summary>
        /// The "Capital" key.
        /// </summary>
        Capital = 0x14,

        /// <summary>
        /// The "Kana" key.
        /// </summary>
        Kana = 0x15,

        /// <summary>
        /// The "Hangeul" key.
        /// </summary>
        Hangeul = 0x15,

        /// <summary>
        /// The "Hangul" key.
        /// </summary>
        Hangul = 0x15,

        /// <summary>
        /// The "Junja" key.
        /// </summary>
        Junja = 0x17,

        /// <summary>
        /// The "Final" key.
        /// </summary>
        Final = 0x18,

        /// <summary>
        /// The "Hanja" key.
        /// </summary>
        Hanja = 0x19,

        /// <summary>
        /// The "Kanji" key.
        /// </summary>
        Kanji = 0x19,

        /// <summary>
        /// The "Escape" key.
        /// </summary>
        Escape = 0x1B,

        /// <summary>
        /// The "Convert" key.
        /// </summary>
        Convert = 0x1C,

        /// <summary>
        /// The "NonConvert" key.
        /// </summary>
        NonConvert = 0x1D,

        /// <summary>
        /// The "Accept" key.
        /// </summary>
        Accept = 0x1E,

        /// <summary>
        /// The "ModeChange" key.
        /// </summary>
        ModeChange = 0x1F,

        /// <summary>
        /// The "Space" key.
        /// </summary>
        Space = 0x20,

        /// <summary>
        /// The "Prior" key.
        /// </summary>
        Prior = 0x21,

        /// <summary>
        /// The "Next" key.
        /// </summary>
        Next = 0x22,

        /// <summary>
        /// The "End" key.
        /// </summary>
        End = 0x23,

        /// <summary>
        /// The "Home" key.
        /// </summary>
        Home = 0x24,

        /// <summary>
        /// The "Left" key.
        /// </summary>
        Left = 0x25,

        Up = 0x26,

        Right = 0x27,

        Down = 0x28,

        Select = 0x29,

        Print = 0x2A,

        Execute = 0x2B,

        Snapshot = 0x2C,

        Insert = 0x2D,

        Delete = 0x2E,

        Help = 0x2F,

        Num0 = 0x30,

        Num1 = 0x31,

        Num2 = 0x32,

        Num3 = 0x33,

        Num4 = 0x34,

        Num5 = 0x35,

        Num6 = 0x36,

        Num7 = 0x37,

        Num8 = 0x38,

        Num9 = 0x39,

        /// <summary>
        /// Represents the letter 'A' key.
        /// </summary>
        A = 0x41,

        /// <summary>
        /// Represents the letter 'B' key.
        /// </summary>
        B = 0x42,

        /// <summary>
        /// Represents the letter 'C' key.
        /// </summary>
        C = 0x43,

        /// <summary>
        /// Represents the letter 'D' key.
        /// </summary>
        D = 0x44,

        /// <summary>
        /// Represents the letter 'E' key.
        /// </summary>
        E = 0x45,

        /// <summary>
        /// Represents the letter 'F' key.
        /// </summary>
        F = 0x46,

        /// <summary>
        /// Represents the letter 'G' key.
        /// </summary>
        G = 0x47,

        /// <summary>
        /// Represents the letter 'H' key.
        /// </summary>
        H = 0x48,

        /// <summary>
        /// Represents the letter 'I' key.
        /// </summary>
        I = 0x49,

        /// <summary>
        /// Represents the letter 'J' key.
        /// </summary>
        J = 0x4A,

        /// <summary>
        /// Represents the letter 'K' key.
        /// </summary>
        K = 0x4B,

        /// <summary>
        /// Represents the letter 'L' key.
        /// </summary>
        L = 0x4C,

        /// <summary>
        /// Represents the letter 'M' key.
        /// </summary>
        M = 0x4D,

        /// <summary>
        /// Represents the letter 'N' key.
        /// </summary>
        N = 0x4E,

        /// <summary>
        /// Represents the letter 'O' key.
        /// </summary>
        O = 0x4F,

        /// <summary>
        /// Represents the letter 'P' key.
        /// </summary>
        P = 0x50,

        /// <summary>
        /// Represents the letter 'Q' key.
        /// </summary>
        Q = 0x51,

        /// <summary>
        /// Represents the letter 'R' key.
        /// </summary>
        R = 0x52,

        /// <summary>
        /// Represents the letter 'S' key.
        /// </summary>
        S = 0x53,

        /// <summary>
        /// Represents the letter 'T' key.
        /// </summary>
        T = 0x54,

        /// <summary>
        /// Represents the letter 'U' key.
        /// </summary>
        U = 0x55,

        /// <summary>
        /// Represents the letter 'V' key.
        /// </summary>
        V = 0x56,

        /// <summary>
        /// Represents the letter 'W' key.
        /// </summary>
        W = 0x57,

        /// <summary>
        /// Represents the letter 'X' key.
        /// </summary>
        X = 0x58,

        /// <summary>
        /// Represents the letter 'Y' key.
        /// </summary>
        Y = 0x59,

        /// <summary>
        /// Represents the letter 'Z' key.
        /// </summary>
        Z = 0x5A,

        LWindows = 0x5B,

        RWindows = 0x5C,

        Apps = 0x5D,

        Power = 0x5E,

        Sleep = 0x5F,

        Numpad0 = 0x60,

        Numpad1 = 0x61,

        Numpad2 = 0x62,

        Numpad3 = 0x63,

        Numpad4 = 0x64,

        Numpad5 = 0x65,

        Numpad6 = 0x66,

        Numpad7 = 0x67,

        Numpad8 = 0x68,

        Numpad9 = 0x69,

        Multiply = 0x6A,

        Add = 0x6B,

        Separator = 0x6C,

        Subtract = 0x6D,

        Decimal = 0x6E,

        Divide = 0x6F,

        F1 = 0x70,

        F2 = 0x71,

        F3 = 0x72,

        F4 = 0x73,

        F5 = 0x74,

        F6 = 0x75,

        F7 = 0x76,

        F8 = 0x77,

        F9 = 0x78,

        F10 = 0x79,

        F11 = 0x7A,

        F12 = 0x7B,

        F13 = 0x7C,

        F14 = 0x7D,

        F15 = 0x7E,

        F16 = 0x7F,

        F17 = 0x80,

        F18 = 0x81,

        F19 = 0x82,

        F20 = 0x83,

        F21 = 0x84,

        F22 = 0x85,

        F23 = 0x86,

        F24 = 0x87,

        Numlock = 0x90,

        Scroll = 0x91,

        LShift = 0xA0,

        RShift = 0xA1,

        LControl = 0xA2,

        RControl = 0xA3,

        LMenu = 0xA4,

        RMenu = 0xA5,

        BrowserBack = 0xA6,

        BrowserForward = 0xA7,

        BrowserRefresh = 0xA8,

        BrowserStop = 0xA9,

        BrowserSearch = 0xAA,

        BrowserFavorites = 0xAB,

        BrowserHome = 0xAC,

        VolumeMute = 0xAD,

        VolumeDown = 0xAE,

        VolumeUp = 0xAF,

        MediaNextTrack = 0xB0,

        MediaPreviousTrack = 0xB1,

        MediaStop = 0xB2,

        MediaPlayPause = 0xB3,

        LaunchMail = 0xB4,

        LaunchMediaSelect = 0xB5,

        LaunchApp1 = 0xB6,

        LaunchApp2 = 0xB7,

        Oem1 = 0xBA,

        OemSemiColon = 0xBA,

        OemPlus = 0xBB,

        OemComma = 0xBC,

        OemMinus = 0xBD,

        OemPeriod = 0xBE,

        Oem2 = 0xBF,

        Oem3 = 0xC0,

        Oem4 = 0xDB,

        Oem5 = 0xDC,

        Oem6 = 0xDD,

        Oem7 = 0xDE,

        Oem8 = 0xDF,

        Oem102 = 0xE2,

        ProcessKey = 0xE5,

        Packet = 0xE7,

        Attn = 0xF6,

        Crsel = 0xF7,

        Exsel = 0xF8,

        Ereof = 0xF9,

        Play = 0xFA,

        Zoom = 0xFB,

        NoName = 0xFC,

        Pa1 = 0xFD,

        OemClear = 0xFE,
    }
}
