using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
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
            Console.WriteLine("Molten Engine Test Framework.");

            GetTests();

            lstTests.MouseDoubleClick += lstTests_MouseDoubleClick;
            lstTests.SelectionChanged += lstTests_SelectionChanged;

            LoadLastRun();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _curTest?.Exit();
            Application.Current?.Shutdown();
            base.OnClosing(e);
        }

        private void SaveLastRun()
        {
            SetLastTestText(lastTestType);

            using (FileStream stream = new FileStream("test.cfg", FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    string typeName = ReflectionHelper.GetQualifiedTypeName(lastTestType);
                    writer.Write(typeName);
                    writer.Write(chkDebugLayer.IsChecked.Value);
                    writer.Write(chkVsync.IsChecked.Value);
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
                        lastTestType = Type.GetType(typeName);
                        SetLastTestText(lastTestType);
                    }
                    catch
                    {
                        lastTestType = null; //just to be sure...
                        chkDebugLayer.IsChecked = false;
                        chkVsync.IsChecked = false;
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
            List<Type> testTypes = ReflectionHelper.FindType<SampleGame>().ToList();

            for (int i = 0; i < testTypes.Count; i++)
            {
                SampleGame test = Activator.CreateInstance(testTypes[i]) as SampleGame;
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
            // Save type of test as the last run type.
            SaveLastRun();

            // Disable list view and enable again after the test ends.
            lstTests.IsEnabled = false;
            StartSample(lastTestType);

            
            GC.Collect();
            lstTests.IsEnabled = true;
        }

        private void StartSample(Type t)
        {
            EngineSettings settings = new EngineSettings();
            settings.Graphics.EnableDebugLayer.Value = chkDebugLayer.IsChecked.Value;
            settings.Graphics.VSync.Value = chkVsync.IsChecked.Value;
            settings.UseGuiControl = chkUseControl.IsChecked.Value;

            _curTest = Activator.CreateInstance(lastTestType) as SampleGame;
            _curTest.Start(settings, true);
        }
    }
}
