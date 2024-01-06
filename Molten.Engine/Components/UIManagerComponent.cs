using Molten.Graphics;
using Molten.Input;

namespace Molten.UI;

/// <summary>
/// A <see cref="SceneComponent"/> used for updating and rendering a UI system into a <see cref="Scene"/>.
/// </summary>
public sealed class UIManagerComponent : SpriteRenderComponent, IPickable<Vector2F>
{       
    UIContainer _root;

    /// <inheritdoc/>
    protected override void OnInitialize(SceneObject obj)
    {
        base.OnInitialize(obj);

        _root = new UIContainer()
        {
            LocalBounds = new Rectangle(0,0,600,480),
            Manager = this,
        };
        Children = _root.Children;
    }

    /// <inheritdoc/>
    protected override void OnDispose()
    {

    }

    /// <inheritdoc/>
    public override void OnUpdate(Timing time)
    {
        base.OnUpdate(time);
        _root.Update(time);
    }

    /// <inheritdoc/>
    protected override void OnRender(SpriteBatcher sb)
    {
        _root.Render(sb);
    }

    /// <inheritdoc/>
    public IPickable<Vector2F> Pick(Vector2F pos, Timing time)
    {
        return _root.Pick(pos, time);
    }

    /// <inheritdoc/>
    public bool OnScrollWheel(InputScrollWheel wheel)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void OnHover(CameraInputTracker tracker)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void OnEnter(CameraInputTracker tracker)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void OnLeave(CameraInputTracker tracker)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void OnPressed(CameraInputTracker tracker)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void OnDoublePressed(CameraInputTracker tracker)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void OnHeld(CameraInputTracker tracker)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void OnDragged(CameraInputTracker tracker)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void OnReleased(CameraInputTracker tracker, bool releasedOutside)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public bool Contains(Vector2F pos)
    {
        throw new NotImplementedException();
    }

    public void Focus()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void Unfocus()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void OnKeyboardInput(KeyboardDevice keyboard, Timing time)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void OnKeyboardChar(KeyboardDevice keyboard, ref KeyboardKeyState state)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void OnKeyDown(KeyboardDevice keyboard, ref KeyboardKeyState state)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void OnKeyUp(KeyboardDevice keyboard, ref KeyboardKeyState state)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets all of the child <see cref="UIElement"/> attached to <see cref="Root"/>. This is an alias propety for <see cref="Root"/>.Children.
    /// </summary>
    public UIElementLayer Children { get; private set; }

    /// <summary>
    /// Gets the root <see cref="UIContainer"/>.
    /// </summary>
    public UIContainer Root => _root;

    /// <inheritdoc/>
    public IPickable<Vector2F> ParentPickable => throw new NotImplementedException();

    /// <inheritdoc/>
    public bool IsFocused { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}
