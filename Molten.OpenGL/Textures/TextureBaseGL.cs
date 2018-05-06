using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Molten.Graphics
{
    public delegate void TextureEvent(TextureBaseGL texture);

    public abstract class TextureBaseGL : PipelineObjectGL
    {
        /// <summary>Triggered right before the internal texture resource is created.</summary>
        public event TextureEvent OnPreCreate;

        /// <summary>Triggered after the internal texture resource has been created.</summary>
        public event TextureEvent OnCreate;

        /// <summary>Triggered if the creation of the internal texture resource has failed (resulted in a null resource).</summary>
        public event TextureEvent OnCreateFailed;

        int _textureID;
        TextureTarget _target;

        internal TextureBaseGL(TextureTarget target)
        {
            _target = target;
        }

        protected void CreateTexture(bool resize)
        {
            OnPreCreate?.Invoke(this);
            OnDisposeForRecreation();
            _textureID = CreateTextureInternal(resize);

            if (_textureID > 0)
            {
                //TrackAllocation();

                OnCreate?.Invoke(this);
            }
            else
            {
                OnCreateFailed?.Invoke(this);
            }
        }

        protected virtual void OnDisposeForRecreation()
        {
            OnDispose();
        }

        protected override void OnDispose()
        {
            if (_textureID > 0)
            {
                GL.BindTexture(_target, _textureID);
                GL.DeleteTexture(1);
                _textureID = 0;
            }
        }

        protected abstract int CreateTextureInternal(bool isResizing);
    }
}
