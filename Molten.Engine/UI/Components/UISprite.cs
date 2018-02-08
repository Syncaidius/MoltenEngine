//using SharpDX;
//using Molten.IO;
//using Molten.Graphics;
//using Molten.Rendering;
//using Molten.Utilities;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Runtime.Serialization;
//using Molten.Serialization;

//namespace Molten.UI
//{
//    /// <summary>A UI component capable of displaying a texture.</summary>
//    public class UISprite : UIComponent
//    {
//        string _sourceFile;

//        Rectangle _source;
//        SpriteSheet _sheet;
//        Sprite _sprite;
//        string _spriteKey;

//        /// <summary>Creates a new instance of <see cref="UIImage"/>.</summary>
//        /// <param name="ui">The UI system to bind to.</param>
//        public UISprite(UISystem ui)
//            : base(ui)
//        {
//            _source = new Rectangle(0, 0, 1, 1);
//        }

//        protected override void OnUpdateBounds()
//        {
//            base.OnUpdateBounds();

//            if (_sprite != null)
//            {
//                Vector2 offset = new Vector2()
//                {
//                    X = _globalBounds.Width * _sprite.Origin.X,
//                    Y = _globalBounds.Height * _sprite.Origin.Y,
//                };

//                _sprite.Position = new Vector2(_globalBounds.X, _globalBounds.Y) + offset;

//                UpdateSpriteScale();
//            }
//        }

//        private void RetrieveSprite(string key)
//        {
//            if (_sheet == null)
//                return;

//            if (_sheet.HasEntry(key))
//                _sprite = _sheet.GetSprite(key, _sprite);
//        }

//        private void UpdateSpriteScale()
//        {
//            // Scale up or down (if its too big, the scale will be < 1.0. If its too small, the scale becomes a multiplier.
//            _sprite.Scale = new Vector2()
//            {
//                X = (float)_globalBounds.Width / _sprite.FrameWidth,
//                Y = (float)_globalBounds.Height / _sprite.FrameHeight,
//            };
//        }

//        private void GetSourceFile()
//        {
//            ContentRequest cr = _ui.Engine.Content.GetNewRequest();
//            cr.AddGet<SpriteSheet>(_sourceFile);
//            cr.OnCompleted += request_OnCompleted;
//            _ui.Engine.Content.SubmitRequest(cr);
//        }

//        void request_OnCompleted(ContentManager content, ContentRequest request)
//        {
//            _sheet = content.Get<SpriteSheet>(request.Files[0].Path);

//            if (!string.IsNullOrWhiteSpace(_spriteKey))
//            {
//                RetrieveSprite(_spriteKey);
//                OnUpdateBounds();
//            }
//        }

//        protected override void OnUpdate(Schedule time)
//        {
//            base.OnUpdate(time);

//            if (_sprite != null)
//                _sprite.Update(time);
//        }

//        protected override void OnRender(SpriteBatch sb, RenderProxy proxy)
//        {
//            base.OnRender(sb, proxy);
//            if (_sprite != null)
//            {
//                UpdateSpriteScale();
//                sb.Draw(_sprite);
//            }
//        }

//        [DisplayName("Sprite Sheet")]
//        [BrowsableString("Select sprite sheet", "Sprite Sheet|*.json")]
//        [DataMember]
//        public string SourceFile
//        {
//            get { return _sourceFile; }
//            set
//            {
//                _sourceFile = value;
//                GetSourceFile();
//            }
//        }

//        [DataMember]
//        /// <summary>Gets or sets the sprite key. This must exist on the loaded sprite sheet for it to be rendered.</summary>
//        public string SpriteKey
//        {
//            get { return _spriteKey; }
//            set
//            {
//                _spriteKey = value;
//                RetrieveSprite(_spriteKey);
//                OnUpdateBounds();
//            }
//        }

//        [Browsable(false)]
//        /// <summary>Gets the currently displayed sprite.</summary>
//        public Sprite Sprite
//        {
//            get { return _sprite; }
//        }
//    }
//}
