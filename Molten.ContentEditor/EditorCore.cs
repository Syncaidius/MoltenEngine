﻿using Molten;
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
        UIMenu _menu;
        UIPanel _leftPanel;

        internal EditorCore() : base("Molten Editor")
        {

        }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            //_uiScene = CreateScene("UI");
            //SceneObject camObj = CreateObject(_uiScene);
            //CameraComponent cam = camObj.AddComponent<CameraComponent>();
            //cam.Mode = RenderCameraMode.Orthographic;
            //cam.MaxDrawDistance = 1.0f;
            //_uiScene.AddObject(camObj);
            //Window.OnPostResize += UpdateWindownBounds;
            //UI = new UIComponent();
            //UpdateWindownBounds(Window);

            //_uiScene.AddObject(UI);

            //_leftPanel = new UIPanel();
            //_leftPanel.ClipPadding.Right = 1;
            //_leftPanel.Margin.SetDock(false, true, false, true);
            //_leftPanel.Width = 300;
            //_leftPanel.Y = 25;
            //UI.AddChild(_leftPanel);

            //// TODO set bounds of UI container to screen size.
            //_menu = new UIMenu();
            //_menu.Height = 25;
            //_menu.Margin.DockLeft = true;
            //_menu.Margin.DockRight = true;
            //_menu.ClipPadding.Bottom = 1;
            //UI.AddChild(_menu);

            //// Test some sub-items
            //UIMenuItem mnuFile = new UIMenuItem();
            //mnuFile.Text = "File";
            //_menu.AddChild(mnuFile);


            // OLD

            //UIMenuItem mnuNew = new UIMenuItem();
            //mnuNew.Label.Text = "New...";
            //mnuNew.BackgroundColor = new Color("#333337");
            //mnuFile.AddChild(mnuNew);

            //UIMenuItem mnuOpen = new UIMenuItem();
            //mnuOpen.Label.Text = "Open";
            //mnuOpen.BackgroundColor = new Color("#333337");
            //mnuFile.AddChild(mnuOpen);

            //UIMenuItem mnuProject = new UIMenuItem();
            //mnuProject.Label.Text = "Project...";
            //mnuProject.BackgroundColor = new Color("#333337");
            //mnuOpen.AddChild(mnuProject);

            //UIMenuItem mnuOpenFile = new UIMenuItem();
            //mnuOpenFile.Label.Text = "File...";
            //mnuOpenFile.BackgroundColor = new Color("#333337");
            //mnuOpen.AddChild(mnuOpenFile);

            //UIMenuItem mnuExit = new UIMenuItem();
            //mnuExit.Label.Text = "Exit";
            //mnuExit.BackgroundColor = new Color("#333337");
            //mnuFile.AddChild(mnuExit);

            UIMenuItem mnuEdit = new UIMenuItem();
            mnuEdit.Text = "Edit";
            _menu.AddChild(mnuEdit);

        }

        private void UpdateWindownBounds(ITexture texture)
        {
            INativeSurface window = texture as INativeSurface;
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
