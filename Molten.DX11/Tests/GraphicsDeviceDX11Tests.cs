using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX.Direct3D11;
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
        public void Cleanup()
        {
            _manager.Dispose();
            _device.Dispose();
            Logger.DisposeAll();
            _manager = null;
            _device = null;
        }

        [TestMethod]
        public void ImmediateContextNotNull()
        {
            DeviceContext context = _device.Context;
            Assert.AreNotEqual(null, context, "Device was not correctly initialized");
        }

        [TestMethod]
        public void DeferredContextAddRemove()
        {
            GraphicsPipe pipe = _device.GetDeferredPipe();
            _device.RemoveDeferredPipe(pipe);

            Assert.AreEqual(true, pipe.IsDisposed, "Graphics pipe was not removed after disposal");            
        }

        [TestMethod]
        public void DeferredContextOwnership()
        {
            GraphicsDeviceDX11 otherDevice = new GraphicsDeviceDX11(Logger.Get(), new GraphicsSettings(), new RenderProfiler(), _manager, false);
            GraphicsPipe pipe = otherDevice.GetDeferredPipe();
        }

        public void GetRecycleBufferSegment()
        {
            BufferSegment seg = _device.GetBufferSegment();
            _device.RecycleBufferSegment(seg);
        }

        [TestMethod]
        public void AllocationTest()
        {
            long toAllocate = 100;
            _device.AllocateVRAM(toAllocate);
            Assert.AreEqual(toAllocate, _device.AllocatedVRAM, "Allocated VRAM is incorrect");
            _device.DeallocateVRAM(toAllocate);
            Assert.AreEqual(0, _device.AllocatedVRAM, "Deallocated VRAM is incorrect");
        }
    }
}
