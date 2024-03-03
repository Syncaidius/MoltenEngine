using Molten.Graphics;

namespace Molten.Examples;

[Example("Skybox", "Demonstrates the use of a basic skybox")]
public class Skybox : MoltenExample
{
    ContentLoadHandle _hShader;
    ContentLoadHandle _hTexture;

    protected override void OnLoadContent(ContentLoadBatch loader)
    {
        base.OnLoadContent(loader);

        _hShader = loader.Load<Shader>("assets/BasicTexture.json");
        _hTexture = loader.Load<ITexture2D>("assets/dds_dxt5.dds");

        loader.Load<ITextureCube>("assets/cubemap.dds",
            (tex, isReload, handle) => MainScene.SkyboxTeture = tex);

        loader.OnCompleted += Loader_OnCompleted;
    }

    private void Loader_OnCompleted(ContentLoadBatch loader)
    {
        if (!_hShader.HasAsset())
        {
            Close();
            return;
        }

        Shader shader = _hShader.Get<Shader>();
        shader[ShaderBindType.Resource, 0] = _hTexture.Get<ITexture2D>();
        TestMesh.Shader = shader;
    }

    protected override Mesh GetTestCubeMesh()
    {
        return Engine.Renderer.Device.Resources.CreateMesh<CubeArrayVertex>(SampleVertexData.TextureArrayCubeVertices);
    }
}
