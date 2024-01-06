using System.Reflection;

namespace Molten.Graphics;

/// <summary>
/// Represents a shader source file.
/// </summary>
public class ShaderSource : EngineObject
{
    string _src;

    public ShaderSource(string filename, in string source, bool isEmbedded, int originalLineCount,
        Assembly assembly, string nameSpace)
    {
        Filename = filename;
        ParentNamespace = nameSpace;
        Dependencies = new List<ShaderSource>();
        IsEmbedded = isEmbedded;
        ParentAssembly = assembly;
        FullFilename = filename;

        string[] lines = source.Split('\n');
        LineCount = lines.Length;
        _src = $"#line 1 \"{filename}\"\n{source}\n#line {originalLineCount} \"{filename}\"";

        if (IsEmbedded)
        {
            if (assembly != null)
            {
                if (string.IsNullOrWhiteSpace(nameSpace))
                    FullFilename = $"{filename}, {assembly}";
                else
                    FullFilename = $"{nameSpace}.{filename}, {assembly}";
            }
        }
    }

    protected override void OnDispose()
    {
        _src = null;
    }

    /// <summary>
    /// Gets the filename that the current <see cref="HlslSource"/> represents.
    /// </summary>
    public string Filename { get; private set; }

    /// <summary>
    /// Gets a reference to the HLSL source code string
    /// </summary>
    public ref string SourceCode => ref _src;

    internal List<ShaderSource> Dependencies { get; }

    /// <summary>
    /// Gets whether or not the source was from an embedded resource.
    /// </summary>
    internal bool IsEmbedded { get; }

    internal Assembly ParentAssembly { get; }

    internal string ParentNamespace { get; }

    internal string FullFilename { get; }

    public int LineCount { get; }
}
