using Molten.Graphics;

namespace Molten.Examples;

[Example("Stress Test - Indexed", "A stress test which spawns a large number of rotating cubes, which use an indexed mesh")]
public class StressTestIndexed : MoltenExample
{
    const int CUBE_COUNT = 5000;

    ContentLoadHandle _hShader;
    List<SceneObject> _objects;

    protected override void OnLoadContent(ContentLoadBatch loader)
    {
        base.OnLoadContent(loader);

        _objects = new List<SceneObject>();
        _hShader = loader.Load<Shader>("assets/BasicColor.json");
        loader.OnCompleted += Loader_OnCompleted;
    }

    private void Loader_OnCompleted(ContentLoadBatch loader)
    {
        if (!_hShader.HasAsset())
        {
            Close();
            return;
        }

        TestMesh.Shader = _hShader.Get<Shader>();
        for (int i = 0; i < CUBE_COUNT; i++)
            SpawnRandomTestCube(TestMesh, 70);
    }

    protected override Mesh GetTestCubeMesh()
    {
        return Engine.Renderer.Device.CreateMesh(SampleVertexData.IndexedColorCubeVertices, SampleVertexData.CubeIndices);
    }

    private void SpawnRandomTestCube(Mesh mesh, int spawnRadius)
    {
        int maxRange = spawnRadius * 2;
        SceneObject obj = MainScene.CreateObject(new Vector3F()
        {
            X = -spawnRadius + (float)(Rng.NextDouble() * maxRange),
            Y = -spawnRadius + (float)(Rng.NextDouble() * maxRange),
            Z = spawnRadius + (float)(Rng.NextDouble() * maxRange)
        });
        RenderableComponent meshCom = obj.Components.Add<RenderableComponent>();
        meshCom.RenderedObject = mesh;

        _objects.Add(obj);
    }

    protected override void OnUpdate(Timing time)
    {
        var rotateAngle = 1.2f * time.Delta;
        foreach (SceneObject obj in _objects)
        {
            obj.Transform.LocalRotationX += rotateAngle;
            obj.Transform.LocalRotationY += rotateAngle;
            obj.Transform.LocalRotationZ += rotateAngle * 0.7f * time.Delta;
        }

        base.OnUpdate(time);
    }
}
