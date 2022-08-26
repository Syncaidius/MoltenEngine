﻿using Molten.Data;
using Molten.Graphics;
using Molten.UI;

namespace Molten.Samples
{
    public class UIExample : SampleGame
    {
        ContentLoadHandle _hMaterial;
        ContentLoadHandle _hTexture;
        UIWindow _window1;
        UIWindow _window2;
        UILineGraph _lineGraph;
        UIButton _button1;
        UIButton _button2;
        UIButton _button3;
        UIButton _button4;
        UIButton _button5;
        UIButton _button6;
        UICheckBox _cbImmediate;
        UIStackPanel _stackPanel;

        GraphDataSet _graphSet;
        GraphDataSet _graphSet2;

        public override string Description => "Demonstrates Molten's UI system.";
        public UIExample() : base("UI Example") { }

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            _hMaterial = loader.Load<IMaterial>("assets/BasicTexture.mfx");
            _hTexture = loader.Load<ITexture2D>("assets/dds_test.dds", parameters: new TextureParameters()
            {
                GenerateMipmaps = true,
            });

            loader.Deserialize<UITheme>("assets/test_theme.json",(theme, isReload) =>
            {
                UI.Root.Theme = theme;
            });

            loader.OnCompleted += Loader_OnCompleted;
        }

        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            if (!_hMaterial.HasAsset())
            {
                Exit();
                return;
            }

            IMaterial mat = _hMaterial.Get<IMaterial>();
            ITexture2D texture = _hTexture.Get<ITexture2D>();

            mat.SetDefaultResource(texture, 0);
            TestMesh.Material = mat;

            _window1 = new UIWindow()
            {
                LocalBounds = new Rectangle(100, 150, 700, 440),
                Title = "This is a Window",
                //ShowDebugBounds = true,
            };

            _window2 = new UIWindow()
            {
                LocalBounds = new Rectangle(660, 250, 640, 550),
                Title = "This is another Window",
                //ShowDebugBounds = true
            };

            _button1 = new UIButton()
            {
                LocalBounds = new Rectangle(100, 100, 100, 30),
                Text = "Plot Data!"
            };

            _button2 = new UIButton()
            {
                LocalBounds = new Rectangle(100, 140, 120, 30),
                Text = "Plot More Data!"
            };

            _button3 = new UIButton()
            {
                LocalBounds = new Rectangle(100, 180, 180, 30),
                Text = "Close Other Window"
            };

            _button4 = new UIButton()
            {
                LocalBounds = new Rectangle(100, 220, 180, 30),
                Text = "Open Other Window"
            };

            _button5 = new UIButton()
            {
                LocalBounds = new Rectangle(100, 260, 180, 30),
                Text = "Minimize Other Window"
            };

            _button6 = new UIButton()
            {
                LocalBounds = new Rectangle(100, 300, 180, 30),
                Text = "Maximize Other Window"
            };

            _cbImmediate = new UICheckBox()
            {
                LocalBounds = new Rectangle(100, 340, 180, 25),
                Text = "Disable Animation"
            };

            _stackPanel = new UIStackPanel()
            {
                LocalBounds = new Rectangle(300, 100, 300, 300),
                Direction = UIElementFlowDirection.Vertical
            };

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
            UILabel lvPanel1Label = lvPanel1.Children.Add<UILabel>(new Rectangle(0, 0, 150, 30));
            lvPanel1Label.Text = "I'm panel label";
            UIButton lvButton3 = _stackPanel.Children.Add<UIButton>(new Rectangle(0, 0, 150, 30));
            lvButton3.Text = "I'm Button 3";
            UIButton lvButton4 = _stackPanel.Children.Add<UIButton>(new Rectangle(0, 0, 150, 30));
            lvButton4.Text = "I'm Button 4";
            UIPanel lvPanel2 = _stackPanel.Children.Add<UIPanel>(new Rectangle(0, 0, 150, 80));
            UILabel lvPanel2Label = lvPanel2.Children.Add<UILabel>(new Rectangle(0, 0, 150, 30));
            lvPanel2Label.Text = "I'm panel label";

            _lineGraph = new UILineGraph()
            {
                LocalBounds = new Rectangle(0, 0, 700, 420)
            };

            PlotGraphData(_lineGraph);

            UI.Children.Add(_window1);
            UI.Children.Add(_window2);
            _window2.Children.Add(_button1);
            _window2.Children.Add(_button2);
            _window2.Children.Add(_button3);
            _window2.Children.Add(_button4);
            _window2.Children.Add(_button5);
            _window2.Children.Add(_button6);
            _window2.Children.Add(_cbImmediate);
            _window2.Children.Add(_stackPanel);
            _window1.Children.Add(_lineGraph);

            _button1.Pressed += _button1_Pressed;
            _button2.Pressed += _button2_Pressed;
            _button3.Pressed += _button3_Pressed;
            _button4.Pressed += _button4_Pressed;
            _button5.Pressed += _button5_Pressed;
            _button6.Pressed += _button6_Pressed;
        }

        private void _button1_Pressed(UIElement element, ScenePointerTracker tracker)
        {
            _graphSet.Plot(Rng.Next(10, 450));
        }

        private void _button2_Pressed(UIElement element, ScenePointerTracker tracker)
        {
            _graphSet2.Plot(Rng.Next(100, 300));
        }

        private void _button3_Pressed(UIElement element, ScenePointerTracker tracker)
        {
            _window1.Close(_cbImmediate.IsChecked);
        }

        private void _button4_Pressed(UIElement element, ScenePointerTracker tracker)
        {
            _window1.Open(_cbImmediate.IsChecked);
        }

        private void _button5_Pressed(UIElement element, ScenePointerTracker tracker)
        {
            _window1.Minimize(_cbImmediate.IsChecked);
        }

        private void _button6_Pressed(UIElement element, ScenePointerTracker tracker)
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

        protected override void OnDrawSprites(SpriteBatcher sb)
        {
            if (SampleFont == null)
                return;

            base.OnDrawSprites(sb);

            string text = $"Hovered UI Element: {(UI.HoverElement != null ? UI.HoverElement.Name : "None")}";
            Vector2F tSize = SampleFont.MeasureString(text);
            Vector2F pos = new Vector2F()
            {
                X = (Window.Width / 2) - (tSize.X / 2),
                Y = 25,
            };

            sb.DrawString(SampleFont, text, pos, Color.White);
        }
    }
}
