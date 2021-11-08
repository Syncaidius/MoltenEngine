using Android.Views;
using Molten.Graphics;
using Molten.Input;
using Molten.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public class AndroidTouchDevice : AndroidInputDeviceBase<int>, ITouchDevice
    {
        // NOTE SEE: https://github.com/MonoGame/MonoGame/blob/71b25eece3d1b92d6c9f3f32cb51dc054e099133/MonoGame.Framework/Platform/Android/MonoGameAndroidGameView.cs#L142
        //      SEE: https://github.com/MonoGame/MonoGame/blob/71b25eece3d1b92d6c9f3f32cb51dc054e099133/MonoGame.Framework/Platform/Android/AndroidGameActivity.cs
        //      SEE: https://github.com/MonoGame/MonoGame/blob/71b25eece3d1b92d6c9f3f32cb51dc054e099133/MonoGame.Framework/Platform/Android/Input/Touch/AndroidTouchEventManager.cs
        //      SEE: https://github.com/MonoGame/MonoGame/blob/71b25eece3d1b92d6c9f3f32cb51dc054e099133/MonoGame.Framework/Platform/Android/AndroidGameWindow.cs#L62
        public override bool IsConnected => throw new NotImplementedException();

        public override string DeviceName => throw new NotImplementedException();

        public int MaxTouchPoints => 5;

        public int TouchPointCount => 0;

        public event TouchGestureHandler<Touch2PointGesture> OnPinchGesture;

        public event MoltenEventHandler<TouchPointSample> OnTouch;

        AndroidViewSurface _boundSurface;
        Queue<TouchPointSample> _queue = new Queue<TouchPointSample>();

        internal override void Bind(INativeSurface surface)
        {
            if (_boundSurface != surface)
            {
                if (surface is AndroidViewSurface vSurface)
                {
                    vSurface.TargetView.Touch += TargetView_Touch;
                    _boundSurface = vSurface;   
                }
            }

            _boundSurface = null;
        }


        internal override void Unbind(INativeSurface surface)
        {
            if (_boundSurface != null && _boundSurface == surface)
                _boundSurface.TargetView.Touch -= TargetView_Touch;
        }

        public TouchPointState GetState(int pointID)
        {
            throw new NotImplementedException();
        }

        private void TargetView_Touch(object sender, View.TouchEventArgs e)
        {
            // TODO add touch event to queue.
        }

        internal override void Update(Timing time)
        {
            // TODO process touch queue and trigger events accordingly.
            // TODO figure out gestures based on number of pressed touch points.
            // TODO individually track each active touch-point so we can form gestures easily.
        }
    }
}
