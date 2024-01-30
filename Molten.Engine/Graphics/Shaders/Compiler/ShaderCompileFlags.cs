namespace Molten.Graphics;

[Flags]
public enum ShaderCompileFlags
{
    None = 0,

    Debug = 1,

    SkipValidation = 1 << 1,

    SkipOptimization = 1 << 2,

    WarningsAreErrors = 1 << 3,

    Level0Optimization = 1 << 4,

    Level1Optimization = 1 << 5,

    Level2Optimization = 1 << 6,

    Level3Optimization = 1 << 7,
}
