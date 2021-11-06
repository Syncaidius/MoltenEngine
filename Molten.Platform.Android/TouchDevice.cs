using Molten.Graphics;
using Molten.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public class TouchDevice : AndroidInputDeviceBase<int>, ITouchDevice
    {
        // NOTE SEE: https://github.com/MonoGame/MonoGame/blob/71b25eece3d1b92d6c9f3f32cb51dc054e099133/MonoGame.Framework/Platform/Android/MonoGameAndroidGameView.cs#L142
        //      SEE: https://github.com/MonoGame/MonoGame/blob/71b25eece3d1b92d6c9f3f32cb51dc054e099133/MonoGame.Framework/Platform/Android/AndroidGameActivity.cs
        //      SEE: https://github.com/MonoGame/MonoGame/blob/71b25eece3d1b92d6c9f3f32cb51dc054e099133/MonoGame.Framework/Platform/Android/Input/Touch/AndroidTouchEventManager.cs
        //      SEE: https://github.com/MonoGame/MonoGame/blob/71b25eece3d1b92d6c9f3f32cb51dc054e099133/MonoGame.Framework/Platform/Android/AndroidGameWindow.cs#L62
        public override bool IsConnected => throw new NotImplementedException();

        public override string DeviceName => throw new NotImplementedException();

        public int MaxTouchPoints => throw new NotImplementedException();

        public int TouchPointCount => throw new NotImplementedException();

        public event TouchGestureHandler<Touch2PointGesture> OnPinchGesture;

        public int GetPressedFingerCount()
        {
            throw new NotImplementedException();
        }

        public TouchPointState GetState(int pointID)
        {
            throw new NotImplementedException();
        }

        internal override void Bind(INativeSurface surface)
        {
            throw new NotImplementedException();
        }

        internal override void Unbind(INativeSurface surface)
        {
            throw new NotImplementedException();
        }

        internal override void Update(Timing time)
        {
            throw new NotImplementedException();
        }
    }
}
