using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Molten.Samples
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class TestEntry
        {
            public Type TestType;
            public string TestName;
            public string Description;

            public override string ToString()
            {
                return TestName;
            }
        }

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(System.IntPtr hWnd, int cmdShow);

        List<TestEntry> testEntries = new List<TestEntry>();
        SampleGame _curTest;
        ConsoleColor defaultCol = Console.ForegroundColor;

        Type lastTestType;

        public MainWindow()
        {
            InitializeComponent();
            Console.ForegroundColor = ConsoleColor.White;

            //a hack for maximizing the console window
            Process p = Process.GetCurrentProcess();
            ShowWindow(p.MainWindowHandle, 3); //SW_MAXIMIZE = 3
            Console.WriteLine("Stone Bolt Engine Test Framework.");

            GetTests();

            lstTests.MouseDoubleClick += lstTests_MouseDoubleClick;
            lstTests.SelectionChanged += lstTests_SelectionChanged;

            cboRenderer.Items.Add(new RendererComboItem()
            {
                Content = "DirectX 11",
                LibraryName = GraphicsSettings.RENDERER_DX11,
            });

            cboRenderer.Items.Add(new RendererComboItem()
            {
                Content = "OpenGL",
                LibraryName = GraphicsSettings.RENDERER_OPENGL,
            });

            if (cboRenderer.SelectedItem == null)
                cboRenderer.SelectedIndex = 0;

            LoadLastRun();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _curTest?.Exit();
            Application.Current.Shutdown();
            base.OnClosing(e);
        }

        private void SaveLastRun()
        {
            SetLastTestText(lastTestType);

            using (FileStream stream = new FileStream("test.cfg", FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    string typeName = TestHelper.GetQualifiedTypeName(lastTestType);
                    writer.Write(typeName);
                    writer.Write(chkDebugLayer.IsChecked.Value);
                    writer.Write(chkVsync.IsChecked.Value);
                    writer.Write(cboRenderer.SelectedIndex);
                }
            }
        }

        private void LoadLastRun()
        {
            if (File.Exists("test.cfg") == false)
                return;

            using (FileStream stream = new FileStream("test.cfg", FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    string typeName = reader.ReadString();

                    try
                    {
                        chkDebugLayer.IsChecked = reader.ReadBoolean();
                        chkVsync.IsChecked = reader.ReadBoolean();
                        cboRenderer.SelectedIndex = reader.ReadInt32();
                        lastTestType = Type.GetType(typeName);
                        SetLastTestText(lastTestType);
                    }
                    catch
                    {
                        lastTestType = null; //just to be sure...
                        chkDebugLayer.IsChecked = false;
                        chkVsync.IsChecked = false;
                        cboRenderer.SelectedIndex = 0;
                    }
                }
            }

            //enable last test button if possible.
            btnLastTest.IsEnabled = lastTestType != null;
        }

        private void SetLastTestText(Type t)
        {
            foreach (TestEntry entry in testEntries)
            {
                if (t == entry.TestType)
                {
                    lblLastTestName.Content = "(" + entry.TestName.Replace("Test", "").Replace("test", "") + ")";
                    break;
                }
            }
        }

        void lstTests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object selected = lstTests.SelectedItem;

            if (selected != null)
            {
                TestEntry entry = selected as TestEntry;

                txtDesc.Text = entry.Description;
            }

            //enable last test button if possible.
            btnLastTest.IsEnabled = lastTestType != null;
        }

        void lstTests_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object selected = lstTests.SelectedItem;

            if (selected != null)
            {
                TestEntry entry = selected as TestEntry;
                lastTestType = entry.TestType;

                btnLastTest.IsEnabled = false;
                btnLastTest_Click(btnLastTest, new RoutedEventArgs());
                btnLastTest.IsEnabled = lastTestType != null;

                //this code is run after the test closes
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Test '" + entry.TestName + "' ended.");
                Console.ForegroundColor = defaultCol;
            }
        }

        private void GetTests()
        {
            List<Type> testTypes = TestHelper.FindType<SampleGame>().ToList();

            for (int i = 0; i < testTypes.Count; i++)
            {
                if (testTypes[i].IsAbstract)
                    continue;

                EngineSettings settings = new EngineSettings();
                settings.Graphics.EnableDebugLayer.Value = chkDebugLayer.IsChecked.Value;
                SampleGame test = Activator.CreateInstance(testTypes[i], settings) as SampleGame;
                TestEntry entry = new TestEntry()
                {
                    TestName = test.Title,
                    TestType = testTypes[i],
                    Description = test.Description,
                };

                testEntries.Add(entry);
            }

            //sort entries
            testEntries = testEntries.OrderBy(o => o.TestName).ToList();

            foreach (TestEntry entry in testEntries)
                lstTests.Items.Add(entry);
        }

        private void btnLastTest_Click(object sender, RoutedEventArgs e)
        {
            //save type of test as the last run type.
            SaveLastRun();

            RendererComboItem renderItem = cboRenderer.SelectedItem as RendererComboItem;

            //disable list view and enable again after the test ends.
            lstTests.IsEnabled = false;
            EngineSettings settings = new EngineSettings();
            settings.Graphics.EnableDebugLayer.Value = chkDebugLayer.IsChecked.Value;
            settings.Graphics.VSync.Value = chkVsync.IsChecked.Value;
            settings.Graphics.RendererLibrary.Value = renderItem.LibraryName;
            settings.UseGuiControl = chkUseControl.IsChecked.Value;

            _curTest = Activator.CreateInstance(lastTestType, settings) as SampleGame;
            _curTest.Start();

            GC.Collect();
            lstTests.IsEnabled = true;
        }

        class RendererComboItem : ComboBoxItem
        {
            public string LibraryName;

            public override string ToString()
            {
                return Content.ToString();
            }
        }
    }
}
