using Molten.Graphics;
using Molten.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    public class UIPieChart : UIComponent
    {
        public struct GraphElement
        {
            public string Name;
            public string KeyText;
            public float Value;
            public float StartAngle;
            public float EndAngle;
            public Vector2 CenterOffset;
            public Color Color;

            public int Points;
        }

        Matrix _mIdentity = Matrix.Identity;

        GraphElement[] _parts;
        int _partCount;
        bool _isDirty;
        bool _showKeys;

        Dictionary<string, int> _lookup;

        float _radius;
        float _total;
        PrimitiveBatch _pb;
        SpriteFont _font;

        string _title = "";
        string _fontName;
        int _maxPoints = 100;
        int _fontSize;

        static string _strKey = "{0} ({1}%)";

        public UIPieChart(UISystem ui)
            : base(ui)
        {
            _lookup = new Dictionary<string, int>();
            _parts = new GraphElement[1];
            _parts[0] = new GraphElement()
            {
                Color = Color.White,
                StartAngle = 0,
                EndAngle = 360,
            };

            _fontName = _ui.DefaultFontName;
            _fontSize = _ui.DefaultFontSize;

            _showKeys = true;
            Initialize(true);
        }

        private void Initialize(bool initBatch = false)
        {
            if (initBatch)
            {
                if (_pb != null)
                    _pb.Dispose();

                _pb = new PrimitiveBatch(_ui.Engine, PrimitiveBatchTopology.TriangleList, _maxPoints * 2, 256);
            }

            _font = SpriteFont.Create(_ui.Engine.GraphicsDevice, _fontName, _fontSize);
            _isDirty = true;
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();

            _radius = Math.Max(_clippingBounds.Width, _clippingBounds.Height);
        }

        public void SetValue(string key, float value, Color color)
        {
            int id = 0;

            if (_lookup.TryGetValue(key, out id) == false)
            {
                id = _partCount++;
                if (id >= _parts.Length)
                    Array.Resize(ref _parts, _parts.Length + 2);

                _lookup.Add(key, id);
                _parts[id].Name = key;
            }

            // Remove old value from total and add new
            _total -= _parts[id].Value;
            _total += value;

            // Update part entry
            _parts[id].Value = value;
            _parts[id].Color = color;

            _isDirty = true;
        }

        public void RemoveValue(string key)
        {
            int id = 0;

            if (_lookup.TryGetValue(key, out id))
            {
                // Remove value from total
                _total -= _parts[id].Value;

                _parts[id].Value = 0;
                _parts[id].Name = null;
            }
        }

        protected override void OnUpdate(Schedule time)
        {
            if (_isDirty)
            {
                _isDirty = false;

                Vector2 center = _clippingBounds.Center;

                // Update segments
                float prevAngle = 0;
                for (int i = 0; i < _partCount; i++)
                {
                    if (_parts[i].Name == null)
                        continue;

                    float percent = _parts[i].Value / _total;
                    float angle = 360 * percent;
                    float points = Math.Max(2, _maxPoints * percent); //Need at least 2 (+ center) to form a segment

                    _parts[i].StartAngle = prevAngle;
                    _parts[i].EndAngle = prevAngle + angle;

                    float halfRange = (_parts[i].EndAngle - _parts[i].StartAngle) / 2;
                    float radAngle = MathHelper.DegreesToRadians(_parts[i].StartAngle + halfRange);
                    float quarterRadius = _radius / 1.5f;

                    _parts[i].Points = (int)Math.Ceiling(points);
                    _parts[i].CenterOffset = new Vector2()
                    {
                        X = (float)Math.Sin(radAngle) * quarterRadius,
                        Y = (float)Math.Cos(radAngle) * quarterRadius,
                    };
                    _parts[i].CenterOffset += center;

                    _parts[i].KeyText = string.Format(_strKey, _parts[i].Name, (percent * 100).ToString("N0"));

                    prevAngle = _parts[i].EndAngle;
                }
            }
        }

        protected override void OnRender(SpriteBatch sb, RenderProxy proxy)
        {
            Vector2 center = _clippingBounds.Center;

            for (int i = 0; i < _partCount; i++)
            {
                if (_parts[i].Name == null)
                    continue;

                _pb.AddCircle(center, _radius, 
                    _parts[i].Points, 
                    _parts[i].StartAngle, 
                    _parts[i].EndAngle, 
                    _parts[i].Color);

                if (_showKeys)
                    sb.DrawString(_font, _parts[i].KeyText, _parts[i].CenterOffset, Color.White);
            }

            proxy.DrawBatch(_pb, _mIdentity, BlendingPreset.Default, DepthStencilPreset.ZDisabled, RasterizerPreset.Default);
        }

        [DataMember]
        /// <summary>Gets or sets the radius of the pie graph.</summary>
        public float Radius
        {
            get { return _radius; }
            set { _radius = value; }
        }

        [DataMember]
        /// <summary>Gets or sets whether or not keys (and their percentage) are shown.</summary>
        public bool ShowKeys
        {
            get { return _showKeys; }
            set { _showKeys = value; }
        }

        [DataMember]
        public int MaxPoints
        {
            get { return _maxPoints; }
            set
            {
                _maxPoints = value;
                Initialize(true);
            }
        }

        [DataMember]
        public string Font
        {
            get { return _font.FontName; }
            set
            {
                _fontName = value;
                Initialize();
            }
        }

        [DataMember]
        public int FontSize
        {
            get { return _fontSize; }
            set
            {
                _fontSize = value;
                Initialize();
            }
        }

        [DataMember]
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }
    }
}
