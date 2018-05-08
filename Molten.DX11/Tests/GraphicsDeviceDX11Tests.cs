using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Hardware.Tests
{
    [TestClass]
    public class GraphicsDeviceDX11Tests
    {
        DisplayManagerDX11 _manager;
        GraphicsDeviceDX11 _device;

        [TestInitialize]
        public void TestInit()
        {
            Logger log = Logger.Get();
            GraphicsSettings settings = new GraphicsSettings();
            _manager = new DisplayManagerDX11();
            _manager.Initialize(log, settings);
            _device = new GraphicsDeviceDX11(log, settings, new RenderProfiler(), _manager, false);
        }

        [TestCleanup]
        private void Cleanup()
        {
            _manager.Dispose();
            _device.Dispose();
            Logger.DisposeAll();
            _manager = null;
            _device = null;
        }
    }
}
