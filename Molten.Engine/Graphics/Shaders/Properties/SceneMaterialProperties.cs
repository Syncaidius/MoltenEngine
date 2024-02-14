namespace Molten.Graphics;

public class SceneMaterialProperties : CommonShaderProperties
{
    public ShaderVariable View { get; private set; }

    public ShaderVariable Projection { get; private set; }

    public ShaderVariable ViewProjection { get; private set; }

    public ShaderVariable InvViewProjection { get; private set; }

    public ShaderVariable MaxSurfaceUV { get; private set; }

    public SceneMaterialProperties(Shader shader) : base(shader)
    {
        View = MapValue(shader, "view");
        Projection = MapValue(shader, "projection");
        ViewProjection = MapValue(shader, "viewProjection");
        InvViewProjection = MapValue(shader, "invViewProjection");
        MaxSurfaceUV = MapValue(shader, "maxSurfaceUV");
    }
}
