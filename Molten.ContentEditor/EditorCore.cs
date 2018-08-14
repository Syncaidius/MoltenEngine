using Molten;
using Molten.Graphics;
using Molten.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.ContentEditor
{
    public class EditorCore : Foundation
    {
        Scene _uiScene;
        UIMenuItem _menu;

        internal EditorCore(EngineSettings settings) : base("Molten Editor", settings)
        {

        }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            _uiScene = CreateScene("UI", SceneRenderFlags.Render2D);
            _uiScene.OutputCamera = new Camera2D();
            Window.OnPostResize += UpdateWindownBounds;
            UI = new UIComponent()
            {
                BackgroundColor = new Color(0),
            };
            UpdateWindownBounds(Window);

            _uiScene.AddObject(UI);

            // TODO set bounds of UI container to screen size.
            _menu = new UIMenuItem();
            _menu.Height = 25;
            _menu.Margin.DockLeft = true;
            _menu.Margin.DockRight = true;
            _menu.Label.Text = "";
            _menu.FlowDirection = UIMenuItem.ItemFlowDirection.LeftToRight;
            UI.AddChild(_menu);

            // Test some sub-items
            UIMenuItem mnuFile = new UIMenuItem();
            mnuFile.Label.Text = "File";
            mnuFile.BackgroundColor = new Color("#333337");
            _menu.AddChild(mnuFile);

            UIMenuItem mnuNew = new UIMenuItem();
            mnuNew.Label.Text = "New...";
            mnuNew.BackgroundColor = new Color("#333337");
            mnuFile.AddChild(mnuNew);

            UIMenuItem mnuOpen = new UIMenuItem();
            mnuOpen.Label.Text = "Open...";
            mnuOpen.BackgroundColor = new Color("#333337");
            mnuFile.AddChild(mnuOpen);

            UIMenuItem mnuExit = new UIMenuItem();
            mnuExit.Label.Text = "Exit";
            mnuExit.BackgroundColor = new Color("#333337");
            mnuFile.AddChild(mnuExit);

            UIMenuItem mnuEdit = new UIMenuItem();
            mnuEdit.Label.Text = "Edit";
            mnuEdit.BackgroundColor = new Color("#333337");
            _menu.AddChild(mnuEdit);

            //_menu.IsOpen = true;
            //mnuFile.ContextMenu.IsOpen = true;
        }

        private void UpdateWindownBounds(ITexture texture)
        {
            IWindowSurface window = texture as IWindowSurface;
            UI.LocalBounds = new Rectangle(0, 0, window.Width, window.Height);
        }

        protected override void OnUpdate(Timing time)
        {
           
        }

        /// <summary>
        /// Gets the root UI component which represents the main editor window.
        /// </summary>
        public UIComponent UI { get; private set; }
    }
}
