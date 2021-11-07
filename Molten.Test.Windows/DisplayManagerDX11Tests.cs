//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Molten.Graphics.Hardware.Tests
//{
//    [TestClass]
//    public class DisplayManagerDX11Tests
//    {
//        DisplayManagerDX11 _manager;

//        [TestInitialize]
//        public void Init()
//        {
//            _manager = new DisplayManagerDX11();

//            Logger log = Logger.Get();
//            GraphicsSettings settings = new GraphicsSettings();
//            _manager.Initialize(log, settings);
//        }

//        [TestCleanup]
//        public void Cleanup()
//        {
//            _manager.Dispose();
//            Logger.DisposeAll();
//        }

//        [TestMethod]
//        public void GetAdapter()
//        {
//            IDisplayAdapter adapter = _manager.GetAdapter(0);
//        }

//        [TestMethod]
//        [ExpectedException(typeof(IndexOutOfRangeException), "Out of range adapter index was not handled correctly.")]
//        public void GetAdapterIndexException()
//        {
//            IDisplayAdapter adapter;
//            int rangePlusOne = _manager.AdapterCount + 1;
//            for (int i = 0; i < rangePlusOne; i++)
//                adapter = _manager.GetAdapter(i);
//        }

//        [TestMethod]
//        public void GetAdapters()
//        {
//            List<IDisplayAdapter> adapters = new List<IDisplayAdapter>();
//            _manager.GetAdapters(adapters);
//            adapters.Clear();
//        }

//        [TestMethod]
//        public void GetSelectedAdapter()
//        {
//            IDisplayAdapter adapter = _manager.SelectedAdapter;
//        }

//        [TestMethod]
//        public void SetSelectedAdapter()
//        {
//            _manager.SelectedAdapter = _manager.SelectedAdapter;
//        }

//        [TestMethod]
//        [ExpectedException(typeof(AdapterException), "Incorrectly handled adapter selection owned by a different display manager.")]
//        public void SetSelectedAdapterDifferentManager()
//        {
//            DisplayManagerDX11 other = new DisplayManagerDX11();
//            other.Initialize(Logger.Get(), new GraphicsSettings());
//            _manager.SelectedAdapter = other.SelectedAdapter;
//            other.Dispose();
//        }
//    }
//}
