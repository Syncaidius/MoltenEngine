//using Molten.Collections;
//using Molten.Graphics;
//using Molten.IO;
//using Molten.Utilities;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Runtime.Serialization;
//using System.Text;
//using System.Threading.Tasks;

//namespace Molten.UI
//{
//    public partial class UITextArea : UICompoundComponent
//    {
//        const int BORDER_THICKNESS = 2;
//        const int SCROLLBAR_THICKNESS = 20;
//        const int REPEAT_DELAY = 500; // Delay before supported non-character keys start repeating, in milliseconds.
//        const int REPEAT_INTERVAL = 50; //Milliseconds between each repeat tick.

//        Color _bgColor;
//        Color _colorBorderFocused;
//        Color _colorBorderUnFocused;
//        Color _cursorColor;
//        Color _selectionColor;

//        List<Line> _lines;
//        ObjectPool<Line> _linePool;
//        int _startLine;
//        int _endLine;

//        int _maxLength = 50;
//        CursorLocation _caret;
//        CursorLocation _selection; 
//        int _blinkInterval = 1000;
//        double _blinkTimer;
//        Vector2 _cursorPos;

//        bool _blinkCursor = false;
//        bool _isEditable = true;
//        bool _cursorVisible = true;

//        bool _sysKeyHeld;
//        double _sysKeyTimer;

//        UIHorizontalScrollBar _hBar;
//        UIVerticalScrollBar _vBar;
//        Vector2 _scrollOffset;
//        int _widestLine;
//        int _lineCapacity;
//        Engine _engine;

//        public event UIComponentHandler<UITextbox> OnEnterKey;

//        static char[] _ignored = {  (char)1, // CTRL + A (select all)
//                                    (char)3, // CTRL + C (copy)
//                                    (char)22, // CTRL + V (paste)
//                                    (char)24, // CTRL + X (cut)
//                                    (char)27, // Escape
//                                 };

//        /* TODO:
//         *  - BUG - horizontal scroll bar only gets its max value from visible lines
//         */

//        public UITextArea(Engine engine) : base(engine)
//        {
//            _engine = engine;
//            _linePool = new ObjectPool<Line>(() => new Line(_engine));
//            _lines = new List<Line>();

//            _bgColor = new Color(90, 90, 90, 255);
//            _colorBorderFocused = new Color(0, 122, 204, 255);
//            _colorBorderUnFocused = new Color(120, 120, 120, 255);
//            _cursorColor = new Color();
//            _selectionColor = new Color(150, 150, 200, 180);

//            Padding.SuppressEvents = true;
//            Padding.Left = BORDER_THICKNESS;
//            Padding.Right = SCROLLBAR_THICKNESS + BORDER_THICKNESS;
//            Padding.Top = BORDER_THICKNESS;
//            Padding.Bottom = SCROLLBAR_THICKNESS + BORDER_THICKNESS;
//            Padding.SuppressEvents = false;

//            _enableClipping = true;

//            _hBar = new UIHorizontalScrollBar(_engine)
//            {
//                BarRange = 20,
//                ScrollSpeed = 10,
//            };
//            _vBar = new UIVerticalScrollBar(_engine)
//            {
//                BarRange = 5,
//                ScrollSpeed = 10,
//            };

//            _hBar.OnScroll += _hBar_OnScroll;
//            _vBar.OnScroll += _vBar_OnScroll;

//            AddPart(_hBar);
//            AddPart(_vBar);

//            GetNewLine();

//            _ui.Keyboard.OnCharacterKey += Keyboard_OnCharacterKey;
//            OnClickStarted += UITextArea_OnClickStarted;
//            OnScrollWheel += UITextArea_OnScrollWheel;
//            OnDrag += UITextArea_OnDrag;
//        }

//        void UITextArea_OnDrag(UIEventData<MouseButton> data)
//        {
//            //Calculate which line was clicked
//            float lineHeight = _lines[0].textObject.GetSize().Y;
//            float localY = data.Position.Y - _clippingBounds.Y;
//            localY /= lineHeight;

//            localY = Math.Max(0, localY);
//            int lineNumber = (int)Math.Floor(localY);

//            _selection.line = Math.Min(_lines.Count - 1, _startLine + lineNumber);
//            Line line = _lines[_selection.line];
//            Vector2 localPos = data.Position - line.textObject.ActualPosition;

//            _selection.index = line.textObject.Font.NearestCharacter(line.Text, localPos);

//            AlignText();
//            CheckSelection(false);
//        }

//        void UITextArea_OnScrollWheel(UIEventData<MouseButton> data)
//        {
//            _vBar.Scroll((int)-data.Delta.Y);
//        }

//        void _vBar_OnScroll(UIVerticalScrollBar component)
//        {
//            float lineHeight = _lines[0].textObject.GetSize().Y;

//            float line = (float)_vBar.Value / lineHeight;
//            float remaining = MathHelper.Mod(line, 1);

//            _scrollOffset.Y = remaining * lineHeight;

//            _startLine = (int)Math.Floor(line);
//            AlignText();
//            CheckSelection(false);
//        }

//        void _hBar_OnScroll(UIHorizontalScrollBar component)
//        {
//            int val = Math.Max(0, _hBar.Value - _clippingBounds.Width);
//            _scrollOffset.X = _hBar.Value;

//            AlignText();
//            CheckSelection(false);
//        }

//        void UITextArea_OnClickStarted(UIEventData<MouseButton> data)
//        {
//            Focus();

//            if (data.InputValue != MouseButton.Left)
//                return;

//            //Calculate which line was clicked
//            float lineHeight = _lines[0].textObject.GetSize().Y;
//            float localY = data.Position.Y - _clippingBounds.Y;
//            localY /= lineHeight;
//            localY = Math.Max(0, localY);

//            int lineNumber = (int)Math.Floor(localY);

//            if (IsShiftPressed())
//            {
//                _selection.line = Math.Min(_lines.Count - 1, _startLine + lineNumber);
//                Line line = _lines[_selection.line];
//                Vector2 localPos = data.Position - line.textObject.ActualPosition;

//                _selection.index = line.textObject.Font.NearestCharacter(line.Text, localPos);
//            }
//            else
//            {
//                _caret.line = Math.Min(_lines.Count - 1, _startLine + lineNumber);
//                Line line = _lines[_caret.line];
//                Vector2 localPos = data.Position - line.textObject.ActualPosition;

//                _caret.index = line.textObject.Font.NearestCharacter(line.Text, localPos);
//                _selection = _caret;
//            }

//            AlignText();
//            CheckSelection();
//        }

//        private bool IsShiftPressed()
//        {
//            return _ui.Keyboard.IsPressed(Key.LeftShift) && !_ui.Keyboard.IsPressed(Key.RightShift);
//        }

//        private Line GetNewLine(int insertAt = -1)
//        {
//            Line line = _linePool.GetInstance();

//            if (insertAt < 0)
//                _lines.Add(line);
//            else
//                _lines.Insert(insertAt, line);

//            return line;
//        }

//        private void RemoveLine(Line line)
//        {
//            _lines.Remove(line);
//            _linePool.Recycle(line);
//        }

//        void Keyboard_OnCharacterKey(IO.CharacterEventArgs e)
//        {
//            if (!_isEditable)
//                return;

//            if (_ui.Focused == this)
//            {
//                bool ignore = false;

//                // Handle special characters
//                if (e.Character == '\b') // Backspace
//                {
//                    if (_selection != _caret)
//                    {
//                        DeleteSelection();
//                    }
//                    else
//                    {
//                        Line line = _lines[_caret.line];
//                        if (line.Length > 0 && _caret.index > 0)
//                        {
//                            // Delete char at the cursor's location
//                            int pos = _caret.index - 1;
//                            if (pos >= 0)
//                            {
//                                line.Text = line.Text.Remove(pos, 1);
//                                GoBackwardAndAlign();

//                                _selection = _caret;
//                            }
//                        }
//                        else
//                        {
//                            // Check if we can move back to the previous line
//                            if (_caret.line > 0)
//                            {
//                                if (line.Length > 0)
//                                {
//                                    int newIndex = _lines[_caret.line - 1].Length;
//                                    MergeLine(_caret.line - 1, _caret.line);
//                                    _caret.line--;
//                                    _caret.index = newIndex;
//                                    _selection = _caret;
//                                }
//                                else
//                                {
//                                    RemoveLine(line);
//                                    _caret.line--;
//                                    _caret.index = _lines[_caret.line].Length;
//                                    _selection = _caret;
//                                }

//                                CheckSelection();
//                            }
//                        }
//                    }

//                    ignore = true;
//                }
//                else if (e.Character == '\r' | e.Character == '\n') // accept both ENTER (return) and SHIFT + ENTER (new line).
//                {
//                    if (_caret.index == _lines[_caret.line].Length)
//                        GetNewLine(_caret.line + 1);
//                    else
//                        SplitLine(_caret.line, _caret.index);

//                    _caret.line++;
//                    _caret.index = 0;

//                    AlignText();
//                    CheckSelection();
//                    ignore = true;
//                }

//                // Check for ignored characters.
//                for (int i = 0; i < _ignored.Length; i++)
//                {
//                    if (e.Character == _ignored[i])
//                    {
//                        ignore = true;
//                        break;
//                    }
//                }

//                CenterOnCaret();

//                // Append character onto string
//                Line curLine = _lines[_caret.line];
//                if (!ignore && curLine.Length < _maxLength)
//                {
//                    if (_selection != _caret)
//                        DeleteSelection();

//                    curLine.Text = curLine.Text.Insert(_caret.index, e.Character.ToString());

//                    GoForwardAndAlign();

//                    _selection = _caret;
//                    CheckSelection();
//                }
//                else
//                {
//                    AlignText();
//                }
//            }
//        }

//        private void CenterViewOnCursor()
//        {
//            // TODO center the text area's scroll bars on to the cursor to bring it in to view.
//        }

//        private void GoBackwardAndAlign()
//        {
//            Line curLine = _lines[_caret.line];

//            // Check if we need to move to the previous line
//            if (_caret.index == 0)
//            {
//                if (_caret.line > 0)
//                {
//                    _caret.line--;
//                    _caret.index = _lines[_caret.line].Length;
//                }
//            }
//            else
//            {
//                _caret.index = Math.Max(0, _caret.index - 1);
//            }

//            AlignText();
//            CheckSelection();
//        }

//        private void GoForwardAndAlign()
//        {
//            Line curLine = _lines[_caret.line];

//            // Check if we need to move to the previous line
//            if (_caret.index == curLine.Length)
//            {
//                if (_caret.line < _lines.Count - 1)
//                {
//                    _caret.line++;
//                    _caret.index = 0;
//                }
//            }
//            else
//            {
//                _caret.index = Math.Min(curLine.Length, _caret.index + 1);
//            }

//            AlignText();
//            CheckSelection();
//        }

//        private void GoUpAndAlign()
//        {
//            _caret.line = Math.Max(0, _caret.line - 1);
//            if (_caret.index > _lines[_caret.line].Length)
//                _caret.index = _lines[_caret.line].Length;

//            AlignText();
//            CheckSelection();
//        }

//        private void GoDownAndAlign()
//        {
//            _caret.line = Math.Min(_caret.line + 1, _lines.Count - 1);
//            if (_caret.index > _lines[_caret.line].Length)
//                _caret.index = _lines[_caret.line].Length;

//            AlignText();
//            CheckSelection();
//        }

//        private void AlignText()
//        {
//            Line curLine = _lines[_caret.line];
//            Vector2 textSize = curLine.textObject.GetSize(_caret.index);

//            // Calculate start and end lines - TEMP - replace with scrolling later
//            _lineCapacity = (int)Math.Ceiling((float)_clippingBounds.Height / textSize.Y);
//            _endLine = _startLine + (int)_lineCapacity;
//            _endLine = Math.Min(_lines.Count - 1, _endLine); // Cap end line

//            int localLine = _caret.line - _startLine;
//            _cursorPos.X = _clippingBounds.X  + (textSize.X - 1);
//            _cursorPos.Y = _clippingBounds.Y + (textSize.Y * localLine);
//            _cursorPos -= _scrollOffset;

//            int largestX = 0;
//            int scrollY = 0;

//            int localID = 0;
//            for (int i = _startLine; i <= _endLine; i++)
//            {
//                curLine = _lines[i];
//                Vector2 lineTextSize = curLine.textObject.GetSize();
//                int yOffset = (int)((textSize.Y * localID) - _scrollOffset.Y);

//                curLine.bounds = new Rectangle()
//                {
//                    X = (_globalBounds.X + _clipPadding.Left) - (int)_scrollOffset.X,
//                    Y = _globalBounds.Y + _clipPadding.Top + yOffset,
//                    Width = (int)lineTextSize.X + 1,
//                    Height = (int)lineTextSize.Y,
//                };

//                curLine.textObject.Bounds = curLine.bounds;

//                // Calculate scroll bar values
//                int lineWidth = curLine.bounds.Width;
//                int extraY = curLine.bounds.Bottom - _clippingBounds.Bottom;

//                if (lineWidth > largestX)
//                    largestX = lineWidth;

//                localID++;
//            }

//            largestX = Math.Max(0, largestX - _clippingBounds.Width);
//            scrollY = Math.Max(0, _lines.Count - _lineCapacity + 1);

//            // Update vertical scroll bar
//            _vBar.MaxValue = (int)Math.Ceiling(scrollY * textSize.Y);
//            _vBar.BarRange = (int)Math.Ceiling((float)_lineCapacity * textSize.Y);
            
//            // Update horizontal scroll bar
//            if (largestX > _widestLine)
//                _widestLine = largestX;

//            _hBar.MaxValue = _widestLine;
//        }

//        /// <summary>Checks whether to update the selection or clear it based on the cursor and selection index.</summary>
//        private void CheckSelection(bool checkShift = true)
//        {
//            if (checkShift)
//            {
//                if (!IsShiftPressed())
//                    _selection = _caret;
//            }

//            // Update the selection bounds of all lines within the selection points

//            if (_caret.line < _selection.line)
//            {
//                Line curLine = null;
//                int start = Math.Max(_startLine, _caret.line);
//                int end = Math.Min(_endLine, _selection.line);

//                for (int i = start; i <= end; i++)
//                {
//                    curLine = _lines[i];

//                    if (i == start)
//                        CheckLineSelection(curLine, _caret.index, curLine.Length);
//                    else if (i == end)
//                        CheckLineSelection(curLine, 0, _selection.index);
//                    else
//                        CheckLineSelection(curLine, 0, curLine.Length);
//                }
//            }
//            else if (_caret.line > _selection.line)
//            {
//                Line curLine = null;
//                int start = Math.Max(_startLine, _selection.line);
//                int end = Math.Min(_endLine, _caret.line);

//                for (int i = start; i <= end; i++)
//                {
//                    curLine = _lines[i];

//                    if (i == start)
//                        CheckLineSelection(curLine, _selection.index, curLine.Length);
//                    else if (i == end)
//                        CheckLineSelection(curLine, 0, _caret.index);
//                    else
//                        CheckLineSelection(curLine, 0, curLine.Length);
//                }
//            }
//            else
//            {
//                CheckLineSelection(_lines[_caret.line], _caret.index, _selection.index);
//            }
//        }

//        private void CheckLineSelection(Line line, int cursorIndex, int selectionIndex)
//        {
//            // Update seleection bounds
//            Vector2 textPos = line.textObject.ActualPosition;
//            Rectangle textBounds = new Rectangle();

//            if (cursorIndex < selectionIndex)
//            {
//                int length = selectionIndex - cursorIndex;
//                textBounds = line.textObject.GetSize(cursorIndex, length);
//            }
//            else if (cursorIndex > selectionIndex)
//            {
//                int length = cursorIndex - selectionIndex;
//                textBounds = line.textObject.GetSize(selectionIndex, length);
//            }

//            line.selectionBounds = new Rectangle()
//            {
//                X = (int)(textPos.X + textBounds.X),
//                Y = (int)textPos.Y,
//                Width = textBounds.Width,
//                Height = textBounds.Height,
//            };
//        }

//        /// <summary>Called when DEL key is pressed</summary>
//        private void DeleteKey()
//        {
//            if (!_isEditable)
//                return;

//            if (_selection != _caret)
//            {
//                DeleteSelection();
//            }
//            else
//            {
//                Line curLine = _lines[_caret.line];
//                if (_caret.index < curLine.Length)
//                {
//                    curLine.Text = curLine.Text.Remove(_caret.index, 1);
//                }
//                else
//                {
//                    if (_caret.line < _lines.Count - 1)
//                        MergeLine(_caret.line, _caret.line + 1);
//                }

//                AlignText();
//                CheckSelection();
//            }
//        }

//        private void MergeLine(int targetIndex, int sourceIndex)
//        {
//            Line target = _lines[targetIndex];
//            Line source = _lines[sourceIndex];

//            target.Text += source.Text;
//            RemoveLine(source);
//        }

//        private void SplitLine(int lineIndex, int charIndex)
//        {
//            Line line = _lines[lineIndex];
//            Line newLine = GetNewLine(lineIndex + 1);

//            int len = line.Text.Length - charIndex;
//            string str = line.Text.Substring(charIndex, len);
//            line.Text = line.Text.Remove(charIndex, len);
//            newLine.Text = str;
//        }


//        private void DeleteSelection()
//        {
//            // Check which way around the selection endings are.
//            if (_caret.line < _selection.line) // cursor location is behind selection endpoint.
//            {
//                DeletionCommon(ref _caret, ref _selection);
//            }
//            else if (_caret.line > _selection.line) // cursor location is in front of selection endpoint.
//            {
//                DeletionCommon(ref _selection, ref _caret);
//            }
//            else
//            {
//                Line curLine = _lines[_caret.line];
//                DeleteText(curLine, _caret.index, _selection.index);
//            }

//            _selection = _caret;

//            CheckSelection();
//            AlignText();
//        }

//        private void DeletionCommon(ref CursorLocation start, ref CursorLocation end)
//        {
//            Line curLine = null;
//            int startLength = _lines[start.line].Length;

//            for (int i = end.line; i >= start.line; i--)
//            {
//                curLine = _lines[i];

//                // Check for start and end lines
//                if (i == start.line) // start
//                {
//                    DeleteText(curLine, start.index, startLength);
//                }
//                else if (i == end.line) // end
//                {
//                    DeleteText(curLine, 0, end.index);
//                    MergeLine(start.line, end.line);
//                }
//                else
//                {
//                    RemoveLine(curLine);
//                }
//            }

//            end = start;
//        }

//        private void DeleteText(Line line, int pointA, int pointB)
//        {
//            if (pointA < pointB)
//            {
//                int len = pointB - pointA;
//                line.Text = line.Text.Remove(pointA, len);
//            }
//            else if (pointA > pointB)
//            {
//                int len = pointA - pointB;
//                line.Text = line.Text.Remove(pointB, len);
//            }
//        }

//        private string GetText(CursorLocation a, CursorLocation b)
//        {
//            string result = "";

//            if (a.line != b.line)
//            {
//                CursorLocation start, end;

//                if (a.line < b.line)
//                {
//                    start = a;
//                    end = b;
//                }
//                else
//                {
//                    start = b;
//                    end = a;
//                }

//                Line curLine = null;
//                for (int i = start.line; i <= end.line; i++)
//                {
//                    curLine = _lines[i];

//                    // Check for start and end lines
//                    if (i == start.line) // start
//                    {
//                        int len = curLine.Length - start.index;
//                        result += curLine.Text.Substring(start.index, len);
//                        result += Environment.NewLine;
//                    }
//                    else if (i == end.line) // end
//                    {
//                        result += curLine.Text.Substring(0, end.index);
//                    }
//                    else
//                    {
//                        result += curLine.Text;
//                        result += Environment.NewLine;
//                    }
//                }
//            }
//            else
//            {
//                if (a.index < b.index)
//                {
//                    int len = b.index - a.index;
//                    result = _lines[a.line].Text.Substring(a.index, len);
//                }
//                else if (a.index > b.index)
//                {
//                    int len = a.index - b.index;
//                    result = _lines[b.line].Text.Substring(b.index, len);
//                }
//            }

//            return result;
//        }

//        public void Cut()
//        {
//            Copy();
//            DeleteSelection();
//        }

//        public void Copy()
//        {
//            string str = GetText(_caret, _selection);
//            _ui.Engine.Clipboard.SetText(str);
//        }

//        public void Paste()
//        {
//            if (!_isEditable)
//                return;

//            if (_caret != _selection)
//                DeleteSelection();

//            if (_ui.Engine.Clipboard.ContainsText())
//                InsertText(_caret, _ui.Engine.Clipboard.GetText());
//        }

//        public void SelectAll()
//        {
//            _selection.line = 0;
//            _selection.index = 0;

//            int endLine = _lines.Count - 1;
//            _caret.line = endLine;
//            _caret.index = _lines[endLine].Length;

//            AlignText();
//            CheckSelection(false);
//        }

//        public void WriteLine(string text)
//        {
//            Line line = _lines[0];

//            if (!string.IsNullOrWhiteSpace(line.Text))
//            {
//                line = GetNewLine(_caret.line + 1);

//                _caret.line++;
//                _caret.index = 0;
//            }

//            line.Text += text;

//            AlignText();
//            CheckSelection(false);
//        }

//        public void Clear()
//        {
//            _lines.Clear();
//            GetNewLine();

//            _caret = new CursorLocation();
//            _selection = _caret;

//            AlignText();
//        }

//        private void InsertText(CursorLocation position, string text)
//        {
//            string[] delims = { Environment.NewLine, "\r", "\n" };
//            string[] pastedLines = text.Split(delims, StringSplitOptions.None);

//            int end = pastedLines.Length - 1;
//            string leftover = null;
//            Line curLine = _lines[position.line];

//            for (int i = 0; i < pastedLines.Length; i++)
//            {

//                if (i == 0)
//                {
//                    if (position.index != curLine.Length)
//                    {
//                        int len = curLine.Length - position.index;
//                        leftover = curLine.Text.Substring(position.index, len);
//                        curLine.Text = curLine.Text.Remove(position.index, len);
//                        curLine.Text = curLine.Text.Insert(position.index, pastedLines[i]);
//                    }
//                    else
//                    {
//                       curLine.Text += pastedLines[i];
//                    }
//                }
//                else
//                {
//                    Line newLine = GetNewLine(position.line + i);
//                    newLine.Text = pastedLines[i];
//                }
//            }

//            // Add the leftover bit of the first line to the end of the last line.
//            if (!string.IsNullOrWhiteSpace(leftover))
//            {
//                _lines[end].Text += leftover;
//            }

//            AlignText();
//            CheckSelection(false);
//        }

//        private void DoSystemKeyAction(Timing time, Action action)
//        {
//            _sysKeyTimer += time.ElapsedTime.TotalMilliseconds;

//            if (_sysKeyTimer >= REPEAT_DELAY && !_sysKeyHeld)
//            {
//                _sysKeyTimer -= REPEAT_DELAY;
//                _sysKeyHeld = true;
//                action();
//            }
//            else if (_sysKeyTimer >= REPEAT_INTERVAL && _sysKeyHeld)
//            {
//                _sysKeyTimer -= REPEAT_INTERVAL;
//                action();
//            }
//        }

//        private void DoPageKey(Timing time, int multiplier)
//        {
//            int pageHeight = _lineCapacity * (int)_lines[0].textObject.GetSize().Y;
//            pageHeight *= multiplier;

//            _sysKeyTimer += time.ElapsedTime.TotalMilliseconds;

//            if (_sysKeyTimer >= REPEAT_DELAY && !_sysKeyHeld)
//            {
//                _sysKeyTimer -= REPEAT_DELAY;
//                _sysKeyHeld = true;
//                _vBar.Scroll(pageHeight);
//            }
//            else if (_sysKeyTimer >= REPEAT_INTERVAL && _sysKeyHeld)
//            {
//                _sysKeyTimer -= REPEAT_INTERVAL;
//                _vBar.Scroll(pageHeight);
//            }
//        }

//        private void CenterOnCaret()
//        {
//            if (_caret.line < _startLine || _caret.line > _endLine)
//            {
//                int center = (int)Math.Ceiling((float)_lineCapacity / 2);
//                int lastLine = _lines.Count - 1;
//                _startLine = Math.Max(0, _caret.line - center);

//                _vBar.Value = _startLine * (int)_lines[0].textObject.GetSize().Y;
//            }
//        }

//        protected override void OnUpdate(Timing time)
//        {
//            base.OnUpdate(time);

//            if (_ui.Focused == this)
//            {
//                if (_ui.Engine.Input.Keyboard.IsTapped(Key.Left))
//                {
//                    GoBackwardAndAlign();
//                }
//                else if (_ui.Engine.Input.Keyboard.IsTapped(Key.Right))
//                {
//                    GoForwardAndAlign();
//                }
//                else if (_ui.Engine.Input.Keyboard.IsTapped(Key.Up))
//                {
//                    GoUpAndAlign();
//                }
//                else if (_ui.Engine.Input.Keyboard.IsTapped(Key.Down))
//                {
//                    GoDownAndAlign();
//                }
//                else if (_ui.Engine.Input.Keyboard.IsTapped(Key.PageUp))
//                {
//                    int pageHeight = _lineCapacity * (int)_lines[0].textObject.GetSize().Y;
//                    _vBar.Scroll(-pageHeight);
//                }
//                else if (_ui.Engine.Input.Keyboard.IsTapped(Key.PageDown))
//                {
//                    int pageHeight = _lineCapacity * (int)_lines[0].textObject.GetSize().Y;
//                    _vBar.Scroll(pageHeight);
//                }
//                else if (_ui.Engine.Input.Keyboard.IsPressed(Key.LeftControl) || _ui.Engine.Input.Keyboard.IsPressed(Key.RightControl))
//                {
//                    if (_ui.Engine.Input.Keyboard.IsTapped(Key.V))
//                        Paste();
//                    else if (_ui.Engine.Input.Keyboard.IsTapped(Key.C))
//                        Copy();
//                    else if (_ui.Engine.Input.Keyboard.IsTapped(Key.X))
//                        Cut();
//                    else if (_ui.Engine.Input.Keyboard.IsTapped(Key.A))
//                        SelectAll();
//                }
//                else if (_ui.Engine.Input.Keyboard.IsTapped(Key.Home))
//                {
//                    _caret.index = 0;
//                    AlignText();
//                    CheckSelection();
//                }
//                else if (_ui.Engine.Input.Keyboard.IsTapped(Key.End))
//                {
//                    _caret.index = _lines[_caret.line].Length;
//                    AlignText();
//                    CheckSelection();
//                }
//                else if (_ui.Engine.Input.Keyboard.IsTapped(Key.Delete))
//                {
//                    DeleteKey();
//                }
//                else if (_ui.Engine.Input.Keyboard.IsPressed(Key.Delete))
//                {
//                    DoSystemKeyAction(time, DeleteKey);
//                }
//                else if (_ui.Engine.Input.Keyboard.IsPressed(Key.Left))
//                {
//                    DoSystemKeyAction(time, GoBackwardAndAlign);
//                }
//                else if (_ui.Engine.Input.Keyboard.IsPressed(Key.Right))
//                {
//                    DoSystemKeyAction(time, GoForwardAndAlign);
//                }
//                else if (_ui.Engine.Input.Keyboard.IsPressed(Key.Up))
//                {
//                    DoSystemKeyAction(time, GoUpAndAlign);
//                }
//                else if (_ui.Engine.Input.Keyboard.IsPressed(Key.Down))
//                {
//                    DoSystemKeyAction(time, GoDownAndAlign);
//                }
//                else if (_ui.Engine.Input.Keyboard.IsPressed(Key.PageUp))
//                {
//                    DoPageKey(time, -1);
//                }
//                else if (_ui.Engine.Input.Keyboard.IsPressed(Key.PageDown))
//                {
//                    DoPageKey(time, 1);
//                }
//                else
//                {
//                    // Reset system key timing/triggers
//                    _sysKeyTimer = 0;
//                    _sysKeyHeld = false;
//                }

//                // Handle cursor blinking
//                if (_blinkCursor)
//                {
//                    _blinkTimer += time.ElapsedTime.TotalMilliseconds;
//                    if (_blinkTimer >= _blinkInterval)
//                    {
//                        _cursorVisible = !_cursorVisible;
//                        _blinkTimer -= _blinkInterval;
//                    }
//                }
//            }
//        }

//        protected override void OnRender(SpriteBatch sb, RenderProxy proxy)
//        {
//            base.OnRender(sb, proxy);

//            if (_ui.Focused == this)
//                sb.Draw(_globalBounds, _colorBorderFocused);
//            else
//                sb.Draw(_globalBounds, _colorBorderUnFocused);

//            sb.Draw(_clippingBounds, _bgColor);
//        }

//        protected override void OnRenderClipped(Graphics.SpriteBatch sb)
//        {
//            base.OnRenderClipped(sb);

//            if (_selection != _caret)
//            {
//                int selectStart = 0;
//                int selectEnd = 0;

//                if (_selection.line > _caret.line)
//                {
//                    selectStart = _caret.line;
//                    selectEnd = _selection.line;
//                }
//                else
//                {
//                    selectStart = _selection.line;
//                    selectEnd = _caret.line;
//                }

//                Line curLine = null;
//                for (int i = _startLine; i <= _endLine; i++)
//                {
//                    curLine = _lines[i];

//                    if(i >= selectStart && i <= selectEnd)
//                        sb.Draw(curLine.selectionBounds, _selectionColor);

//                    curLine.textObject.Draw(sb);
//                }
//            }
//            else
//            {
//                for (int i = _startLine; i <= _endLine; i++)
//                    _lines[i].textObject.Draw(sb);
//            }

//            // Draw caret
//            if (_cursorVisible && _ui.Focused == this && _isEditable)
//            {
//                Line curLine = _lines[_caret.line];

//                sb.DrawString(curLine.textObject.Font,
//                    "|",
//                    _cursorPos,
//                    _cursorColor);
//            }
//        }

//        protected override void OnUpdateBounds()
//        {
//            base.OnUpdateBounds();

//            int whitespace = BORDER_THICKNESS + SCROLLBAR_THICKNESS;
//            int whitespace2 = (BORDER_THICKNESS * 2) + SCROLLBAR_THICKNESS;

//            // Update scroll bars
//            _hBar.LocalBounds = new Rectangle()
//            {
//                X = _globalBounds.X + BORDER_THICKNESS,
//                Y = _globalBounds.Bottom - whitespace,
//                Width = _globalBounds.Width - whitespace2,
//                Height = SCROLLBAR_THICKNESS,
//            };

//            _vBar.LocalBounds = new Rectangle()
//            {
//                X = _globalBounds.Right - whitespace,
//                Y = _globalBounds.Y + BORDER_THICKNESS,
//                Width = SCROLLBAR_THICKNESS,
//                Height = _globalBounds.Height - whitespace2,
//            };

//            //_hBar.BarRange = _clippingBounds.Width;

//            AlignText();
//            CheckSelection(false);
//        }

//        protected override void OnDispose()
//        {
//            _ui.Engine.Input.Keyboard.OnCharacterKey -= Keyboard_OnCharacterKey;
//            base.OnDispose();
//        }

//        /// <summary>Gets or sets the string of text displayed in the text area.</summary>
//        [DataMember]
//        public string Text
//        {
//            get
//            {
//                int endLine = Math.Max(0, _lines.Count - 1);
//                int endIndex = _lines[endLine].Length;

//                return GetText(new CursorLocation()
//                    {
//                        index = 0,
//                        line = 0,
//                    },
//                    new CursorLocation()
//                    {
//                        index = endIndex,
//                        line = endLine,
//                    });
//            }

//            set
//            {
//                Clear();
//                InsertText(_caret, value);
//            }
//        }

//        [DataMember]
//        [Category("Appearance")]
//        [DisplayName("Background Color")]
//        public Color BackgroundColor
//        {
//            get { return _bgColor; }
//            set { _bgColor = value; }
//        }

//        [DataMember]
//        [Category("Appearance")]
//        [DisplayName("Focused Border Color")]
//        public Color FocusedBorderColor
//        {
//            get { return _colorBorderFocused; }
//            set { _colorBorderFocused = value; }
//        }

//        [DataMember]
//        [Category("Appearance")]
//        [DisplayName("unfocused Border Color")]
//        public Color UnfocusedBorderColor
//        {
//            get { return _colorBorderUnFocused; }
//            set { _colorBorderUnFocused = value; }
//        }

//        [DataMember]
//        [Category("Appearance")]
//        [DisplayName("Selection Color")]
//        public Color SelectionColor
//        {
//            get { return _selectionColor; }
//            set { _selectionColor = value; }
//        }

//        [DataMember]
//        [Category("Appearance")]
//        [DisplayName("Cursor Color")]
//        public Color CursorColor
//        {
//            get { return _cursorColor; }
//            set { _cursorColor = value; }
//        }

//        [DataMember]
//        [Category("Settings")]
//        [DisplayName("Blink Interval (ms)")]
//        public int BlinkInterval
//        {
//            get { return _blinkInterval; }
//            set { _blinkInterval = value; }
//        }

//        [DataMember]
//        [Category("Settings")]
//        [DisplayName("Is Editable")]
//        public bool IsEditable
//        {
//            get { return _isEditable; }
//            set { _isEditable = value; }
//        }

//        [DataMember]
//        [Category("Settings")]
//        [DisplayName("Blink Cursor")]
//        public bool BlinkCursor
//        {
//            get { return _blinkCursor; }
//            set
//            {
//                _blinkCursor = value;
//                if (!_blinkCursor)
//                    _cursorVisible = true;
//            }
//        }

//        [DataMember]
//        public int MaxLength
//        {
//            get { return _maxLength; }
//            set
//            {
//                _maxLength = value;
//                // TODO cull text
//            }
//        }
//    }
//}
