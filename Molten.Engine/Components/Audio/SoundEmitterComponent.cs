namespace Molten.Audio;

public class SoundEmitterComponent : SceneComponent
{
    ISoundSource _sound;

    protected override void OnDispose()
    {
        throw new NotImplementedException();
    }

    public override void OnUpdate(Timing time)
    {
        base.OnUpdate(time);
    }

    public ISoundSource Sound
    {
        get => _sound;
        set
        {
            if (_sound != value)
            {
                if (_sound != null)
                {
                    // TODO unbind old _sound
                }

                _sound = value;

                if (_sound != null)
                {
                    // TODO bind new _sound
                }
            }
        }
    }
}
