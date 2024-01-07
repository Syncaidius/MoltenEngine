﻿namespace Molten.Graphics;

/// <summary>A <see cref="RenderSceneChange"/> for adding a <see cref="SceneObject"/> to the root of a scene.</summary>
internal class RenderRemoveScene : GraphicsTask
{
    public SceneRenderData Data;

    public override void ClearForPool()
    {
        Data = null;
    }

    public override void Process(RenderService renderer, GraphicsQueue queue)
    {
        renderer.Scenes.Remove(Data);
    }
}
