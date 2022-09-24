using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Data;
using Molten.Graphics;
using Molten.UI;

namespace Molten.Examples
{
    [Example("UI Demo", "Demonstrates the usage of various UI elements")]
    public class UIDemo : MoltenExample
    {
        ContentLoadHandle _hMaterial;
        ContentLoadHandle _hTexture;
        UIWindow _window1;
        UIWindow _window2;
        UIWindow _window3;
        UIWindow _window4;
        UILineGraph _lineGraph;
        UIButton _button1;
        UIButton _button2;
        UIButton _button3;
        UIButton _button4;
        UIButton _button5;
        UIButton _button6;
        UICheckBox _cbImmediate;
        UIStackPanel _stackPanel;
        UIListView _listView;
        UITextBox _textbox;

        GraphDataSet _graphSet;
        GraphDataSet _graphSet2;

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            base.OnLoadContent(loader);

            _hMaterial = loader.Load<IMaterial>("assets/BasicTexture.mfx");
            _hTexture = loader.Load<ITexture2D>("assets/logo_512_bc7.dds", parameters: new TextureParameters()
            {
                GenerateMipmaps = true,
            });

            loader.Deserialize<UITheme>("assets/test_theme.json", (theme, isReload) =>
            {
                UI.Root.Theme = theme;
            });

            loader.OnCompleted += Loader_OnCompleted;
        }

        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            if (!_hMaterial.HasAsset())
            {
                Close();
                return;
            }


            IMaterial mat = _hMaterial.Get<IMaterial>();
            ITexture2D texture = _hTexture.Get<ITexture2D>();

            mat.SetDefaultResource(texture, 0);
            TestMesh.Material = mat;

            _window1 = new UIWindow()
            {
                LocalBounds = new Rectangle(50, 150, 700, 440),
                Title = "Line Graph Test",
            };
            {
                _lineGraph = _window1.Children.Add<UILineGraph>(new Rectangle(0, 0, 700, 420));
                PlotGraphData(_lineGraph);
            }

            _window2 = new UIWindow()
            {
                LocalBounds = new Rectangle(760, 250, 640, 550),
                Title = "Button & Stack Panel Test",
            };
            {
                _button1 = _window2.Children.Add<UIButton>(new Rectangle(100, 100, 100, 30));
                _button1.Text = "Plot Data!";

                _button2 = _window2.Children.Add<UIButton>(new Rectangle(100, 140, 120, 30));
                _button2.Text = "Plot More Data!";

                _button3 = _window2.Children.Add<UIButton>(new Rectangle(100, 180, 180, 30));
                _button3.Text = "Close Other Window";

                _button4 = _window2.Children.Add<UIButton>(new Rectangle(100, 220, 180, 30));
                _button4.Text = "Open Other Window";

                _button5 = _window2.Children.Add<UIButton>(new Rectangle(100, 260, 180, 30));
                _button5.Text = "Minimize Other Window";

                _button6 = _window2.Children.Add<UIButton>(new Rectangle(100, 300, 180, 30));
                _button6.Text = "Maximize Other Window";

                _cbImmediate = _window2.Children.Add<UICheckBox>(new Rectangle(100, 340, 180, 25));
                _cbImmediate.Text = "Disable Animation";

                _button1.Pressed += _button1_Pressed;
                _button2.Pressed += _button2_Pressed;
                _button3.Pressed += _button3_Pressed;
                _button4.Pressed += _button4_Pressed;
                _button5.Pressed += _button5_Pressed;
                _button6.Pressed += _button6_Pressed;

                _stackPanel = _window2.Children.Add<UIStackPanel>(new Rectangle(300, 100, 300, 300));
                _stackPanel.Direction = UIElementFlowDirection.Vertical;
                {
                    // Add some items to the stack panel
                    UICheckBox lvCheckbox1 = _stackPanel.Children.Add<UICheckBox>(new Rectangle(0, 0, 150, 30));
                    lvCheckbox1.Text = "Check me out!";
                    UICheckBox lvCheckbox2 = _stackPanel.Children.Add<UICheckBox>(new Rectangle(0, 0, 150, 30));
                    lvCheckbox2.Text = "Don't forget about me!";
                    UILabel lvLabel1 = _stackPanel.Children.Add<UILabel>(new Rectangle(0, 0, 150, 30));
                    lvLabel1.Text = "I'm a label";
                    UIButton lvButton1 = _stackPanel.Children.Add<UIButton>(new Rectangle(0, 0, 150, 30));
                    lvButton1.Text = "I'm Button 1";
                    UIButton lvButton2 = _stackPanel.Children.Add<UIButton>(new Rectangle(0, 0, 150, 30));
                    lvButton2.Text = "I'm Button 2";
                    UIPanel lvPanel1 = _stackPanel.Children.Add<UIPanel>(new Rectangle(0, 0, 150, 80));
                    {
                        UILabel lvPanel1Label = lvPanel1.Children.Add<UILabel>(new Rectangle(0, 0, 150, 30));
                        lvPanel1Label.Text = "I'm panel label";
                        UIButton lvButton3 = _stackPanel.Children.Add<UIButton>(new Rectangle(0, 0, 150, 30));
                        lvButton3.Text = "I'm Button 3";
                        UIButton lvButton4 = _stackPanel.Children.Add<UIButton>(new Rectangle(0, 0, 150, 30));
                        lvButton4.Text = "I'm Button 4";
                        UIPanel lvPanel2 = _stackPanel.Children.Add<UIPanel>(new Rectangle(0, 0, 150, 80));
                        UILabel lvPanel2Label = lvPanel2.Children.Add<UILabel>(new Rectangle(0, 0, 150, 30));
                        lvPanel2Label.Text = "I'm panel label";
                    }
                }
            }

            _window3 = new UIWindow()
            {
                LocalBounds = new Rectangle(260, 450, 440, 450),
                Title = "List View Test",
            };
            {
                _listView = _window3.Children.Add<UIListView>(new Rectangle(0, 0, 200, 450));
                {
                    for (int i = 0; i < 10; i++)
                    {
                        UIListViewItem li = _listView.Children.Add<UIListViewItem>(new Rectangle(0, 0, 100, 30));
                        li.Text = $"List Item {i + 1}";
                    }
                }
            }

            UI.Children.Add(_window1);
            UI.Children.Add(_window2);
            UI.Children.Add(_window3);

            _window4 = UI.Children.Add<UIWindow>(new Rectangle(400, 250, 850, 700));
            {
                _window4.Title = "Textbox Test";
                _textbox = _window4.Children.Add<UITextBox>(new Rectangle(0, 0, 850, 670));
                _textbox.ShowLineNumbers = true;
                _textbox.Text = @"using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Molten
{
    /// <summary>
    /// Represents a four dimensional mathematical vector of bool (32 bits per bool value).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    [Serializable]
    public struct Bool4 : IEquatable<Bool4>, IFormattable
    {
        /// <summary>
        /// The size of the <see cref = ""Bool4"" /> type, in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Marshal.SizeOf(typeof (Bool4));

        /// <summary>
        /// A <see cref = ""Bool4"" /> with all of its components set to false.
        /// </summary>
        public static readonly Bool4 False = new Bool4();

        /// <summary>
        /// The X unit <see cref = ""Bool4"" /> (true, 0, 0, 0).
        /// </summary>
        public static readonly Bool4 UnitX = new Bool4(true, false, false, false);

        /// <summary>
        /// The Y unit <see cref = ""Bool4"" /> (0, true, 0, 0).
        /// </summary>
        public static readonly Bool4 UnitY = new Bool4(false, true, false, false);

        /// <summary>
        /// The Z unit <see cref = ""Bool4"" /> (0, 0, true, 0).
        /// </summary>
        public static readonly Bool4 UnitZ = new Bool4(false, false, true, false);

        /// <summary>
        /// The W unit <see cref = ""Bool4"" /> (0, 0, 0, true).
        /// </summary>
        public static readonly Bool4 UnitW = new Bool4(false, false, false, true);

        /// <summary>
        /// A <see cref = ""Bool4"" /> with all of its components set to true.
        /// </summary>
        public static readonly Bool4 One = new Bool4(true, true, true, true);

        /// <summary>
        /// The X component of the vector.
        /// </summary>
        [DataMember]
        internal int iX;

        /// <summary>
        /// The Y component of the vector.
        /// </summary>
        [DataMember]
        internal int iY;

        /// <summary>
        /// The Z component of the vector.
        /// </summary>
        [DataMember]
        internal int iZ;

        /// <summary>
        /// The W component of the vector.
        /// </summary>
        [DataMember]
        internal int iW;

        /// <summary>
        /// The X component of the vector.
        /// </summary>
        public bool X
        {
            get => iX != 0;
            set => iX = value ? 1 : 0;
        }

        /// <summary>
        /// The Y component of the vector.
        /// </summary>
        public bool Y
        {
            get => iY != 0;
            set => iY = value ? 1 : 0;
        }

        /// <summary>
        /// The Z component of the vector.
        /// </summary>
        public bool Z
        {
            get => iZ != 0;
            set => iZ = value ? 1 : 0;
        }

        /// <summary>
        /// The W component of the vector.
        /// </summary>
        public bool W
        {
            get => iW != 0;
            set => iW = value ? 1 : 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref = ""Bool4"" /> struct.
        /// </summary>
        /// <param name = ""value"">The value that will be assigned to all components.</param>
        public Bool4(bool value)
        {
            iX = value ? 1 : 0;
            iY = value ? 1 : 0;
            iZ = value ? 1 : 0;
            iW = value ? 1 : 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref = ""Bool4"" /> struct.
        /// </summary>
        /// <param name = ""x"">Initial value for the X component of the vector.</param>
        /// <param name = ""y"">Initial value for the Y component of the vector.</param>
        /// <param name = ""z"">Initial value for the Z component of the vector.</param>
        /// <param name = ""w"">Initial value for the W component of the vector.</param>
        public Bool4(bool x, bool y, bool z, bool w)
        {
            iX = x ? 1 : 0;
            iY = y ? 1 : 0;
            iZ = z ? 1 : 0;
            iW = w ? 1 : 0; 
        }


        /// <summary>
        /// Initializes a new instance of the <see cref = ""Bool4"" /> struct.
        /// </summary>
        /// <param name = ""values"">The values to assign to the X, Y, Z, and W components of the vector. This must be an array with four elements.</param>
        /// <exception cref = ""ArgumentNullException"">Thrown when <paramref name = ""values"" /> is <c>null</c>.</exception>
        /// <exception cref = ""ArgumentOutOfRangeException"">Thrown when <paramref name = ""values"" /> contains more or less than four elements.</exception>
        public Bool4(bool[] values)
        {
            if (values == null)
                throw new ArgumentNullException(""values"");
            if (values.Length != 4)
                throw new ArgumentOutOfRangeException(
                    ""values"",
                    ""There must be four and only four input values for Bool4."");

            iX = values[0] ? 1 : 0;
            iY = values[1] ? 1 : 0;
            iZ = values[2] ? 1 : 0;
            iW = values[3] ? 1 : 0;
        }

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X, Y, Z, or W component, depending on the index.</value>
        /// <param name = ""index"">The index of the component to access. Use 0 for the X component, 1 for the Y component, 2 for the Z component, and 3 for the W component.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref = ""System.ArgumentOutOfRangeException"">Thrown when the <paramref name = ""index"" /> is out of the range [0, 3].</exception>
        public bool this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                    case 3:
                        return W;
                }

                throw new ArgumentOutOfRangeException(""index"", ""Indices for Bool4 run from 0 to 3, inclusive."");
            }

            set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    case 2:
                        Z = value;
                        break;
                    case 3:
                        W = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(""index"", ""Indices for Bool4 run from 0 to 3, inclusive."");
                }
            }
        }

        /// <summary>
        /// Creates an array containing the elements of the vector.
        /// </summary>
        /// <returns>A four-element array containing the components of the vector.</returns>
        public bool[] ToArray()
        {
            return new bool[] {X, Y, Z, W};
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name = ""left"">The first value to compare.</param>
        /// <param name = ""right"">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name = ""left"" /> has the same value as <paramref name = ""right"" />; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Bool4 left, Bool4 right)
        {
            return left.Equals(ref right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name = ""left"">The first value to compare.</param>
        /// <param name = ""right"">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name = ""left"" /> has a different value than <paramref name = ""right"" />; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Bool4 left, Bool4 right)
        {
            return !left.Equals(ref right);
        }

        /// <summary>
        /// Returns a <see cref = ""System.String"" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref = ""System.String"" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, ""X:{0} Y:{1} Z:{2} W:{3}"", X, Y, Z, W);
        }

        /// <summary>
        /// Returns a <see cref=""System.String""/> that represents this instance.
        /// </summary>
        /// <param name=""format"">The format.</param>
        /// <param name=""formatProvider"">The format provider.</param>
        /// <returns>
        /// A <see cref=""System.String""/> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, format, X, Y, Z, W);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = iX;
                hashCode = (hashCode * 397) ^ iY;
                hashCode = (hashCode * 397) ^ iZ;
                hashCode = (hashCode * 397) ^ iW;
                return hashCode;
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref = ""Bool4"" /> is equal to this instance.
        /// </summary>
        /// <param name = ""other"">The <see cref = ""Bool4"" /> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref = ""Bool4"" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref Bool4 other)
        {
            return other.X == X && other.Y == Y && other.Z == Z && other.W == W;
        }

        /// <summary>
        /// Determines whether the specified <see cref = ""Bool4"" /> is equal to this instance.
        /// </summary>
        /// <param name = ""other"">The <see cref = ""Bool4"" /> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref = ""Bool4"" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Bool4 other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref = ""System.Object"" /> is equal to this instance.
        /// </summary>
        /// <param name = ""value"">The <see cref = ""System.Object"" /> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref = ""System.Object"" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (!(value is Bool4))
                return false;

            var strongValue = (Bool4)value;
            return Equals(ref strongValue);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref=""int""/> array to <see cref=""Bool4""/>.
        /// </summary>
        /// <param name=""input"">The input.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Bool4(bool[] input)
        {
            return new Bool4(input);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref=""Bool4""/> to <see cref=""System.Int32""/> array.
        /// </summary>
        /// <param name=""input"">The input.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator bool[](Bool4 input)
        {
            return input.ToArray();
        }
    }
}";
            }
        }


        private void _button1_Pressed(UIElement element, UIPointerTracker tracker)
        {
            _graphSet.Plot(Rng.Next(10, 450));
        }

        private void _button2_Pressed(UIElement element, UIPointerTracker tracker)
        {
            _graphSet2.Plot(Rng.Next(100, 300));
        }

        private void _button3_Pressed(UIElement element, UIPointerTracker tracker)
        {
            _window1.Close(_cbImmediate.IsChecked);
        }

        private void _button4_Pressed(UIElement element, UIPointerTracker tracker)
        {
            _window1.Open(_cbImmediate.IsChecked);
        }

        private void _button5_Pressed(UIElement element, UIPointerTracker tracker)
        {
            _window1.Minimize(_cbImmediate.IsChecked);
        }

        private void _button6_Pressed(UIElement element, UIPointerTracker tracker)
        {
            _window1.Maximize(_cbImmediate.IsChecked);
        }

        private void PlotGraphData(UILineGraph graph)
        {
            _graphSet = new GraphDataSet(200);
            _graphSet.KeyColor = Color.Grey;
            for (int i = 0; i < _graphSet.Capacity; i++)
                _graphSet.Plot(Rng.Next(0, 500));

            _graphSet2 = new GraphDataSet(200);
            _graphSet2.KeyColor = Color.Lime;
            float piInc = MathHelper.TwoPi / 20;
            float waveScale = 100;
            for (int i = 0; i < _graphSet2.Capacity; i++)
                _graphSet2.Plot(waveScale * Math.Sin(piInc * i));

            graph.AddDataSet(_graphSet);
            graph.AddDataSet(_graphSet2);
        }

        protected override IMesh GetTestCubeMesh()
        {
            IMesh<CubeArrayVertex> cube = Engine.Renderer.Resources.CreateMesh<CubeArrayVertex>(36);
            cube.SetVertices(SampleVertexData.TextureArrayCubeVertices);
            return cube;
        }

        protected override void OnDrawSprites(SpriteBatcher sb)
        {
            base.OnDrawSprites(sb);

            string text = $"Focused UI Element: {(UI.FocusedElement != null ? UI.FocusedElement.Name : "None")}";
            Vector2F tSize = Font.MeasureString(text);
            Vector2F pos = new Vector2F()
            {
                X = (Surface.Width / 2) - (tSize.X / 2),
                Y = 25,
            };

            sb.DrawString(Font, text, pos, Color.White);
        }
    }
}
