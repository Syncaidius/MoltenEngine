//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using SharpDX.Direct3D11;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Molten.Graphics.Hardware.Tests
//{
//    [TestClass]
//    public class GraphicsDeviceDX11Tests
//    {
//        DisplayManagerDX11 _manager;
//        DeviceDX11 _device;

//        [TestInitialize]
//        public void TestInit()
//        {
//            Logger log = Logger.Get();
//            GraphicsSettings settings = new GraphicsSettings();
//            _manager = new DisplayManagerDX11();
//            _manager.Initialize(log, settings);
//            _device = new DeviceDX11(log, settings, _manager, false);
//        }

//        [TestCleanup]
//        public void Cleanup()
//        {
//            _manager.Dispose();
//            _device.Dispose();
//            Logger.DisposeAll();
//            _manager = null;
//            _device = null;
//        }

//        [TestMethod]
//        public void ImmediateContextNotNull()
//        {
//            DeviceContext context = _device.Context;
//            Assert.AreNotEqual(null, context, "Device was not correctly initialized");
//        }

//        [TestMethod]
//        public void DeferredContextAddRemove()
//        {
//            PipeDX11 pipe = _device.GetDeferredPipe();
//            _device.RemoveDeferredPipe(pipe);

//            Assert.AreEqual(true, pipe.IsDisposed, "Graphics pipe was not removed after disposal");            
//        }

//        [TestMethod]
//        [ExpectedException(typeof(GraphicsContextException), "Incorrectly handled graphics pipe from different device")]
//        public void DeferredContextOwnership()
//        {
//            DeviceDX11 otherDevice = new DeviceDX11(Logger.Get(), new GraphicsSettings(), _manager, false);
//            PipeDX11 otherPipe = otherDevice.GetDeferredPipe();

//            _device.RemoveDeferredPipe(otherPipe);

//            otherPipe.Dispose();
//            otherDevice.Dispose();
//        }

//        [TestMethod]
//        public void DeferredPipeDisposal()
//        {
//            PipeDX11 pipe = _device.GetDeferredPipe();
//            pipe.Dispose();

//            if (_device.ActivePipes.Contains(pipe))
//                Assert.Fail("Disposed graphics pipe is still in active list");
//        }

//        public void GetRecycleBufferSegment()
//        {
//            BufferSegment seg = _device.GetBufferSegment();
//            _device.RecycleBufferSegment(seg);
//        }

//        [TestMethod]
//        public void AllocationTest()
//        {
//            long toAllocate = 100;
//            _device.AllocateVRAM(toAllocate);
//            Assert.AreEqual(toAllocate, _device.AllocatedVRAM, "Allocated VRAM is incorrect");
//            _device.DeallocateVRAM(toAllocate);
//            Assert.AreEqual(0, _device.AllocatedVRAM, "Deallocated VRAM is incorrect");
//        }
//    }
//}
