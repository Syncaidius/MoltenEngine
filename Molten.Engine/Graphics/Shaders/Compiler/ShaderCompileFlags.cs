namespace Molten.Graphics;

[Flags]
public enum ShaderCompileFlags
{
    None = 0,

    /// <summary>
    /// The shader source is from an embedded resource file.
    /// </summary>
    EmbeddedFile = 1,

    Debug = 1 << 1,

    SkipValidation = 1 << 2,

    SkipOptimization = 1 << 3,

    WarningsAreErrors = 1 << 4,

    Level0Optimization = 1 << 5,

    Level1Optimization = 1 << 6,

    Level2Optimization = 1 << 7,

    Level3Optimization = 1 << 8,
}
