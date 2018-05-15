using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Molten.Tests
{
    [TestClass]
    public class ContentManagerTests
    {
        [DataContract]
        class TestObject
        {
            [DataMember]
            public int TestProperty1 { get; set; } = 5;

            [DataMember]
            public string TestProperty2 { get; set; } = "testing";
        }

        Engine _engine;
        bool _done;

        [TestInitialize]
        public void TestInit()
        {
            _engine = new Engine();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _engine.Dispose();
        }

        [TestMethod]
        public void Serialize()
        {
            ContentRequest cr = _engine.Content.BeginRequest("tests");
            cr.Serialize("test_object.txt", new TestObject());
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            while (!_done)
                Thread.Sleep(5);
        }

        private void Cr_OnCompleted(ContentRequest request)
        {
            _done = true;
        }
    }
}
