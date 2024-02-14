namespace Molten.Graphics;

public class LightMaterialProperties : CommonShaderProperties
{
    public ShaderVariable Data { get; set; }

    public ShaderVariable MapDiffuse { get; set; }

    public ShaderVariable MapNormal { get; set; }

    public ShaderVariable MapDepth { get; set; }

    public ShaderVariable InvViewProjection { get; set; }

    public ShaderVariable CameraPosition { get; set; }

    public LightMaterialProperties(Shader shader) : base(shader)
    {
        Data = MapValue(shader, "LightData");
        MapDiffuse = MapValue(shader, "mapDiffuse");
        MapNormal = MapValue(shader, "mapNormal");
        MapDepth = MapValue(shader, "mapDepth");
        InvViewProjection = MapValue(shader, "invViewProjection");
        CameraPosition = MapValue(shader, "cameraPosition");
    }
}
