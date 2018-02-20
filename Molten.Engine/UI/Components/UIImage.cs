using Molten.IO;
using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Molten.UI
{
    /// <summary>A UI component capable of displaying a texture.</summary>
    public class UIImage : UIComponent
    {
        string _sourceFile;
        bool _surpassSourceUpdate;

        ITexture2D _texture;
        Rectangle _source;
        Color _color;

        /// <summary>Creates a new instance of <see cref="UIImage"/>.</summary>
        /// <param name="ui">The UI system to bind to.</param>
        public UIImage(Engine engine) : base(engine)
        {
            _color = new Color(255, 255, 255, 255);
            _source = new Rectangle(0, 0, 1, 1);
        }

        private void GetSourceFile()
        {
            ContentRequest cr = _engine.Content.StartRequest();
            cr.Load<ITexture2D>(_sourceFile);
            cr.OnCompleted += request_OnCompleted;
            cr.Commit();
        }

        void request_OnCompleted(ContentManager content, ContentRequest request)
        {
            _texture = content.Get<ITexture2D>(request.RequestedFiles[0]);

            if (!_surpassSourceUpdate)
                _source = new Rectangle(0, 0, _texture.Width, _texture.Height);
        }

        protected override void OnRender(ISpriteBatch sb)
        {
            base.OnRender(sb);

            if (_texture != null)
                sb.Draw(_texture, _source, _clippingBounds, _color, 0, new Vector2());
        }

        [DisplayName("Source File")]
        [BrowsableString("Select texture", "Texture|*.png;*.JPG;*.DDS")]
        [DataMember]
        public string SourceFile
        {
            get { return _sourceFile; }
            set
            {
                _sourceFile = value;
                GetSourceFile();
            }
        }

        [Browsable(false)]
        public ITexture2D Texture
        {
            get { return _texture; }
            set { _texture = value; }
        }

        [Category("Appearance")]
        [DataMember]
        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        [ExpandablePropertyAttribute]
        [Category("Settings")]
        [DisplayName("Source Area")]
        [DataMember]
        public Rectangle SourceArea
        {
            get { return _source; }
            set
            {
                _source = value;
                _surpassSourceUpdate = true;
            }
        }
    }
}
