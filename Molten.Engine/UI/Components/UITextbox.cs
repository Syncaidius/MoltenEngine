using Molten.Graphics;
using Molten.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    /// <summary>A textbox component that allows input of typed text.</summary>
    [DataContract]
    public class UITextbox : UIComponent
    {
        const int REPEAT_DELAY = 500; // Delay before supported non-character keys start repeating, in milliseconds.
        const int REPEAT_INTERVAL = 100; //Milliseconds between each repeat tick.

        string _actualText = "";
        UIRenderedText _text;
        Color _bgColor;
        Color _colorBorderFocused;
        Color _colorBorderUnFocused;
        Color _cursorColor;
        Color _selectionColor;

        int _maxLength = 50;
        int _cursorIndex;
        int _selectionIndex; //the index at which a selection starts
        int _blinkInterval = 1000;

        bool _numericOnly = false;
        bool _isMasked = false;
        bool _blinkCursor = false;
        bool _isEditable = true;
        bool _cursorVisible = true;
        double _blinkTimer;
        float _cursorPos;
        float _selectionPos;

        Rectangle _textBounds;
        Rectangle _selectionBounds;

        bool _sysKeyHeld;
        double _sysKeyTimer;

        public event UIComponentHandler<UITextbox> OnEnterKey;

        static char[] _ignored = { '\n', '\r',
                                    (char)1, // CTRL + A (select all)
                                    (char)3, // CTRL + C (copy)
                                    (char)22, // CTRL + V (paste)
                                    (char)24, // CTRL + X (cut)
                                    (char)27, // Escape
                                 };

        public UITextbox(Engine engine) : base(engine)
        {
            _bgColor = new Color(90, 90, 90, 255);
            _cursorColor = new Color();
            _selectionColor = new Color(150, 150, 200, 180);
            _colorBorderFocused = new Color(0, 122, 204, 255);
            _colorBorderUnFocused = new Color(120, 120, 120, 255);

            Padding.SuppressEvents = true;
            Padding.Left = 2;
            Padding.Right = 2;
            Padding.Top = 2;
            Padding.Bottom = 2;
            Padding.SuppressEvents = false;


            _text = new UIRenderedText(engine);
            _text.Text = "";
            _text.VerticalAlignment = UIVerticalAlignment.Center;

            _text.OnChanged += _text_OnChanged;
            _enableClipping = true;

            OnClickEnded += UITextbox_OnPressCompleted;
            OnClickStarted += UITextbox_OnPressStarted;
            OnClickEndedOutside += UITextbox_OnPressCompletedOutside;
            OnDrag += UITextbox_OnDrag;
        }

        protected override void OnUISystemChanged(UISystem oldSystem, UISystem newSystem)
        {
            base.OnUISystemChanged(oldSystem, newSystem);

            if (oldSystem != null)
                oldSystem.Keyboard.OnCharacterKey -= Keyboard_OnCharacterKey;

            if (newSystem != null)
                newSystem.Keyboard.OnCharacterKey += Keyboard_OnCharacterKey;
        }

        void _text_OnChanged(UIRenderedText text)
        {
            AlignText();
        }

        void Keyboard_OnCharacterKey(IO.CharacterEventArgs e)
        {
            if (!_isEditable)
                return;

            if (_ui.Focused == this)
            {
                bool ignore = false;

                if (_numericOnly)
                {
                    ignore = e.Character < 48 || e.Character > 57;
                }
                else
                {
                    //handle backspace
                    if (e.Character == '\b')
                    {
                        if (_selectionIndex != _cursorIndex)
                        {
                            DeleteSelection();
                            ApplyText();
                        }
                        else
                        {
                            if (_text.Length > 0)
                            {
                                // Delete char at the cursor's location
                                int pos = _cursorIndex - 1;
                                if (pos >= 0)
                                {
                                    _actualText = _actualText.Remove(pos, 1);
                                    ApplyText();
                                    GoBackwardAndAlign();
                                }
                            }
                        }

                        ignore = true;
                    }

                    // Check for ignored characters
                    for (int i = 0; i < _ignored.Length; i++)
                    {
                        if (e.Character == _ignored[i])
                        {
                            ignore = true;
                            break;
                        }
                    }
                }

                //append character onto string
                if (!ignore && _text.Length < _maxLength)
                {
                    if (_selectionIndex != _cursorIndex)
                        DeleteSelection();

                    _actualText = _actualText.Insert(_cursorIndex, e.Character.ToString());
                    ApplyText();

                    GoForwardAndAlign();

                    _selectionIndex = _cursorIndex;
                    CheckSelection();
                }
                else
                {
                    AlignText();
                }
            }
        }

        private void ApplyText()
        {
            if (_isMasked)
                _text.Text = new string('*', _actualText.Length);
            else
                _text.Text = _actualText;
        }

        private void AlignText()
        {
            _cursorPos = _text.GetSize(_cursorIndex).X;
            _textBounds = _clippingBounds;
            _textBounds.X = _globalBounds.X + _clipPadding.Left;

            if (_cursorPos > _clippingBounds.Width)
            {
                int difX = (int)_cursorPos - _clippingBounds.Width;
                _textBounds.X -= difX + 5;
            }

            // Offset cursor by 1 pixel.
            _cursorPos -= 1;
            _text.Bounds = _textBounds;
        }

        void UITextbox_OnPressStarted(UIEventData<MouseButton> data)
        {
            Focus();

            if (data.InputValue != MouseButton.Left)
                return;

            Vector2F localPos = data.Position - _text.ActualPosition;

            int nearest = _text.Font.NearestCharacter(_text.Text, localPos);
            _selectionIndex = nearest;
            _cursorIndex = nearest;

            AlignText();
            CheckSelection();
        }

        void UITextbox_OnDrag(UIEventData<MouseButton> data)
        {
            Vector2F localPos = data.Position - _text.ActualPosition;
            Vector2F textSize = _text.GetSize();

            if (localPos.X >= 0 && localPos.X <= textSize.X)
            {
                int nearest = _text.Font.NearestCharacter(_text.Text, localPos);
                _cursorIndex = Math.Min(nearest + 1, _text.Length);
                _cursorPos = localPos.X;

                AlignText();
                CheckSelection(false);
            }
        }

        void UITextbox_OnPressCompleted(UIEventData<MouseButton> data)
        {
            //Finalize the end point of the dragged selection
            if (data.WasDragged)
            {
                Vector2F textSize = _text.GetSize(_cursorIndex);
                _cursorPos = textSize.X;

                AlignText();
                CheckSelection(false);
            }
        }

        void UITextbox_OnPressCompletedOutside(UIEventData<MouseButton> data)
        {
            UITextbox_OnPressCompleted(data);
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();
            AlignText();
            CheckSelection(false);
        }

        private void GoBackwardAndAlign()
        {
            _cursorIndex = Math.Max(0, _cursorIndex - 1);
            AlignText();
            CheckSelection();
        }

        private void GoForwardAndAlign()
        {
            _cursorIndex = Math.Min(_text.Length, _cursorIndex + 1);
            AlignText();
            CheckSelection();
        }

        /// <summary>Checks whether to update the selection or clear it based on the cursor and selection index.</summary>
        private void CheckSelection(bool checkShift = true)
        {
            if (checkShift)
            {
                if (!_ui.Keyboard.IsPressed(Key.LeftShift) && !_ui.Keyboard.IsPressed(Key.RightShift))
                {
                    _selectionIndex = _cursorIndex;
                    _selectionPos = _cursorPos;
                }
            }

            // Update seleection bounds
            Vector2F textPos = _text.ActualPosition;
            Vector2F textSize = _text.GetSize();

            if (_cursorIndex < _selectionIndex)
            {
                _selectionBounds = new Rectangle()
                {
                    X = (int)(textPos.X + _cursorPos),
                    Y = (int)textPos.Y,
                    Width = (int)(_selectionPos - _cursorPos),
                    Height = (int)textSize.Y,
                };
            }
            else if (_cursorIndex > _selectionIndex)
            {
                _selectionBounds = new Rectangle()
                {
                    X = (int)(textPos.X + _selectionPos),
                    Y = (int)textPos.Y,
                    Width = (int)(_cursorPos - _selectionPos),
                    Height = (int)textSize.Y,
                };
            }
        }

        public void DeleteSelection()
        {
            if (_selectionIndex > _cursorIndex)
            {
                int len = _selectionIndex - _cursorIndex;
                _actualText = _text.Text.Remove(_cursorIndex, len);

                _selectionIndex = _cursorIndex;
            }
            else if (_selectionIndex < _cursorIndex)
            {
                int len = _cursorIndex - _selectionIndex;
                _actualText = _text.Text.Remove(_selectionIndex, len);
                _cursorIndex = _selectionIndex;
            }

            CheckSelection();
            AlignText();
        }

        public void Cut()
        {
            Copy();

            if (!_isEditable)
                return;

            DeleteSelection();
        }

        public void Copy()
        {
            if (_selectionIndex > _cursorIndex)
            {
                int len = _selectionIndex - _cursorIndex;
                string substr = _text.Text.Substring(_cursorIndex, len);
                _engine.Input.Clipboard.SetText(substr);
            }
            else if (_selectionIndex < _cursorIndex)
            {
                int len = _cursorIndex - _selectionIndex;
                string substr = _text.Text.Substring(_selectionIndex, len);
                _engine.Input.Clipboard.SetText(substr);
            }
        }

        public void Paste()
        {
            if (!_isEditable)
                return;

            string toPaste = _engine.Input.Clipboard.GetText();

            if (_selectionIndex != _cursorIndex)
                DeleteSelection();

            _actualText = _actualText.Insert(_cursorIndex, toPaste);
            _cursorIndex = Math.Min(_text.Length, _cursorIndex + toPaste.Length);

            ApplyText();
            AlignText();
            CheckSelection();
        }

        public void SelectAll()
        {
            _selectionIndex = 0;
            _selectionPos = 0;
            _cursorIndex = _text.Length;

            AlignText();
            CheckSelection(false);
        }

        /// <summary>Called when DEL key is pressed</summary>
        private void DeleteKey()
        {
            if (!_isEditable)
                return;

            if (_text.Length > 0)
            {
                if (_selectionIndex != _cursorIndex)
                {
                    DeleteSelection();
                }
                else
                {
                    // Delete char at the cursor's location
                    int pos = _cursorIndex;
                    if (pos < _text.Text.Length)
                    {
                        _actualText = _actualText.Remove(pos, 1);
                        ApplyText();
                    }

                    CheckSelection();
                }
            }
        }

        protected override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);

            if (_ui.Focused == this)
            {
                if (_ui.Keyboard.IsTapped(Key.Left))
                {
                    GoBackwardAndAlign();
                }
                else if (_ui.Keyboard.IsTapped(Key.Right))
                {
                    GoForwardAndAlign();
                }
                else if (_ui.Keyboard.IsPressed(Key.LeftControl) || _ui.Keyboard.IsPressed(Key.RightControl))
                {
                    if (_ui.Keyboard.IsTapped(Key.V))
                        Paste();
                    else if (_ui.Keyboard.IsTapped(Key.C))
                        Copy();
                    else if (_ui.Keyboard.IsTapped(Key.X))
                        Cut();
                    else if (_ui.Keyboard.IsTapped(Key.A))
                        SelectAll();
                }
                else if (_ui.Keyboard.IsTapped(Key.Home))
                {
                    _cursorIndex = 0;
                    AlignText();
                    CheckSelection();
                }
                else if (_ui.Keyboard.IsTapped(Key.End))
                {
                    _cursorIndex = _text.Text.Length;
                    AlignText();
                    CheckSelection();
                }
                else if (_ui.Keyboard.IsTapped(Key.Return))
                {
                    if (OnEnterKey != null)
                        OnEnterKey(this);
                }
                else if (_ui.Keyboard.IsTapped(Key.NumberPadEnter))
                {
                    if (OnEnterKey != null)
                        OnEnterKey(this);
                }
                else if (_ui.Keyboard.IsTapped(Key.Delete))
                {
                    DeleteKey();
                }
                else if (_ui.Keyboard.IsPressed(Key.Delete))
                {
                    _sysKeyTimer += time.ElapsedTime.TotalMilliseconds;

                    if (_sysKeyTimer >= REPEAT_DELAY && !_sysKeyHeld)
                    {
                        _sysKeyTimer -= REPEAT_DELAY;
                        _sysKeyHeld = true;
                        DeleteKey();
                    }
                    else if (_sysKeyTimer >= REPEAT_INTERVAL && _sysKeyHeld)
                    {
                        _sysKeyTimer -= REPEAT_INTERVAL;
                        DeleteKey();
                    }
                }
                else if (_ui.Keyboard.IsPressed(Key.Left))
                {
                    _sysKeyTimer += time.ElapsedTime.TotalMilliseconds;

                    if (_sysKeyTimer >= REPEAT_DELAY && !_sysKeyHeld)
                    {
                        _sysKeyTimer -= REPEAT_DELAY;
                        _sysKeyHeld = true;
                        GoBackwardAndAlign();
                    }
                    else if (_sysKeyTimer >= REPEAT_INTERVAL && _sysKeyHeld)
                    {
                        _sysKeyTimer -= REPEAT_INTERVAL;
                        GoBackwardAndAlign();
                    }
                }
                else if (_ui.Keyboard.IsPressed(Key.Right))
                {
                    _sysKeyTimer += time.ElapsedTime.TotalMilliseconds;

                    if (_sysKeyTimer >= REPEAT_DELAY && !_sysKeyHeld)
                    {
                        _sysKeyTimer -= REPEAT_DELAY;
                        _sysKeyHeld = true;
                        GoForwardAndAlign();
                    }
                    else if (_sysKeyTimer >= REPEAT_INTERVAL && _sysKeyHeld)
                    {
                        _sysKeyTimer -= REPEAT_INTERVAL;
                        GoForwardAndAlign();
                    }
                }
                else
                {
                    // Reset system key timing/triggers
                    _sysKeyTimer = 0;
                    _sysKeyHeld = false;
                }

                // TODO blink cursor
                if (_blinkCursor)
                {
                    _blinkTimer += time.ElapsedTime.TotalMilliseconds;
                    if (_blinkTimer >= _blinkInterval)
                    {
                        _cursorVisible = !_cursorVisible;
                        _blinkTimer -= _blinkInterval;
                    }
                }
            }
        }

        protected override void OnRender(SpriteBatch sb)
        {
            base.OnRender(sb);

            if (_ui.Focused == this)
                sb.DrawRect(_globalBounds, _colorBorderFocused);
            else
                sb.DrawRect(_globalBounds, _colorBorderUnFocused);

            sb.DrawRect(_clippingBounds, _bgColor);

            if (_cursorVisible && _ui.Focused == this)
                sb.DrawString(_text.Font, "|", _text.ActualPosition + new Vector2F(_cursorPos, 0), _cursorColor);
        }

        protected override void OnRenderClipped(SpriteBatch sb)
        {
            base.OnRenderClipped(sb);

            if (_selectionIndex != _cursorIndex)
                sb.DrawRect(_selectionBounds, _selectionColor);

            _text.Draw(sb);
        }

        [ExpandablePropertyAttribute]
        [Category("Appearance")]
        [DataMember]
        public UIRenderedText TextRenderer
        {
            get { return _text; }
        }

        [DataMember]
        [Category("Appearance")]
        [DisplayName("Background Color")]
        public Color BackgroundColor
        {
            get { return _bgColor; }
            set { _bgColor = value; }
        }

        [DataMember]
        [Category("Appearance")]
        [DisplayName("Focused Border Color")]
        public Color FocusedBorderColor
        {
            get { return _colorBorderFocused; }
            set { _colorBorderFocused = value; }
        }

        [DataMember]
        [Category("Appearance")]
        [DisplayName("unfocused Border Color")]
        public Color UnfocusedBorderColor
        {
            get { return _colorBorderUnFocused; }
            set { _colorBorderUnFocused = value; }
        }

        [DataMember]
        [Category("Appearance")]
        [DisplayName("Selection Color")]
        public Color SelectionColor
        {
            get { return _selectionColor; }
            set { _selectionColor = value; }
        }

        [DataMember]
        [Category("Appearance")]
        [DisplayName("Cursor Color")]
        public Color CursorColor
        {
            get { return _cursorColor; }
            set { _cursorColor = value; }
        }

        [Category("Settings")]
        [DisplayName("Blink Interval (ms)")]
        [DataMember]
        public int BlinkInterval
        {
            get { return _blinkInterval; }
            set { _blinkInterval = value; }
        }

        [Category("Settings")]
        [DisplayName("Is Editable")]
        [DataMember]
        public bool IsEditable
        {
            get { return _isEditable; }
            set { _isEditable = value; }
        }

        [Category("Settings")]
        [DisplayName("Blink Cursor")]
        [DataMember]
        public bool BlinkCursor
        {
            get { return _blinkCursor; }
            set
            {
                _blinkCursor = value;
                if (!_blinkCursor)
                    _cursorVisible = true;
            }
        }

        /// <summary>Gets or sets whether or not the textbox only accepts numeric characters.</summary>
        [Category("Settings")]
        [DisplayName("Is Numeric Only")]
        [DataMember]
        public bool IsNumericOnly
        {
            get { return _numericOnly; }
            set { _numericOnly = value; }
        }

        /// <summary>Gets or sets whether or not the textbox is masked (hides whatever is typed).</summary>
        [Category("Settings")]
        [DisplayName("Is Masked")]
        [DataMember]
        public bool IsMasked
        {
            get { return _isMasked; }
            set { _isMasked = value; }
        }

        [DataMember]
        public int MaxLength
        {
            get { return _maxLength; }
            set
            {
                _maxLength = value;
                if (_text.Text.Length > _maxLength)
                {
                    _text.Text = _text.Text.Substring(0, Math.Min(_text.Length, _maxLength));
                    AlignText();
                    CheckSelection();
                }
            }
        }

        [DataMember]
        public string Text
        {
            get { return _actualText; }
            set
            {
                _actualText = value;
                _cursorIndex = _actualText.Length;
                _selectionIndex = _cursorIndex;

                ApplyText();
                AlignText();
                CheckSelection();
            }
        }
    }
}
