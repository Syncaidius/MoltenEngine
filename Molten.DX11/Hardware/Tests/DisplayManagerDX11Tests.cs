using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Hardware.Tests
{
    [TestClass]
    public class DisplayManagerDX11Tests
    {
        DisplayManagerDX11 _manager;

        [TestMethod]
        public void Initialize()
        {
            Init();
            Cleanup();
        }

        [TestMethod]
        public void GetAdapter()
        {
            Init();
            IDisplayAdapter adapter = _manager.GetAdapter(0);

            Cleanup();
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException), "Out of range adapter index was not handled correctly.")]
        public void GetAdapterIndexException()
        {
            Init();

            IDisplayAdapter adapter;
            int rangePlusOne = _manager.AdapterCount + 1;
            for (int i = 0; i < rangePlusOne; i++)
                adapter = _manager.GetAdapter(i);

            Cleanup();
        }

        [TestMethod]
        public void GetAdapters()
        {
            Init();

            List<IDisplayAdapter> adapters = new List<IDisplayAdapter>();
            _manager.GetAdapters(adapters);
            adapters.Clear();
            Cleanup();
        }

        [TestMethod]
        public void GetSelectedAdapter()
        {
            Init();
            IDisplayAdapter adapter = _manager.SelectedAdapter;
            Cleanup();
        }

        [TestMethod]
        public void SetSelectedAdapter()
        {
            Init();
            _manager.SelectedAdapter = _manager.SelectedAdapter;
            Cleanup();
        }

        [TestMethod]
        [ExpectedException(typeof(AdapterException), "Incorrectly handled selection of adapter owned by a different display manager.")]
        public void SetSelectedAdapterDifferentManager()
        {
            Init();

            DisplayManagerDX11 other = new DisplayManagerDX11();
            other.Initialize(Logger.Get(), new GraphicsSettings());
            _manager.SelectedAdapter = other.SelectedAdapter;
            other.Dispose();
            Cleanup();
        }

        private void Init()
        {
            Logger log = Logger.Get();
            GraphicsSettings settings = new GraphicsSettings();
            _manager = new DisplayManagerDX11();
            _manager.Initialize(log, settings);
        }

        private void Cleanup()
        {
            _manager.Dispose();
            Logger.DisposeAll();
            _manager = null;
        }
    }
}
