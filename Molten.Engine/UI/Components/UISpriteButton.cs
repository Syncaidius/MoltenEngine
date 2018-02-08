//using Molten.IO;
//using Molten.Graphics;
//using Molten.Utilities;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Runtime.Serialization;
//using Molten.IO;

//namespace Molten.UI
//{
//    /// <summary>A button component which uses sprites to display itself.</summary>
//    public class UISpriteButton : UIComponent
//    {
//        struct SpriteState
//        {
//            public Sprite Sprite;
//            public string SpriteKey;
//        }

//        string _sourceFile;

//        Rectangle _source;
//        SpriteSheet _sheet;
//        SpriteState[] _states;
//        SpriteState _icon;

//        UIButtonState _curState;
//        int _curStateVal;

//        /// <summary>Creates a new instance of <see cref="UIImage"/>.</summary>
//        /// <param name="ui">The UI system to bind to.</param>
//        public UISpriteButton(UISystem ui)
//            : base(ui)
//        {
//            _source = new Rectangle(0, 0, 1, 1);
//            _states = new SpriteState[4];

//            OnClickStarted += UISpriteButton_StateClicked;
//            OnClickEnded += UISpriteButton_StateHover;
//            OnClickEndedOutside += UISpriteButton_StateDefault;
//            OnEnter += UISpriteButton_StateHover;
//            OnLeave += UISpriteButton_StateDefault;
//        }

//        private void UISpriteButton_StateDefault(UIEventData<MouseButton> data)
//        {
//            SetState(UIButtonState.Default);
//        }

//        private void UISpriteButton_StateHover(UIEventData<MouseButton> data)
//        {
//            SetState(UIButtonState.Hover);
//        }

//        private void UISpriteButton_StateClicked(UIEventData<MouseButton> data)
//        {
//            SetState(UIButtonState.Clicked);
//        }

//        private void SetState(UIButtonState newState)
//        {
//            if (newState != _curState)
//            {
//                // Stop the sprite of the old state
//                int stateVal = (int)_curState;
//                if (_states[stateVal].Sprite != null)
//                    _states[stateVal].Sprite.Stop();

//                _curState = newState;

//                // Start playing the new state sprite
//                _curStateVal = (int)_curState;
//                if (_states[_curStateVal].Sprite != null)
//                {
//                    _states[_curStateVal].Sprite.Play(false);
//                    OnUpdateBounds();
//                }
//            }
//        }

//        protected override void OnUpdateBounds()
//        {
//            base.OnUpdateBounds();

//            Sprite spr;
//            Vector2 newPos = new Vector2(_globalBounds.X, _globalBounds.Y);

//            // Update position of all state sprites.
//            for (int i = 0; i < _states.Length; i++)
//            {
//                spr = _states[i].Sprite;
//                if (spr != null)
//                {
//                    Vector2 offset = new Vector2()
//                    {
//                        X = _globalBounds.Width * spr.Origin.X,
//                        Y = _globalBounds.Height * spr.Origin.Y,
//                    };

//                    spr.Position = newPos + offset;
//                    UpdateSpriteScale(spr);
//                }
//            }

//            if (_icon.Sprite != null)
//                _icon.Sprite.Position = _globalBounds.Center;
//        }

//        public override bool Contains(Vector2 location)
//        {

//            if (base.Contains(location))
//            {
//                // Attempt collision testing on the sprite's own terms (pixel perfect if it's sheet supports it).
//                if (_states[_curStateVal].Sprite != null)
//                    return _states[_curStateVal].Sprite.Contains(location);
//                else
//                    return true;
//            }
//            else
//            {
//                return false;
//            }
//        }

//        private void RetrieveSprite(UIButtonState state)
//        {
//            if (_sheet == null)
//                return;

//            int stateVal = (int)state;
//            Vector2 pos = new Vector2(_globalBounds.X, _globalBounds.Y);

//            // Ensure the sprite key is a valid string
//            if (!string.IsNullOrWhiteSpace(_states[stateVal].SpriteKey))
//            {
//                // Check for key in sheet.
//                if (_sheet.HasEntry(_states[stateVal].SpriteKey))
//                {
//                    _states[stateVal].Sprite = _sheet.GetSprite(_states[stateVal].SpriteKey, _states[stateVal].Sprite);
//                    _states[stateVal].Sprite.Position = pos;
//                }
//            }
//        }

//        private void RetrieveIcon()
//        {
//            if (_sheet == null)
//                return;

//            if (_sheet.HasEntry(_icon.SpriteKey))
//            {
//                _icon.Sprite = _sheet.GetSprite(_icon.SpriteKey, _icon.Sprite);
//                _icon.Sprite.Position = _globalBounds.Center;
//            }
//        }

//        private void UpdateSpriteScale(Sprite spr)
//        {
//            // Scale up or down (if its too big, the scale will be < 1.0. If its too small, the scale becomes a multiplier.
//            spr.Scale = new Vector2()
//            {
//                X = (float)_globalBounds.Width / spr.FrameWidth,
//                Y = (float)_globalBounds.Height / spr.FrameHeight,
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

//            for(UIButtonState s = 0; s < UIButtonState.Disabled; s++)
//                    RetrieveSprite(s);

//            OnUpdateBounds();
//        }

//        protected override void OnUpdate(Schedule time)
//        {
//            base.OnUpdate(time);

//            if (_states[_curStateVal].Sprite != null)
//                _states[_curStateVal].Sprite.Update(time);

//            if (_icon.Sprite != null)
//                _icon.Sprite.Update(time);
//        }

//        protected override void OnRender(SpriteBatch sb, RenderProxy proxy)
//        {
//            base.OnRender(sb, proxy);

//            Sprite spr = _states[_curStateVal].Sprite;
//            if (_curStateVal > 0)
//            {
//                int derp = 0;
//            }
//            if (spr != null)
//            {
//                UpdateSpriteScale(spr);
//                sb.Draw(spr);
//            }

//            if (_icon.Sprite != null)
//                sb.Draw(_icon.Sprite);
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

//        [DisplayName("Default Key")]
//        [Category("States")]
//        [DataMember]
//        /// <summary>Gets or sets the default state sprite key. The sprite must exist on the loaded sprite sheet for it to be rendered.</summary>
//        public string DefaultKey
//        {
//            get { return _states[(int)UIButtonState.Default].SpriteKey; }
//            set
//            {
//                _states[(int)UIButtonState.Default].SpriteKey = value;
//                RetrieveSprite(UIButtonState.Default);
//            }
//        }

//        [DisplayName("Hover Key")]
//        [Category("States")]
//        [DataMember]
//        /// <summary>Gets or sets the hover state sprite key. The sprite must exist on the loaded sprite sheet for it to be rendered.</summary>
//        public string HoverKey
//        {
//            get { return _states[(int)UIButtonState.Hover].SpriteKey; }
//            set
//            {
//                _states[(int)UIButtonState.Hover].SpriteKey = value;
//                RetrieveSprite(UIButtonState.Hover);
//            }
//        }

//        [DisplayName("Clicked Key")]
//        [Category("States")]
//        [DataMember]
//        /// <summary>Gets or sets the clicked state sprite key. The sprite must exist on the loaded sprite sheet for it to be rendered.</summary>
//        public string ClickedKey
//        {
//            get { return _states[(int)UIButtonState.Clicked].SpriteKey; }
//            set
//            {
//                _states[(int)UIButtonState.Clicked].SpriteKey = value;
//                RetrieveSprite(UIButtonState.Clicked);
//            }
//        }

//        [DisplayName("Disabled Key")]
//        [Category("States")]
//        [DataMember]
//        /// <summary>Gets or sets the default state sprite key. The sprite must exist on the loaded sprite sheet for it to be rendered.</summary>
//        public string DisabledKey
//        {
//            get { return _states[(int)UIButtonState.Disabled].SpriteKey; }
//            set
//            {
//                _states[(int)UIButtonState.Disabled].SpriteKey = value;
//                RetrieveSprite(UIButtonState.Disabled);
//            }
//        }

//        [DisplayName("Icon Key")]
//        [DataMember]
//        public string IconKey
//        {
//            get { return _icon.SpriteKey; }
//            set
//            {
//                _icon.SpriteKey = value;
//                RetrieveIcon();
//            }
//        }

//        [Browsable(false)]
//        /// <summary>Gets a sprite representing a button state.</summary>
//        /// <param name="state"></param>
//        /// <returns></returns>
//        public Sprite this[UIButtonState state]
//        {
//            get { return _states[(int)state].Sprite; }
//        }

//        [Browsable(false)]
//        /// <summary>Gets the current button state.</summary>
//        public UIButtonState State
//        {
//            get { return _curState; }
//        }

//        [DataMember]
//        /// <summary>Gets or sets whether or not the button is enabled.</summary>
//        public override bool IsEnabled
//        {
//            get { return base.IsEnabled; }

//            set
//            {
//                base.IsEnabled = true;
//                if (value)
//                    SetState(UIButtonState.Default);
//                else
//                    SetState(UIButtonState.Disabled);
//            }
//        }
//    }
//}
